using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Servicio de negocio de `universidad`.
/// Traduce registros inexistentes a NoEncontradoExcepcion (404) y el PATCH sin
/// campos a ArgumentException (400). El mapeo a columnas de la tabla (snake_case)
/// vive aqui. Fuente: docs/spec_kit/versiones/v1_producto_sqlserver/8_tasks.md Fase 4.
/// </summary>
public class ServicioUniversidad : IServicioUniversidad
{
    private readonly IRepositorioUniversidad _repositorio;

    public ServicioUniversidad(IRepositorioUniversidad repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<Universidad>> ObtenerTodosAsync(int limite)
    {
        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Universidad> ObtenerPorIdAsync(int id)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(id);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return registro;
    }

    public async Task CrearAsync(UniversidadCrear peticion)
    {
        await _repositorio.CrearAsync(new Universidad
        {
            Id = peticion.Id!.Value,
            Nombre = peticion.Nombre!,
            Tipo = peticion.Tipo!,
            Ciudad = peticion.Ciudad!
        });
    }

    public async Task<int> ReemplazarAsync(int id, UniversidadReemplazo peticion)
    {
        var datos = new Dictionary<string, object>
        {
            ["nombre"] = peticion.Nombre!,
            ["tipo"] = peticion.Tipo!,
            ["ciudad"] = peticion.Ciudad!
        };

        var filas = await _repositorio.ActualizarAsync(id, datos);
        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return filas;
    }

    public async Task<int> ActualizarAsync(int id, UniversidadActualizar peticion)
    {
        var datos = new Dictionary<string, object>();

        if (peticion.Nombre is not null) datos["nombre"] = peticion.Nombre;
        if (peticion.Tipo is not null) datos["tipo"] = peticion.Tipo;
        if (peticion.Ciudad is not null) datos["ciudad"] = peticion.Ciudad;

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