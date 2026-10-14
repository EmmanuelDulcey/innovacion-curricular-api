using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Contrato del servicio de negocio de `premio` (v2).
/// Fuente: docs/spec_kit/versiones/v2/3_plan.md §3.
/// </summary>
public interface IServicioPremio
{
    Task<List<Premio>> ObtenerTodosAsync(int limite);
    Task<Premio> ObtenerPorIdAsync(int id);
    Task<Premio> CrearAsync(PremioCrear peticion);
    Task<int> ReemplazarAsync(int id, PremioReemplazo peticion);
    Task<int> ActualizarAsync(int id, PremioActualizar peticion);
    Task<int> EliminarAsync(int id);
}
