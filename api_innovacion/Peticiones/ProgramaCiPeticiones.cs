// =============================================================================
// QUÉ ES: cuerpo de asignación de programa_ci.
// PAPEL: valida presencia, longitud y claves positivas antes de entrar al servicio.
// =============================================================================
using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

public class ProgramaCiCrear
{
    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo programa debe ser un entero positivo.")]
    public int? Programa { get; set; }

    [Required(ErrorMessage = "El campo car_innovacion es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo car_innovacion debe ser un entero positivo.")]
    public int? CarInnovacion { get; set; }
}
