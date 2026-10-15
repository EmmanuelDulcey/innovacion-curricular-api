using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Contrato del servicio de negocio de `facultad` (v2).
/// Fuente: docs/spec_kit/versiones/v2/3_plan.md §3.
/// </summary>
public interface IServicioFacultad
{
    Task<List<Facultad>> ObtenerTodosAsync(int limite);
    Task<Facultad> ObtenerPorIdAsync(int id);
    Task<Facultad> CrearAsync(FacultadCrear peticion);
    Task<int> ReemplazarAsync(int id, FacultadReemplazo peticion);
    Task<int> ActualizarAsync(int id, FacultadActualizar peticion);
    Task<int> EliminarAsync(int id);
}
