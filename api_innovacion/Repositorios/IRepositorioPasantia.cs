using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Contrato de datos de la tabla `pasantia` (v2).
/// El servicio conoce esta interfaz, nunca el repositorio concreto.
/// Fuente: docs/spec_kit/versiones/v2/3_plan.md §4.1.
/// </summary>
public interface IRepositorioPasantia
{
    Task<List<Pasantia>> ObtenerTodosAsync(int limite);
    Task<Pasantia?> ObtenerPorIdAsync(int id);
    Task<Pasantia> CrearAsync(Pasantia entidad);
    Task<int> ActualizarAsync(Pasantia entidad);
    Task<int> EliminarAsync(int id);
}
