using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Contrato de datos de la tabla `area_conocimiento`.
/// El servicio conoce esta interfaz, nunca el repositorio concreto.
/// Fuente: docs/spec_kit/versiones/v1_producto_sqlserver/3_plan.md, Seccion 4.1.
/// </summary>
public interface IRepositorioAreaConocimiento
{
    Task<List<AreaConocimiento>> ObtenerTodosAsync(int limite);
    Task<AreaConocimiento?> ObtenerPorIdAsync(int id);
    Task CrearAsync(AreaConocimiento entidad);
    Task<int> ActualizarAsync(int id, Dictionary<string, object> datos);
    Task<int> EliminarAsync(int id);
}