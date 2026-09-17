using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

public interface IRepositorioPracticaEstrategia
{
    Task<List<PracticaEstrategia>> ObtenerTodosAsync(int limite);
    Task<PracticaEstrategia?> ObtenerPorIdAsync(int id);
    Task CrearAsync(PracticaEstrategia entidad);
    Task<int> ActualizarAsync(int id, Dictionary<string, object> datos);
    Task<int> EliminarAsync(int id);
}
