using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

public class ServicioPracticaEstrategia : IServicioPracticaEstrategia
{
    private readonly IRepositorioPracticaEstrategia _repositorio;

    public ServicioPracticaEstrategia(IRepositorioPracticaEstrategia repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<PracticaEstrategia>> ObtenerTodosAsync(int limite)
    {
        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<PracticaEstrategia> ObtenerPorIdAsync(int id)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(id);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return registro;
    }

    public async Task CrearAsync(PracticaEstrategiaCrear peticion)
    {
        await _repositorio.CrearAsync(new PracticaEstrategia
        {
            Id = peticion.Id!.Value,
            Tipo = peticion.Tipo!,
            Nombre = peticion.Nombre!,
            Descripcion = peticion.Descripcion!
        });
    }

    public async Task<int> ReemplazarAsync(int id, PracticaEstrategiaReemplazo peticion)
    {
        var datos = new Dictionary<string, object>
        {
            ["tipo"] = peticion.Tipo!,
            ["nombre"] = peticion.Nombre!,
            ["descripcion"] = peticion.Descripcion!
        };

        var filas = await _repositorio.ActualizarAsync(id, datos);
        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return filas;
    }

    public async Task<int> ActualizarAsync(int id, PracticaEstrategiaActualizar peticion)
    {
        var datos = new Dictionary<string, object>();

        if (peticion.Tipo is not null) datos["tipo"] = peticion.Tipo;
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
