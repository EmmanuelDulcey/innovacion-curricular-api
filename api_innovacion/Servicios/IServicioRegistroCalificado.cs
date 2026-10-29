using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Contrato del servicio de negocio de `registro_calificado` (v2). Su PK es
/// `codigo`; es padre de las puentes enfoque_rc y aa_rc, asi que su retiro se
/// bloquea con 409 mientras tenga parejas activas.
/// </summary>
public interface IServicioRegistroCalificado
{
    Task<List<RegistroCalificado>> ObtenerTodosAsync(int limite);
    Task<RegistroCalificado> ObtenerPorIdAsync(int codigo);
    Task<RegistroCalificado> CrearAsync(RegistroCalificadoCrear peticion);
    Task<int> ReemplazarAsync(int codigo, RegistroCalificadoReemplazo peticion);
    Task<int> ActualizarAsync(int codigo, RegistroCalificadoActualizar peticion);
    Task<int> EliminarAsync(int codigo);
}
