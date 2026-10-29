using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Servicio de negocio de `registro_calificado` (v2). Aqui las fechas son DATE
/// (DateTime), el unico caso del bloque junto con la v1: se respeta el script
/// (4_research.md D13).
///
/// Tiene la regla del padre: sp_eliminar_registro_calificado devuelve 0 filas
/// si hay puentes activas (enfoque_rc o aa_rc) y el servicio lo traduce a 409.
/// </summary>
public class ServicioRegistroCalificado : IServicioRegistroCalificado
{
    private readonly IRepositorioRegistroCalificado _repositorio;

    public ServicioRegistroCalificado(IRepositorioRegistroCalificado repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<List<RegistroCalificado>> ObtenerTodosAsync(int limite)
    {
        return _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<RegistroCalificado> ObtenerPorIdAsync(int codigo)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(codigo);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con codigo = {codigo}.");

        return registro;
    }

    public async Task<RegistroCalificado> CrearAsync(RegistroCalificadoCrear peticion)
    {
        var entidad = new RegistroCalificado
        {
            Codigo = peticion.Codigo!.Value,
            CantCreditos = peticion.CantCreditos!,
            HoraAcom = peticion.HoraAcom!,
            HoraInd = peticion.HoraInd!,
            Metodologia = peticion.Metodologia!,
            FechaInicio = peticion.FechaInicio!.Value,
            FechaFin = peticion.FechaFin!.Value,
            DuracionAnios = peticion.DuracionAnios!,
            DuracionSemestres = peticion.DuracionSemestres!,
            TipoTitulacion = peticion.TipoTitulacion!,
            Programa = peticion.Programa!.Value
        };

        try
        {
            return await _repositorio.CrearAsync(entidad);
        }
        catch (SqlException ex)
        {
            throw TraducirSqlException(ex, peticion.Codigo!.Value);
        }
    }

    public async Task<int> ReemplazarAsync(int codigo, RegistroCalificadoReemplazo peticion)
    {
        var entidad = new RegistroCalificado
        {
            Codigo = codigo,
            CantCreditos = peticion.CantCreditos!,
            HoraAcom = peticion.HoraAcom!,
            HoraInd = peticion.HoraInd!,
            Metodologia = peticion.Metodologia!,
            FechaInicio = peticion.FechaInicio!.Value,
            FechaFin = peticion.FechaFin!.Value,
            DuracionAnios = peticion.DuracionAnios!,
            DuracionSemestres = peticion.DuracionSemestres!,
            TipoTitulacion = peticion.TipoTitulacion!,
            Programa = peticion.Programa!.Value
        };

        int filas;
        try
        {
            filas = await _repositorio.ActualizarAsync(entidad);
        }
        catch (SqlException ex)
        {
            throw TraducirSqlException(ex, codigo);
        }

        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con codigo = {codigo}.");

        return filas;
    }

    public async Task<int> ActualizarAsync(int codigo, RegistroCalificadoActualizar peticion)
    {
        if (peticion.CantCreditos is null && peticion.HoraAcom is null && peticion.HoraInd is null
            && peticion.Metodologia is null && peticion.FechaInicio is null && peticion.FechaFin is null
            && peticion.DuracionAnios is null && peticion.DuracionSemestres is null
            && peticion.TipoTitulacion is null && peticion.Programa is null)
            throw new ArgumentException("No se envió ningún campo para actualizar.");

        var entidad = await ObtenerPorIdAsync(codigo);

        if (peticion.CantCreditos is not null) entidad.CantCreditos = peticion.CantCreditos;
        if (peticion.HoraAcom is not null) entidad.HoraAcom = peticion.HoraAcom;
        if (peticion.HoraInd is not null) entidad.HoraInd = peticion.HoraInd;
        if (peticion.Metodologia is not null) entidad.Metodologia = peticion.Metodologia;
        if (peticion.FechaInicio is not null) entidad.FechaInicio = peticion.FechaInicio.Value;
        if (peticion.FechaFin is not null) entidad.FechaFin = peticion.FechaFin.Value;
        if (peticion.DuracionAnios is not null) entidad.DuracionAnios = peticion.DuracionAnios;
        if (peticion.DuracionSemestres is not null) entidad.DuracionSemestres = peticion.DuracionSemestres;
        if (peticion.TipoTitulacion is not null) entidad.TipoTitulacion = peticion.TipoTitulacion;
        if (peticion.Programa is not null) entidad.Programa = peticion.Programa.Value;

        try
        {
            return await _repositorio.ActualizarAsync(entidad);
        }
        catch (SqlException ex)
        {
            throw TraducirSqlException(ex, codigo);
        }
    }

    public async Task<int> EliminarAsync(int codigo)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(codigo);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con codigo = {codigo}.");

        var filas = await _repositorio.EliminarAsync(codigo);
        if (filas == 0)
            throw new ConflictoExcepcion(
                "El registro calificado tiene registros asociados activos; primero retírelos.",
                "No se puede retirar el registro.");

        return filas;
    }

    private static Exception TraducirSqlException(SqlException ex, int codigo)
    {
        return ex.Number switch
        {
            50101 => new ConflictoExcepcion("El programa indicado no existe o está inactivo."),
            547 => new ConflictoExcepcion("El programa indicado no existe o está inactivo."),
            2627 or 2601 => new ConflictoExcepcion($"Ya existe un registro con esa clave ({codigo})."),
            _ => ex
        };
    }
}
