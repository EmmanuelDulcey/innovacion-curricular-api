using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// Peticion del POST /api/facultad: todo obligatorio, incluida la PK (id) y la
/// FK (universidad). Fuente: docs/spec_kit/versiones/v2/6_contracts.md, Sección 8.
/// </summary>
public class FacultadCrear
{
    [Required(ErrorMessage = "El campo id es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo id debe ser un entero positivo.")]
    public int? Id { get; set; }

    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo nombre no puede superar los 60 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo fecha_fun es obligatorio.")]
    public DateTime? FechaFun { get; set; }

    [Required(ErrorMessage = "El campo universidad es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo universidad debe ser un entero positivo.")]
    public int? Universidad { get; set; }
}

/// <summary>
/// Peticion del PUT /api/facultad/{id}: todo obligatorio, sin la PK (va en la URL).
/// Fuente: docs/spec_kit/versiones/v2/6_contracts.md, Sección 8.
/// </summary>
public class FacultadReemplazo
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo nombre no puede superar los 60 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo fecha_fun es obligatorio.")]
    public DateTime? FechaFun { get; set; }

    [Required(ErrorMessage = "El campo universidad es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo universidad debe ser un entero positivo.")]
    public int? Universidad { get; set; }
}

/// <summary>
/// Peticion del PATCH /api/facultad/{id}: todo opcional.
/// Un body sin campos es 400 (regla de negocio, la decide el servicio).
/// Fuente: docs/spec_kit/versiones/v2/6_contracts.md, Sección 8.
/// </summary>
public class FacultadActualizar
{
    [StringLength(60, ErrorMessage = "El campo nombre no puede superar los 60 caracteres.")]
    public string? Nombre { get; set; }

    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    public DateTime? FechaFun { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo universidad debe ser un entero positivo.")]
    public int? Universidad { get; set; }
}
