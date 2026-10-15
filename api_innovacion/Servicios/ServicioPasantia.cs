using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Servicio de negocio de `pasantia` (v2). Traduce errores del motor a errores
/// de dominio: FK inexistente y clave duplicada -> ConflictoExcepcion (409);
/// registro inexistente -> NoEncontradoExcepcion (404).
/// Fuente: docs/spec_kit/versiones/v2/3_plan.md §4.6 y §7.
/// </summary>
public class ServicioPasantia : IServicioPasantia
{
    private readonly IRepositorioPasantia _repositorio;

    public ServicioPasantia(IRepositorioPasantia repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<List<Pasantia>> ObtenerTodosAsync(int limite)
    {
        return _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Pasantia> ObtenerPorIdAsync(int id)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(id);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return registro;
    }

    public async Task<Pasantia> CrearAsync(PasantiaCrear peticion)
    {
        var entidad = new Pasantia
        {
            Id = peticion.Id!.Value,
            Nombre = peticion.Nombre!,
            Pais = peticion.Pais!,
            Empresa = peticion.Empresa!,
            Descripcion = peticion.Descripcion!,
            Programa = peticion.Programa!.Value
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

    public async Task<int> ReemplazarAsync(int id, PasantiaReemplazo peticion)
    {
        var entidad = new Pasantia
        {
            Id = id,
            Nombre = peticion.Nombre!,
            Pais = peticion.Pais!,
            Empresa = peticion.Empresa!,
            Descripcion = peticion.Descripcion!,
            Programa = peticion.Programa!.Value
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

    public async Task<int> ActualizarAsync(int id, PasantiaActualizar peticion)
    {
        if (peticion.Nombre is null && peticion.Pais is null && peticion.Empresa is null
            && peticion.Descripcion is null && peticion.Programa is null)
            throw new ArgumentException("No se envió ningún campo para actualizar.");

        var entidad = await ObtenerPorIdAsync(id);

        if (peticion.Nombre is not null) entidad.Nombre = peticion.Nombre;
        if (peticion.Pais is not null) entidad.Pais = peticion.Pais;
        if (peticion.Empresa is not null) entidad.Empresa = peticion.Empresa;
        if (peticion.Descripcion is not null) entidad.Descripcion = peticion.Descripcion;
        if (peticion.Programa is not null) entidad.Programa = peticion.Programa.Value;

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
        var filas = await _repositorio.EliminarAsync(id);
        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return filas;
    }

    private static ConflictoExcepcion TraducirSqlException(SqlException ex, int id)
    {
        return ex.Number switch
        {
            547 => new ConflictoExcepcion("El programa indicado no existe o está inactivo."),
            2627 or 2601 => new ConflictoExcepcion($"Ya existe un registro con esa clave ({id})."),
            _ => new ConflictoExcepcion(ex.Message)
        };
    }
}
