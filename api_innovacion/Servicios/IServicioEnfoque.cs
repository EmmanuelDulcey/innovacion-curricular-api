using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

public interface IServicioEnfoque
{
    Task<List<Enfoque>> ObtenerTodosAsync(int limite);
    Task<Enfoque> ObtenerPorIdAsync(int id);
    Task CrearAsync(EnfoqueCrear peticion);
    Task<int> ReemplazarAsync(int id, EnfoqueReemplazo peticion);
    Task<int> ActualizarAsync(int id, EnfoqueActualizar peticion);
    Task<int> EliminarAsync(int id);
}
