using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Contrato del servicio de negocio de `car_innovacion`. El controlador depende
/// de esta interfaz: no conoce HTTP ni el motor de base de datos
/// (1_constitution.md, Articulo 3).
/// </summary>
public interface IServicioCarInnovacion
{
    Task<List<CarInnovacion>> ObtenerTodosAsync(int limite);
    Task<CarInnovacion> ObtenerPorIdAsync(int id);
    Task CrearAsync(CarInnovacionCrear peticion);
    Task<int> ReemplazarAsync(int id, CarInnovacionReemplazo peticion);
    Task<int> ActualizarAsync(int id, CarInnovacionActualizar peticion);
    Task<int> EliminarAsync(int id);
}
