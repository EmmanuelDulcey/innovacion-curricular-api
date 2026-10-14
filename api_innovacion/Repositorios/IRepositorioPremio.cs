using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Contrato de datos de la tabla `premio` (v2).
/// El servicio conoce esta interfaz, nunca el repositorio concreto.
/// Fuente: docs/spec_kit/versiones/v2/3_plan.md §4.1.
/// </summary>
public interface IRepositorioPremio
{
    Task<List<Premio>> ObtenerTodosAsync(int limite);
    Task<Premio?> ObtenerPorIdAsync(int id);
    Task<Premio> CrearAsync(Premio entidad);
    Task<int> ActualizarAsync(Premio entidad);
    Task<int> EliminarAsync(int id);
}
