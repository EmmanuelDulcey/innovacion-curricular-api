using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Contrato de datos de la tabla `programa` (v2). El servicio conoce esta
/// interfaz, nunca el repositorio concreto (3_plan.md §4.1).
/// `ActualizarAsync` recibe la entidad completa (los procedimientos tienen
/// parametros fijos; el PATCH lo combina el servicio) y `EliminarAsync`
/// devuelve 0 cuando la regla del padre con hijos activos bloqueo el retiro.
/// </summary>
public interface IRepositorioPrograma
{
    Task<List<Programa>> ObtenerTodosAsync(int limite);
    Task<Programa?> ObtenerPorIdAsync(int id);
    Task<Programa> CrearAsync(Programa entidad);
    Task<int> ActualizarAsync(Programa entidad);
    Task<int> EliminarAsync(int id);
}
