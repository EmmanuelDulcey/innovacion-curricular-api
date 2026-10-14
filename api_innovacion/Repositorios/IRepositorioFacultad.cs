using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Contrato de datos de la tabla `facultad` (v2).
/// El servicio conoce esta interfaz, nunca el repositorio concreto.
/// Fuente: docs/spec_kit/versiones/v2/3_plan.md §4.1.
/// </summary>
public interface IRepositorioFacultad
{
    Task<List<Facultad>> ObtenerTodosAsync(int limite);
    Task<Facultad?> ObtenerPorIdAsync(int id);
    Task<Facultad> CrearAsync(Facultad entidad);
    Task<int> ActualizarAsync(Facultad entidad);
    Task<int> EliminarAsync(int id);
}
