// =============================================================================
// QUÉ ES: contrato de persistencia del catálogo de docentes.
// PAPEL: permite probar las reglas del servicio sin depender de SQL Server.
// =============================================================================
using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

public interface IRepositorioDocente
{
    Task<List<Docente>> ObtenerTodosAsync(int limite);
    Task<Docente?> ObtenerPorIdAsync(int id);
    Task<Docente> CrearAsync(Docente entidad);
    Task<int> ActualizarAsync(Docente entidad);
    Task<int> EliminarAsync(int id);
}
