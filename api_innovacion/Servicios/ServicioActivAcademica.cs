using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Servicio de negocio de `activ_academica` (v2). Su unica FK es `disenio`
/// (opcional): cuando llega null, el procedimiento no valida nada y guarda
/// NULL; cuando llega con valor, el procedimiento comprueba que el programa
/// exista y este activo (THROW 50101).
///
/// Tiene la regla del padre: se bloquea el retiro (409) mientras haya parejas
/// activas en aa_rc.
/// </summary>
public class ServicioActivAcademica : IServicioActivAcademica
{
    private readonly IRepositorioActivAcademica _repositorio;

    public ServicioActivAcademica(IRepositorioActivAcademica repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<List<ActivAcademica>> ObtenerTodosAsync(int limite)
    {
        return _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<ActivAcademica> ObtenerPorIdAsync(int id)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(id);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con id = {id}.");

        return registro;
    }

    public async Task<ActivAcademica> CrearAsync(ActivAcademicaCrear peticion)
    {
        var entidad = new ActivAcademica
        {
            // Sin Id: lo genera SQL Server al insertar y vuelve en la fila.
            Nombre = peticion.Nombre!,
            NumCreditos = peticion.NumCreditos!.Value,
            Tipo = peticion.Tipo!,
            AreaFormacion = peticion.AreaFormacion!,
            HAcom = peticion.HAcom!.Value,
            HIndep = peticion.HIndep!.Value,
            Idioma = peticion.Idioma!,
            Espejo = peticion.Espejo!.Value,
            EntidadEspejo = peticion.EntidadEspejo!,
            PaisEspejo = peticion.PaisEspejo!,
            Disenio = peticion.Disenio                     // opcional: null se guarda NULL
        };

        try
        {
            return await _repositorio.CrearAsync(entidad);
        }
        catch (SqlException ex)
        {
            // Al crear no hay clave enviada por el cliente.
            throw TraducirSqlException(ex, null);
        }
    }

    public async Task<int> ReemplazarAsync(int id, ActivAcademicaReemplazo peticion)
    {
        var entidad = new ActivAcademica
        {
            Id = id,
            Nombre = peticion.Nombre!,
            NumCreditos = peticion.NumCreditos!.Value,
            Tipo = peticion.Tipo!,
            AreaFormacion = peticion.AreaFormacion!,
            HAcom = peticion.HAcom!.Value,
            HIndep = peticion.HIndep!.Value,
            Idioma = peticion.Idioma!,
            Espejo = peticion.Espejo!.Value,
            EntidadEspejo = peticion.EntidadEspejo!,
            PaisEspejo = peticion.PaisEspejo!,
            Disenio = peticion.Disenio
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

    public async Task<int> ActualizarAsync(int id, ActivAcademicaActualizar peticion)
    {
        if (peticion.Nombre is null && peticion.NumCreditos is null && peticion.Tipo is null
            && peticion.AreaFormacion is null && peticion.HAcom is null && peticion.HIndep is null
            && peticion.Idioma is null && peticion.Espejo is null && peticion.EntidadEspejo is null
            && peticion.PaisEspejo is null && peticion.Disenio is null)
            throw new ArgumentException("No se envió ningún campo para actualizar.");

        var entidad = await ObtenerPorIdAsync(id);

        if (peticion.Nombre is not null) entidad.Nombre = peticion.Nombre;
        if (peticion.NumCreditos is not null) entidad.NumCreditos = peticion.NumCreditos.Value;
        if (peticion.Tipo is not null) entidad.Tipo = peticion.Tipo;
        if (peticion.AreaFormacion is not null) entidad.AreaFormacion = peticion.AreaFormacion;
        if (peticion.HAcom is not null) entidad.HAcom = peticion.HAcom.Value;
        if (peticion.HIndep is not null) entidad.HIndep = peticion.HIndep.Value;
        if (peticion.Idioma is not null) entidad.Idioma = peticion.Idioma;
        if (peticion.Espejo is not null) entidad.Espejo = peticion.Espejo.Value;
        if (peticion.EntidadEspejo is not null) entidad.EntidadEspejo = peticion.EntidadEspejo;
        if (peticion.PaisEspejo is not null) entidad.PaisEspejo = peticion.PaisEspejo;
        if (peticion.Disenio is not null) entidad.Disenio = peticion.Disenio.Value;

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
                "La actividad académica tiene registros asociados activos; primero retírelos.",
                "No se puede retirar el registro.");

        return filas;
    }

    private static Exception TraducirSqlException(SqlException ex, int? id)
    {
        return ex.Number switch
        {
            50101 => new ConflictoExcepcion("El programa indicado no existe o está inactivo."),
            547 => new ConflictoExcepcion("El programa indicado no existe o está inactivo."),
            2627 or 2601 => new ConflictoExcepcion(id is null
                ? "Ya existe un registro con esa información."
                : $"Ya existe un registro con esa clave ({id})."),
            _ => ex
        };
    }
}
