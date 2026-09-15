using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Servicio de negocio de `area_conocimiento`.
/// Traduce registros inexistentes a NoEncontradoExcepcion (404) y el PATCH sin
/// campos a ArgumentException (400). El mapeo a columnas de la tabla (snake_case)
/// vive aqui. Fuente: docs/spec_kit/versiones/v1_producto_sqlserver/8_tasks.md Fase 4.
/// </summary>
public class ServicioAreaConocimiento : IServicioAreaConocimiento
{
    private readonly IRepositorioAreaConocimiento _repositorio;

    public ServicioAreaConocimiento(IRepositorioAreaConocimiento repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<AreaConocimiento>> ObtenerTodosAsync(int limite)
    {
        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<AreaConocimiento> ObtenerPorIdAsync(int id)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(id);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return registro;
    }

    public async Task CrearAsync(AreaConocimientoCrear peticion)
    {
        await _repositorio.CrearAsync(new AreaConocimiento
        {
            Id = peticion.Id!.Value,
            GranArea = peticion.GranArea!,
            Area = peticion.Area!,
            Disciplina = peticion.Disciplina!
        });
    }

    public async Task<int> ReemplazarAsync(int id, AreaConocimientoReemplazo peticion)
    {
        var datos = new Dictionary<string, object>
        {
            ["gran_area"] = peticion.GranArea!,
            ["area"] = peticion.Area!,
            ["disciplina"] = peticion.Disciplina!
        };

        var filas = await _repositorio.ActualizarAsync(id, datos);
        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return filas;
    }

    public async Task<int> ActualizarAsync(int id, AreaConocimientoActualizar peticion)
    {
        var datos = new Dictionary<string, object>();

        if (peticion.GranArea is not null) datos["gran_area"] = peticion.GranArea;
        if (peticion.Area is not null) datos["area"] = peticion.Area;
        if (peticion.Disciplina is not null) datos["disciplina"] = peticion.Disciplina;

        if (datos.Count == 0)
            throw new ArgumentException("No se envió ningún campo para actualizar.");

        var filas = await _repositorio.ActualizarAsync(id, datos);
        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return filas;
    }

    public async Task<int> EliminarAsync(int id)
    {
        var filas = await _repositorio.EliminarAsync(id);
        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return filas;
    }
}