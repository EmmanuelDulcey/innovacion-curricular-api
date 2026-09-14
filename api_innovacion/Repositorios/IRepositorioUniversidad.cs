using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Contrato de datos de la tabla `universidad`.
/// El servicio conoce esta interfaz, nunca el repositorio concreto.
/// Fuente: docs/spec_kit/versiones/v1_producto_sqlserver/3_plan.md, Seccion 4.1.
/// </summary>
public interface IRepositorioUniversidad
{
    Task<List<Universidad>> ObtenerTodosAsync(int limite);
    Task<Universidad?> ObtenerPorIdAsync(int id);
    Task CrearAsync(Universidad entidad);
    Task<int> ActualizarAsync(int id, Dictionary<string, object> datos);
    Task<int> EliminarAsync(int id);
}