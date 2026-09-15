using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

public class ServicioAspectoNormativo : IServicioAspectoNormativo
{
    private readonly IRepositorioAspectoNormativo _repositorio;

    public ServicioAspectoNormativo(IRepositorioAspectoNormativo repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<AspectoNormativo>> ObtenerTodosAsync(int limite)
    {
        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<AspectoNormativo> ObtenerPorIdAsync(int id)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(id);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return registro;
    }

    public async Task CrearAsync(AspectoNormativoCrear peticion)
    {
        await _repositorio.CrearAsync(new AspectoNormativo
        {
            Id = peticion.Id!.Value,
            Tipo = peticion.Tipo!,
            Descripcion = peticion.Descripcion!,
            Fuente = peticion.Fuente!
        });
    }

    public async Task<int> ReemplazarAsync(int id, AspectoNormativoReemplazo peticion)
    {
        var datos = new Dictionary<string, object>
        {
            ["tipo"] = peticion.Tipo!,
            ["descripcion"] = peticion.Descripcion!,
            ["fuente"] = peticion.Fuente!
        };

        var filas = await _repositorio.ActualizarAsync(id, datos);
        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return filas;
    }

    public async Task<int> ActualizarAsync(int id, AspectoNormativoActualizar peticion)
    {
        var datos = new Dictionary<string, object>();

        if (peticion.Tipo is not null) datos["tipo"] = peticion.Tipo;
        if (peticion.Descripcion is not null) datos["descripcion"] = peticion.Descripcion;
        if (peticion.Fuente is not null) datos["fuente"] = peticion.Fuente;

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
