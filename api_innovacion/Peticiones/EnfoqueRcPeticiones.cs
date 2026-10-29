using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// Peticion del POST /api/enfoque_rc (v2): una tabla puente solo se asigna, no
/// se edita (tropiezo 9), por eso hay UNA sola clase: la pareja. No hay
/// Reemplazo ni Actualizar porque las puentes no tienen PUT ni PATCH.
/// Fuente: docs/spec_kit/versiones/v2/6_contracts.md §8.
/// </summary>
public class EnfoqueRcPareja
{
    [Required(ErrorMessage = "El campo enfoque es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo enfoque debe ser un entero positivo.")]
    public int? Enfoque { get; set; }

    [Required(ErrorMessage = "El campo registro_calificado es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo registro_calificado debe ser un entero positivo.")]
    public int? RegistroCalificado { get; set; }
}
