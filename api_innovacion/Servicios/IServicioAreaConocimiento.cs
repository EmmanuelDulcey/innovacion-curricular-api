using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Contrato del servicio de negocio de `area_conocimiento`.
/// El controlador conoce esta interfaz; el servicio recibe IRepositorioAreaConocimiento
/// y no conoce HTTP ni el motor. Fuente: docs/spec_kit/versiones/v1_producto_sqlserver/3_plan.md §3.
/// </summary>
public interface IServicioAreaConocimiento
{
    Task<List<AreaConocimiento>> ObtenerTodosAsync(int limite);
    Task<AreaConocimiento> ObtenerPorIdAsync(int id);
    Task CrearAsync(AreaConocimientoCrear peticion);
    Task<int> ReemplazarAsync(int id, AreaConocimientoReemplazo peticion);
    Task<int> ActualizarAsync(int id, AreaConocimientoActualizar peticion);
    Task<int> EliminarAsync(int id);
}