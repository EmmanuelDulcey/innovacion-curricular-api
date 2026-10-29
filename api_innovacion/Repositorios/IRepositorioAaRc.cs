using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Contrato de datos de la puente `aa_rc` (v2): asignar y retirar por pareja,
/// sin edicion (D7). Los nombres de la PK son los largos del script
/// (`activ_academicas_idcurso`, `registro_calificado_codigo`) y se usan tal
/// cual en metodos y parametros (5_data_model.md §3.13).
/// </summary>
public interface IRepositorioAaRc
{
    Task<List<AaRc>> ObtenerTodosAsync(int limite);
    Task<AaRc?> ObtenerPorIdAsync(int activAcademicasIdcurso, int registroCalificadoCodigo);
    Task<AaRc> AsignarAsync(AaRc entidad);
    Task<int> RetirarAsync(int activAcademicasIdcurso, int registroCalificadoCodigo);
}
