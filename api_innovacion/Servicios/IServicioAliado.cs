using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Contrato del servicio de negocio de `aliado`. El controlador depende de esta
/// interfaz: no conoce HTTP ni el motor de base de datos
/// (1_constitution.md, Articulo 3).
/// </summary>
public interface IServicioAliado
{
    Task<List<Aliado>> ObtenerTodosAsync(int limite);
    Task<Aliado> ObtenerPorIdAsync(int nit);
    Task CrearAsync(AliadoCrear peticion);
    Task<int> ReemplazarAsync(int nit, AliadoReemplazo peticion);
    Task<int> ActualizarAsync(int nit, AliadoActualizar peticion);
    Task<int> EliminarAsync(int nit);
}
