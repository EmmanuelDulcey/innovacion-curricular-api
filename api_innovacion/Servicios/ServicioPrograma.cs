using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Servicio de negocio de `programa` (v2), el maestro central del bloque.
///
/// Aqui viven las tres responsabilidades que la arquitectura le asigna
/// (3_plan.md §7): las reglas de negocio (PATCH vacio -> 400; el maestro con
/// hijos activos no se retira -> 409), la combinacion del PATCH (lee la fila
/// actual y encima aplica solo los campos enviados) y la traduccion del error
/// del motor a error de dominio.
///
/// La traduccion de SqlException: SOLO los errores de integridad se vuelven
/// ConflictoExcepcion (409); cualquier otro numero se relanza como SqlException
/// para que el controlador responda 500 (contrato §0.6: el 500 se reserva para
/// lo inesperado). Los numeros 50101-50105 los lanza THROW desde el
/// procedimiento cuando una FK no existe o esta inactiva (la restriccion de la
/// base no detecta una FK retirada).
/// </summary>
public class ServicioPrograma : IServicioPrograma
{
    private readonly IRepositorioPrograma _repositorio;

    public ServicioPrograma(IRepositorioPrograma repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<List<Programa>> ObtenerTodosAsync(int limite)
    {
        return _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Programa> ObtenerPorIdAsync(int id)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(id);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return registro;
    }

    public async Task<Programa> CrearAsync(ProgramaCrear peticion)
    {
        var entidad = new Programa
        {
            Id = peticion.Id!.Value,
            Nombre = peticion.Nombre!,
            Tipo = peticion.Tipo!,
            Nivel = peticion.Nivel!,
            FechaCreacion = peticion.FechaCreacion!,
            FechaCierre = peticion.FechaCierre,          // opcional: null se guarda NULL
            NumeroCohortes = peticion.NumeroCohortes!,
            CantGraduados = peticion.CantGraduados!,
            FechaActualizacion = peticion.FechaActualizacion!,
            Ciudad = peticion.Ciudad!,
            Facultad = peticion.Facultad!.Value
        };

        try
        {
            return await _repositorio.CrearAsync(entidad);
        }
        catch (SqlException ex)
        {
            throw TraducirSqlException(ex, peticion.Id!.Value);
        }
    }

    public async Task<int> ReemplazarAsync(int id, ProgramaReemplazo peticion)
    {
        var entidad = new Programa
        {
            Id = id,
            Nombre = peticion.Nombre!,
            Tipo = peticion.Tipo!,
            Nivel = peticion.Nivel!,
            FechaCreacion = peticion.FechaCreacion!,
            FechaCierre = peticion.FechaCierre,
            NumeroCohortes = peticion.NumeroCohortes!,
            CantGraduados = peticion.CantGraduados!,
            FechaActualizacion = peticion.FechaActualizacion!,
            Ciudad = peticion.Ciudad!,
            Facultad = peticion.Facultad!.Value
        };

        int filas;
        try
        {
            filas = await _repositorio.ActualizarAsync(entidad);
        }
        catch (SqlException ex)
        {
            throw TraducirSqlException(ex, id);
        }

        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return filas;
    }

    public async Task<int> ActualizarAsync(int id, ProgramaActualizar peticion)
    {
        // Body sin campos -> regla de negocio 400 (contrato §6). Se comprueba
        // antes de ir a la base: no tiene sentido consultar si nada va a cambiar.
        if (peticion.Nombre is null && peticion.Tipo is null && peticion.Nivel is null
            && peticion.FechaCreacion is null && peticion.FechaCierre is null
            && peticion.NumeroCohortes is null && peticion.CantGraduados is null
            && peticion.FechaActualizacion is null && peticion.Ciudad is null
            && peticion.Facultad is null)
            throw new ArgumentException("No se envió ningún campo para actualizar.");

        // Se lee la fila actual y se combina: el procedimiento tiene parametros
        // fijos, asi que el PATCH se resuelve aqui (3_plan.md §7).
        var entidad = await ObtenerPorIdAsync(id);

        if (peticion.Nombre is not null) entidad.Nombre = peticion.Nombre;
        if (peticion.Tipo is not null) entidad.Tipo = peticion.Tipo;
        if (peticion.Nivel is not null) entidad.Nivel = peticion.Nivel;
        if (peticion.FechaCreacion is not null) entidad.FechaCreacion = peticion.FechaCreacion;
        if (peticion.FechaCierre is not null) entidad.FechaCierre = peticion.FechaCierre;
        if (peticion.NumeroCohortes is not null) entidad.NumeroCohortes = peticion.NumeroCohortes;
        if (peticion.CantGraduados is not null) entidad.CantGraduados = peticion.CantGraduados;
        if (peticion.FechaActualizacion is not null) entidad.FechaActualizacion = peticion.FechaActualizacion;
        if (peticion.Ciudad is not null) entidad.Ciudad = peticion.Ciudad;
        if (peticion.Facultad is not null) entidad.Facultad = peticion.Facultad.Value;

        try
        {
            return await _repositorio.ActualizarAsync(entidad);
        }
        catch (SqlException ex)
        {
            throw TraducirSqlException(ex, id);
        }
    }

    public async Task<int> EliminarAsync(int id)
    {
        // D4 (4_research.md): el servicio distingue "no existe" (404) de "tiene
        // hijos activos" (409) leyendo el maestro antes de borrar; el
        // procedimiento no devuelve un motivo, solo 0 filas cuando bloquea.
        var registro = await _repositorio.ObtenerPorIdAsync(id);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        var filas = await _repositorio.EliminarAsync(id);
        if (filas == 0)
            throw new ConflictoExcepcion(
                "El programa tiene registros asociados activos; primero retírelos.",
                "No se puede retirar el registro.");

        return filas;
    }

    /// <summary>
    /// Traduce el SqlException del motor a la excepcion de dominio correcta.
    /// Devuelve Exception a proposito: si el numero no es de integridad, se
    /// relanza el SqlException original y el controlador responde 500 (un 409
    /// solo sale por integridad referencial o claves; contrato §10).
    /// </summary>
    private static Exception TraducirSqlException(SqlException ex, int id)
    {
        return ex.Number switch
        {
            // THROW del procedimiento: la facultad no existe o esta retirada.
            50102 => new ConflictoExcepcion("La facultad indicada no existe o está inactiva."),
            // Respaldo: violacion real de la restriccion FK (carrera).
            547 => new ConflictoExcepcion("La facultad indicada no existe o está inactiva."),
            2627 or 2601 => new ConflictoExcepcion($"Ya existe un registro con esa clave ({id})."),
            _ => ex
        };
    }
}
