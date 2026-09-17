using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

public class AspectoNormativoCrear
{
    [Required(ErrorMessage = "El campo id es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo id debe ser un entero positivo.")]
    public int? Id { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo descripcion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo descripcion no puede superar los 45 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El campo fuente es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo fuente no puede superar los 45 caracteres.")]
    public string? Fuente { get; set; }
}

public class AspectoNormativoReemplazo
{
    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo descripcion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo descripcion no puede superar los 45 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El campo fuente es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo fuente no puede superar los 45 caracteres.")]
    public string? Fuente { get; set; }
}

public class AspectoNormativoActualizar
{
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [StringLength(45, ErrorMessage = "El campo descripcion no puede superar los 45 caracteres.")]
    public string? Descripcion { get; set; }

    [StringLength(45, ErrorMessage = "El campo fuente no puede superar los 45 caracteres.")]
    public string? Fuente { get; set; }
}
