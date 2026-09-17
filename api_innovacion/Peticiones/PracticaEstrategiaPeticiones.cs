using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

public class PracticaEstrategiaCrear
{
    [Required(ErrorMessage = "El campo id es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo id debe ser un entero positivo.")]
    public int? Id { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo nombre no puede superar los 45 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo descripcion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo descripcion no puede superar los 45 caracteres.")]
    public string? Descripcion { get; set; }
}

public class PracticaEstrategiaReemplazo
{
    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo nombre no puede superar los 45 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo descripcion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo descripcion no puede superar los 45 caracteres.")]
    public string? Descripcion { get; set; }
}

public class PracticaEstrategiaActualizar
{
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [StringLength(45, ErrorMessage = "El campo nombre no puede superar los 45 caracteres.")]
    public string? Nombre { get; set; }

    [StringLength(45, ErrorMessage = "El campo descripcion no puede superar los 45 caracteres.")]
    public string? Descripcion { get; set; }
}
