using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Contrato del servicio de negocio de `acreditacion` (v2). La PK de la tabla
/// es `resolucion` y asi viaja tambien en la ruta y en los metodos.
/// </summary>
public interface IServicioAcreditacion
{
    Task<List<Acreditacion>> ObtenerTodosAsync(int limite);
    Task<Acreditacion> ObtenerPorIdAsync(int resolucion);
    Task<Acreditacion> CrearAsync(AcreditacionCrear peticion);
    Task<int> ReemplazarAsync(int resolucion, AcreditacionReemplazo peticion);
    Task<int> ActualizarAsync(int resolucion, AcreditacionActualizar peticion);
    Task<int> EliminarAsync(int resolucion);
}
