using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Contrato del servicio de la puente `enfoque_rc` (v2): listar, consultar la
/// pareja, asignar y retirar. Sin PUT ni PATCH (D7): una pareja existe o no.
/// </summary>
public interface IServicioEnfoqueRc
{
    Task<List<EnfoqueRc>> ObtenerTodosAsync(int limite);
    Task<EnfoqueRc> ObtenerPorIdAsync(int enfoque, int registroCalificado);
    Task<EnfoqueRc> AsignarAsync(EnfoqueRcPareja peticion);
    Task<int> RetirarAsync(int enfoque, int registroCalificado);
}
