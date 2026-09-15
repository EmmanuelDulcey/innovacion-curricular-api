using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Reglas de negocio de `enfoque`: el repositorio solo recibe campos permitidos.
/// Un PATCH sin campos produce ArgumentException y un registro inexistente produce 404.
/// </summary>
public class ServicioEnfoque : IServicioEnfoque
{
    private readonly IRepositorioEnfoque _repositorio;

    public ServicioEnfoque(IRepositorioEnfoque repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<Enfoque>> ObtenerTodosAsync(int limite)
    {
        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Enfoque> ObtenerPorIdAsync(int id)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(id);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return registro;
    }

    public async Task CrearAsync(EnfoqueCrear peticion)
    {
        await _repositorio.CrearAsync(new Enfoque
        {
            Id = peticion.Id!.Value,
            Nombre = peticion.Nombre!,
            Descripcion = peticion.Descripcion!
        });
    }

    public async Task<int> ReemplazarAsync(int id, EnfoqueReemplazo peticion)
    {
        var datos = new Dictionary<string, object>
        {
            ["nombre"] = peticion.Nombre!,
            ["descripcion"] = peticion.Descripcion!
        };

        var filas = await _repositorio.ActualizarAsync(id, datos);
        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return filas;
    }

    public async Task<int> ActualizarAsync(int id, EnfoqueActualizar peticion)
    {
        var datos = new Dictionary<string, object>();

        if (peticion.Nombre is not null) datos["nombre"] = peticion.Nombre;
        if (peticion.Descripcion is not null) datos["descripcion"] = peticion.Descripcion;

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
