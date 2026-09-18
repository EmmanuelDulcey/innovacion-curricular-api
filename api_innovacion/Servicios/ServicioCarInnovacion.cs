using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Reglas de negocio de `car_innovacion`. Recibe la INTERFAZ del repositorio
/// (la inyecta el ensamblador), asi que esta clase no sabe que detras hay
/// SQL Server. Traduce "no existe" a NoEncontradoExcepcion (-> 404) y
/// "PATCH sin campos" a ArgumentException (-> 400)
/// (3_plan.md, Secciones 4.2 y 4.6).
/// </summary>
public class ServicioCarInnovacion : IServicioCarInnovacion
{
    private readonly IRepositorioCarInnovacion _repositorio;

    public ServicioCarInnovacion(IRepositorioCarInnovacion repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<CarInnovacion>> ObtenerTodosAsync(int limite)
    {
        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<CarInnovacion> ObtenerPorIdAsync(int id)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(id);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return registro;
    }

    public async Task CrearAsync(CarInnovacionCrear peticion)
    {
        await _repositorio.CrearAsync(new CarInnovacion
        {
            Id = peticion.Id!.Value,
            Nombre = peticion.Nombre!,
            Descripcion = peticion.Descripcion!,
            Tipo = peticion.Tipo!
        });
    }

    public async Task<int> ReemplazarAsync(int id, CarInnovacionReemplazo peticion)
    {
        // El PUT envia todos los campos modificables: el diccionario va completo.
        var datos = new Dictionary<string, object>
        {
            ["nombre"] = peticion.Nombre!,
            ["descripcion"] = peticion.Descripcion!,
            ["tipo"] = peticion.Tipo!
        };

        var filas = await _repositorio.ActualizarAsync(id, datos);
        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return filas;
    }

    public async Task<int> ActualizarAsync(int id, CarInnovacionActualizar peticion)
    {
        // El PATCH solo incluye en el diccionario los campos que llegaron.
        var datos = new Dictionary<string, object>();

        if (peticion.Nombre is not null) datos["nombre"] = peticion.Nombre;
        if (peticion.Descripcion is not null) datos["descripcion"] = peticion.Descripcion;
        if (peticion.Tipo is not null) datos["tipo"] = peticion.Tipo;

        // No es un error de forma (eso seria 422) sino de regla de negocio:
        // no hay nada que actualizar (3_plan.md, Seccion 4.2).
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
