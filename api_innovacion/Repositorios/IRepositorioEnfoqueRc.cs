using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Contrato de datos de la puente `enfoque_rc` (v2). Una puente no tiene el
/// juego CRUD completo: se ASIGNA (crear la pareja) y se RETIRA (borrado
/// logico), sin PUT ni PATCH (4_research.md D7). Por eso no hay
/// `ActualizarAsync`: una pareja existe o no existe.
/// </summary>
public interface IRepositorioEnfoqueRc
{
    Task<List<EnfoqueRc>> ObtenerTodosAsync(int limite);
    Task<EnfoqueRc?> ObtenerPorIdAsync(int enfoque, int registroCalificado);
    Task<EnfoqueRc> AsignarAsync(EnfoqueRc entidad);
    Task<int> RetirarAsync(int enfoque, int registroCalificado);
}
