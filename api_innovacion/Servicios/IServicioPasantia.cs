using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Contrato del servicio de negocio de `pasantia` (v2).
/// Fuente: docs/spec_kit/versiones/v2/3_plan.md §3.
/// </summary>
public interface IServicioPasantia
{
    Task<List<Pasantia>> ObtenerTodosAsync(int limite);
    Task<Pasantia> ObtenerPorIdAsync(int id);
    Task<Pasantia> CrearAsync(PasantiaCrear peticion);
    Task<int> ReemplazarAsync(int id, PasantiaReemplazo peticion);
    Task<int> ActualizarAsync(int id, PasantiaActualizar peticion);
    Task<int> EliminarAsync(int id);
}
