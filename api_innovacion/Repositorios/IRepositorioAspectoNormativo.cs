using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

public interface IRepositorioAspectoNormativo
{
    Task<List<AspectoNormativo>> ObtenerTodosAsync(int limite);
    Task<AspectoNormativo?> ObtenerPorIdAsync(int id);
    Task CrearAsync(AspectoNormativo entidad);
    Task<int> ActualizarAsync(int id, Dictionary<string, object> datos);
    Task<int> EliminarAsync(int id);
}
