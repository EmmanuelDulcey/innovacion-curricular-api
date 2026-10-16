// =============================================================================
// QUÉ ES: servicio de negocio del catálogo de docentes.
// PAPEL: valida operaciones y traduce duplicados y retiros bloqueados a 409.
// =============================================================================
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

public class ServicioDocente : IServicioDocente
{
    private readonly IRepositorioDocente _repositorio;

    public ServicioDocente(IRepositorioDocente repositorio) => _repositorio = repositorio;

    public Task<List<Docente>> ObtenerTodosAsync(int limite) => _repositorio.ObtenerTodosAsync(limite);

    public async Task<Docente> ObtenerPorIdAsync(int id) =>
        await _repositorio.ObtenerPorIdAsync(id)
        ?? throw new NoEncontradoExcepcion($"No existe un docente activo con id = {id}.");

    public async Task<Docente> CrearAsync(DocenteCrear peticion)
    {
        try
        {
            return await _repositorio.CrearAsync(new Docente
            {
                Documento = peticion.Documento!.Trim(),
                Nombre = peticion.Nombre!.Trim()
            });
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            throw new ConflictoExcepcion("Ya existe un docente activo con ese documento.");
        }
    }

    public async Task<int> ReemplazarAsync(int id, DocenteReemplazo peticion)
    {
        var docente = new Docente { Id = id, Documento = peticion.Documento!.Trim(), Nombre = peticion.Nombre!.Trim() };
        return await GuardarAsync(docente, id);
    }

    public async Task<int> ActualizarAsync(int id, DocenteActualizar peticion)
    {
        if (peticion.Documento is null && peticion.Nombre is null)
            throw new ArgumentException("Indica el documento o el nombre que deseas actualizar.");

        var docente = await ObtenerPorIdAsync(id);
        if (peticion.Documento is not null) docente.Documento = peticion.Documento.Trim();
        if (peticion.Nombre is not null) docente.Nombre = peticion.Nombre.Trim();
        return await GuardarAsync(docente, id);
    }

    private async Task<int> GuardarAsync(Docente docente, int id)
    {
        try
        {
            var filas = await _repositorio.ActualizarAsync(docente);
            if (filas == 0) throw new NoEncontradoExcepcion($"No existe un docente activo con id = {id}.");
            return filas;
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            throw new ConflictoExcepcion("Ya existe otro docente con ese documento.");
        }
    }

    public async Task<int> EliminarAsync(int id)
    {
        _ = await ObtenerPorIdAsync(id);
        var filas = await _repositorio.EliminarAsync(id);
        if (filas == 0)
            throw new ConflictoExcepcion("El docente tiene vinculaciones activas; retíralas antes de desactivar el registro.");
        return filas;
    }
}
