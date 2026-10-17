// =============================================================================
// QUÉ ES: cuerpo de asignación de an_programa.
// PAPEL: valida presencia, longitud y claves positivas antes de entrar al servicio.
// =============================================================================
using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

public class AnProgramaCrear
{
    [Required(ErrorMessage = "El campo aspecto_normativo es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo aspecto_normativo debe ser un entero positivo.")]
    public int? AspectoNormativo { get; set; }

    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo programa debe ser un entero positivo.")]
    public int? Programa { get; set; }
}
