using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Servicio de negocio de `facultad` (v2). Punto unico que traduce los errores
/// del motor a errores de dominio: FK inexistente y clave duplicada ->
/// ConflictoExcepcion (409); retiro bloqueado por programas activos ->
/// ConflictoExcepcion (409); registro inexistente -> NoEncontradoExcepcion (404).
/// Fuente: docs/spec_kit/versiones/v2/3_plan.md §4.6 y §7.
/// </summary>
public class ServicioFacultad : IServicioFacultad
{
    private readonly IRepositorioFacultad _repositorio;

    public ServicioFacultad(IRepositorioFacultad repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<List<Facultad>> ObtenerTodosAsync(int limite)
    {
        return _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Facultad> ObtenerPorIdAsync(int id)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(id);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return registro;
    }

    public async Task<Facultad> CrearAsync(FacultadCrear peticion)
    {
        var entidad = new Facultad
        {
                        Nombre = peticion.Nombre!,
            Tipo = peticion.Tipo!,
            FechaFun = peticion.FechaFun!.Value,
            Universidad = peticion.Universidad!.Value
        };

        try
        {
            return await _repositorio.CrearAsync(entidad);
        }
        catch (SqlException ex)
        {
            throw TraducirSqlException(ex, null);
        }
    }

    public async Task<int> ReemplazarAsync(int id, FacultadReemplazo peticion)
    {
        var entidad = new Facultad
        {
            Id = id,
            Nombre = peticion.Nombre!,
            Tipo = peticion.Tipo!,
            FechaFun = peticion.FechaFun!.Value,
            Universidad = peticion.Universidad!.Value
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

    public async Task<int> ActualizarAsync(int id, FacultadActualizar peticion)
    {
        if (peticion.Nombre is null && peticion.Tipo is null
            && peticion.FechaFun is null && peticion.Universidad is null)
            throw new ArgumentException("No se envió ningún campo para actualizar.");

        var entidad = await ObtenerPorIdAsync(id);

        if (peticion.Nombre is not null) entidad.Nombre = peticion.Nombre;
        if (peticion.Tipo is not null) entidad.Tipo = peticion.Tipo;
        if (peticion.FechaFun is not null) entidad.FechaFun = peticion.FechaFun.Value;
        if (peticion.Universidad is not null) entidad.Universidad = peticion.Universidad.Value;

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
        var registro = await _repositorio.ObtenerPorIdAsync(id);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        var filas = await _repositorio.EliminarAsync(id);
        if (filas == 0)
            throw new ConflictoExcepcion(
                "La facultad tiene programas asociados activos; primero retírelos.",
                "No se puede retirar el registro.");

        return filas;
    }

    private static ConflictoExcepcion TraducirSqlException(SqlException ex, int? id)
    {
        return ex.Number switch
        {
            547 => new ConflictoExcepcion("La universidad indicada no existe o está inactiva."),
            2627 or 2601 => new ConflictoExcepcion(id is null ? "Ya existe un registro con esa información." : "Ya existe un registro con esa clave (" + id.Value + ")."),
            _ => new ConflictoExcepcion(ex.Message)
        };
    }
}
