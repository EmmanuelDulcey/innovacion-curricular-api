using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// Peticion del POST /api/aa_rc (v2): la pareja de la puente MAS sus dos
/// atributos propios, `componente` y `semestre`, que son obligatorios
/// (6_contracts.md §8, nota de puentes con atributos). Como toda puente, solo
/// se asigna y se retira: no hay edicion posterior.
/// </summary>
public class AaRcPareja
{
    [Required(ErrorMessage = "El campo activ_academicas_idcurso es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo activ_academicas_idcurso debe ser un entero positivo.")]
    public int? ActivAcademicasIdcurso { get; set; }

    [Required(ErrorMessage = "El campo registro_calificado_codigo es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo registro_calificado_codigo debe ser un entero positivo.")]
    public int? RegistroCalificadoCodigo { get; set; }

    [Required(ErrorMessage = "El campo componente es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo componente no puede superar los 45 caracteres.")]
    public string? Componente { get; set; }

    [Required(ErrorMessage = "El campo semestre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo semestre no puede superar los 45 caracteres.")]
    public string? Semestre { get; set; }
}
