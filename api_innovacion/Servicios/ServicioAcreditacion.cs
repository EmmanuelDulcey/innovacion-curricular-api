using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Servicio de negocio de `acreditacion` (v2). Es la tabla del disparador:
/// crear, actualizar o retirar una acreditacion recalcula solo el contador del
/// programa; este servicio NUNCA escribe num_acreditaciones.
///
/// No tiene la regla del padre con hijos: acreditacion es hoja. Por eso su
/// DELETE no distingue motivos: 0 filas es simplemente "no existe" (404).
/// </summary>
public class ServicioAcreditacion : IServicioAcreditacion
{
    private readonly IRepositorioAcreditacion _repositorio;

    public ServicioAcreditacion(IRepositorioAcreditacion repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<List<Acreditacion>> ObtenerTodosAsync(int limite)
    {
        return _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Acreditacion> ObtenerPorIdAsync(int resolucion)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(resolucion);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con resolucion = {resolucion}.");

        return registro;
    }

    public async Task<Acreditacion> CrearAsync(AcreditacionCrear peticion)
    {
        var entidad = new Acreditacion
        {
            Resolucion = peticion.Resolucion!.Value,
            Tipo = peticion.Tipo!,
            Calificacion = peticion.Calificacion!,
            FechaInicio = peticion.FechaInicio!,
            FechaFin = peticion.FechaFin!,
            Programa = peticion.Programa!.Value
        };

        try
        {
            return await _repositorio.CrearAsync(entidad);
        }
        catch (SqlException ex)
        {
            throw TraducirSqlException(ex, peticion.Resolucion!.Value);
        }
    }

    public async Task<int> ReemplazarAsync(int resolucion, AcreditacionReemplazo peticion)
    {
        var entidad = new Acreditacion
        {
            Resolucion = resolucion,
            Tipo = peticion.Tipo!,
            Calificacion = peticion.Calificacion!,
            FechaInicio = peticion.FechaInicio!,
            FechaFin = peticion.FechaFin!,
            Programa = peticion.Programa!.Value
        };

        int filas;
        try
        {
            filas = await _repositorio.ActualizarAsync(entidad);
        }
        catch (SqlException ex)
        {
            throw TraducirSqlException(ex, resolucion);
        }

        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con resolucion = {resolucion}.");

        return filas;
    }

    public async Task<int> ActualizarAsync(int resolucion, AcreditacionActualizar peticion)
    {
        if (peticion.Tipo is null && peticion.Calificacion is null
            && peticion.FechaInicio is null && peticion.FechaFin is null
            && peticion.Programa is null)
            throw new ArgumentException("No se envió ningún campo para actualizar.");

        var entidad = await ObtenerPorIdAsync(resolucion);

        if (peticion.Tipo is not null) entidad.Tipo = peticion.Tipo;
        if (peticion.Calificacion is not null) entidad.Calificacion = peticion.Calificacion;
        if (peticion.FechaInicio is not null) entidad.FechaInicio = peticion.FechaInicio;
        if (peticion.FechaFin is not null) entidad.FechaFin = peticion.FechaFin;
        if (peticion.Programa is not null) entidad.Programa = peticion.Programa.Value;

        try
        {
            return await _repositorio.ActualizarAsync(entidad);
        }
        catch (SqlException ex)
        {
            throw TraducirSqlException(ex, resolucion);
        }
    }

    public async Task<int> EliminarAsync(int resolucion)
    {
        // Borrado logico (UPDATE activo = 0): el disparador escucha el UPDATE y
        // baja el contador del programa. Sin hijos: 0 filas -> 404.
        var filas = await _repositorio.EliminarAsync(resolucion);
        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con resolucion = {resolucion}.");

        return filas;
    }

    /// <summary>
    /// Solo los errores de integridad se traducen a 409; el resto se relanza
    /// como SqlException (el controlador responde 500). Ver ServicioPrograma.
    /// </summary>
    private static Exception TraducirSqlException(SqlException ex, int resolucion)
    {
        return ex.Number switch
        {
            50101 => new ConflictoExcepcion("El programa indicado no existe o está inactivo."),
            547 => new ConflictoExcepcion("El programa indicado no existe o está inactivo."),
            2627 or 2601 => new ConflictoExcepcion($"Ya existe un registro con esa clave ({resolucion})."),
            _ => ex
        };
    }
}
