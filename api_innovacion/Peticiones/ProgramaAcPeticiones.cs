// =============================================================================
// QUÉ ES: cuerpo de asignación de programa_ac.
// PAPEL: valida presencia, longitud y claves positivas antes de entrar al servicio.
// =============================================================================
using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

public class ProgramaAcCrear
{
    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo programa debe ser un entero positivo.")]
    public int? Programa { get; set; }

    [Required(ErrorMessage = "El campo area_conocimiento es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo area_conocimiento debe ser un entero positivo.")]
    public int? AreaConocimiento { get; set; }
}
