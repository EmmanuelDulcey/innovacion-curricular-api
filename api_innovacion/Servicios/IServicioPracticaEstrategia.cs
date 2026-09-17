using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

public interface IServicioPracticaEstrategia
{
    Task<List<PracticaEstrategia>> ObtenerTodosAsync(int limite);
    Task<PracticaEstrategia> ObtenerPorIdAsync(int id);
    Task CrearAsync(PracticaEstrategiaCrear peticion);
    Task<int> ReemplazarAsync(int id, PracticaEstrategiaReemplazo peticion);
    Task<int> ActualizarAsync(int id, PracticaEstrategiaActualizar peticion);
    Task<int> EliminarAsync(int id);
}
