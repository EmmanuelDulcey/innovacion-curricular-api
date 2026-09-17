using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

// Tres peticiones por tabla (3_plan.md, Seccion 4.2): una por verbo.
//   CarInnovacionCrear      -> POST : todo obligatorio, incluida la llave primaria.
//   CarInnovacionReemplazo  -> PUT  : todo obligatorio, sin la llave (va en la URL).
//   CarInnovacionActualizar -> PATCH: todo opcional.
// Las anotaciones producen el 422 automatico antes de tocar SQL Server
// (6_contracts.md, Seccion 8.6).

/// <summary>Peticion del POST /api/car_innovacion (6_contracts.md, Seccion 4.6).</summary>
public class CarInnovacionCrear
{
    [Required(ErrorMessage = "El campo id es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo id debe ser un entero positivo.")]
    public int? Id { get; set; }

    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo nombre no puede superar los 45 caracteres.")]
    public string? Nombre { get; set; }

    // `descripcion` es VARCHAR(MAX) en la base: obligatoria y sin vacio, pero
    // SIN [StringLength] porque no tiene limite practico en la API
    // (6_contracts.md, Seccion 8.6).
    [Required(ErrorMessage = "El campo descripcion es obligatorio.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }
}

/// <summary>Peticion del PUT /api/car_innovacion/{id} (6_contracts.md, Seccion 5.6).</summary>
public class CarInnovacionReemplazo
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo nombre no puede superar los 45 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo descripcion es obligatorio.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }
}

/// <summary>Peticion del PATCH /api/car_innovacion/{id} (6_contracts.md, Seccion 6.6).</summary>
public class CarInnovacionActualizar
{
    [StringLength(45, ErrorMessage = "El campo nombre no puede superar los 45 caracteres.")]
    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }
}
