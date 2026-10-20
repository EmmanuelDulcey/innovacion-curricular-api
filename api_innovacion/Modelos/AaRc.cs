namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla puente `aa_rc` (v2): pareja activ_academica -
/// registro_calificado, con dos atributos propios de la pareja: `componente` y
/// `semestre` (obligatorios). Los nombres de la PK son los largos del script
/// (activ_academicas_idcurso, registro_calificado_codigo) tal cual
/// (5_data_model.md §3.13), y asi viajan en rutas, parametros y alias.
///
/// Como toda puente: se asigna y se retira; no se edita (tropiezo 9).
/// </summary>
public class AaRc
{
    public required int ActivAcademicasIdcurso { get; set; }    // clave 1 (PK)
    public required int RegistroCalificadoCodigo { get; set; }  // clave 2 (PK)
    public required string Componente { get; set; }             // atributo de la pareja
    public required string Semestre { get; set; }               // atributo de la pareja
    public string? ActivAcademicaNombre { get; set; }           // solo lectura
    public string? RegistroCalificadoNombre { get; set; }       // solo lectura
    public bool Activo { get; set; } = true;
}
