// =============================================================================
// QUÉ ES: validaciones de entrada para el catálogo de docentes.
// PAPEL: el documento identifica a la persona, mientras SQL genera la PK.
// =============================================================================
using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

public class DocenteCrear
{
    [Required(ErrorMessage = "El campo documento es obligatorio.")]
    [StringLength(20, ErrorMessage = "El documento no puede superar los 20 caracteres.")]
    public string? Documento { get; set; }

    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre no puede superar los 120 caracteres.")]
    public string? Nombre { get; set; }
}

public class DocenteReemplazo
{
    [Required(ErrorMessage = "El campo documento es obligatorio.")]
    [StringLength(20, ErrorMessage = "El documento no puede superar los 20 caracteres.")]
    public string? Documento { get; set; }

    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre no puede superar los 120 caracteres.")]
    public string? Nombre { get; set; }
}

public class DocenteActualizar
{
    [StringLength(20, ErrorMessage = "El documento no puede superar los 20 caracteres.")]
    public string? Documento { get; set; }

    [StringLength(120, ErrorMessage = "El nombre no puede superar los 120 caracteres.")]
    public string? Nombre { get; set; }
}
