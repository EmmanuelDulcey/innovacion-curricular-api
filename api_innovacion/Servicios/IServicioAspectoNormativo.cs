using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

public interface IServicioAspectoNormativo
{
    Task<List<AspectoNormativo>> ObtenerTodosAsync(int limite);
    Task<AspectoNormativo> ObtenerPorIdAsync(int id);
    Task CrearAsync(AspectoNormativoCrear peticion);
    Task<int> ReemplazarAsync(int id, AspectoNormativoReemplazo peticion);
    Task<int> ActualizarAsync(int id, AspectoNormativoActualizar peticion);
    Task<int> EliminarAsync(int id);
}
