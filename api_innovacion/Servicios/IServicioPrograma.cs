using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Contrato del servicio de negocio de `programa` (v2). El controlador solo
/// conoce esta interfaz; el servicio es el punto unico que traduce los errores
/// del motor (SqlException) a errores de dominio (3_plan.md §3 y §7).
/// </summary>
public interface IServicioPrograma
{
    Task<List<Programa>> ObtenerTodosAsync(int limite);
    Task<Programa> ObtenerPorIdAsync(int id);
    Task<Programa> CrearAsync(ProgramaCrear peticion);
    Task<int> ReemplazarAsync(int id, ProgramaReemplazo peticion);
    Task<int> ActualizarAsync(int id, ProgramaActualizar peticion);
    Task<int> EliminarAsync(int id);
}
