using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Contrato de datos de la tabla `enfoque`.
/// </summary>
public interface IRepositorioEnfoque
{
    Task<List<Enfoque>> ObtenerTodosAsync(int limite);
    Task<Enfoque?> ObtenerPorIdAsync(int id);
    Task CrearAsync(Enfoque entidad);
    Task<int> ActualizarAsync(int id, Dictionary<string, object> datos);
    Task<int> EliminarAsync(int id);
}
