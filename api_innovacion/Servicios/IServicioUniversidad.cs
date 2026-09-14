using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Contrato del servicio de negocio de `universidad`.
/// El controlador conoce esta interfaz; el servicio recibe IRepositorioUniversidad
/// y no conoce HTTP ni el motor. Fuente: docs/spec_kit/versiones/v1_producto_sqlserver/3_plan.md §3.
/// </summary>
public interface IServicioUniversidad
{
    Task<List<Universidad>> ObtenerTodosAsync(int limite);
    Task<Universidad> ObtenerPorIdAsync(int id);
    Task CrearAsync(UniversidadCrear peticion);
    Task<int> ReemplazarAsync(int id, UniversidadReemplazo peticion);
    Task<int> ActualizarAsync(int id, UniversidadActualizar peticion);
    Task<int> EliminarAsync(int id);
}