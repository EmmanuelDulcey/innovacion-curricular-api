using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Servicio de negocio de `premio` (v2). Traduce errores del motor a errores de
/// dominio: FK inexistente y clave duplicada -> ConflictoExcepcion (409);
/// registro inexistente -> NoEncontradoExcepcion (404).
/// Fuente: docs/spec_kit/versiones/v2/3_plan.md §4.6 y §7.
/// </summary>
public class ServicioPremio : IServicioPremio
{
    private readonly IRepositorioPremio _repositorio;

    public ServicioPremio(IRepositorioPremio repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<List<Premio>> ObtenerTodosAsync(int limite)
    {
        return _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Premio> ObtenerPorIdAsync(int id)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(id);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return registro;
    }

    public async Task<Premio> CrearAsync(PremioCrear peticion)
    {
        var entidad = new Premio
        {
            Id = peticion.Id!.Value,
            Nombre = peticion.Nombre!,
            Descripcion = peticion.Descripcion!,
            Fecha = peticion.Fecha!.Value,
            EntidadOtorgante = peticion.EntidadOtorgante!,
            Pais = peticion.Pais!,
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

    public async Task<int> ReemplazarAsync(int id, PremioReemplazo peticion)
    {
        var entidad = new Premio
        {
            Id = id,
            Nombre = peticion.Nombre!,
            Descripcion = peticion.Descripcion!,
            Fecha = peticion.Fecha!.Value,
            EntidadOtorgante = peticion.EntidadOtorgante!,
            Pais = peticion.Pais!,
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

    public async Task<int> ActualizarAsync(int id, PremioActualizar peticion)
    {
        if (peticion.Nombre is null && peticion.Descripcion is null && peticion.Fecha is null
            && peticion.EntidadOtorgante is null && peticion.Pais is null && peticion.Programa is null)
            throw new ArgumentException("No se envió ningún campo para actualizar.");

        var entidad = await ObtenerPorIdAsync(id);

        if (peticion.Nombre is not null) entidad.Nombre = peticion.Nombre;
        if (peticion.Descripcion is not null) entidad.Descripcion = peticion.Descripcion;
        if (peticion.Fecha is not null) entidad.Fecha = peticion.Fecha.Value;
        if (peticion.EntidadOtorgante is not null) entidad.EntidadOtorgante = peticion.EntidadOtorgante;
        if (peticion.Pais is not null) entidad.Pais = peticion.Pais;
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
