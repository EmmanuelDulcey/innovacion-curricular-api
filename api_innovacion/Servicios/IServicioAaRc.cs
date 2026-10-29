using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Contrato del servicio de la puente `aa_rc` (v2). Ademas de la pareja, sus
/// peticiones llevan `componente` y `semestre`; como toda puente, solo se
/// asigna y se retira (sin edicion).
/// </summary>
public interface IServicioAaRc
{
    Task<List<AaRc>> ObtenerTodosAsync(int limite);
    Task<AaRc> ObtenerPorIdAsync(int activAcademicasIdcurso, int registroCalificadoCodigo);
    Task<AaRc> AsignarAsync(AaRcPareja peticion);
    Task<int> RetirarAsync(int activAcademicasIdcurso, int registroCalificadoCodigo);
}
