using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// Peticion del POST /api/premio: datos requeridos y la
/// FK (programa). Fuente: docs/spec_kit/versiones/v2/6_contracts.md, Sección 8.
/// </summary>
public class PremioCrear
{

    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo nombre no puede superar los 45 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo descripcion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo descripcion no puede superar los 45 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El campo fecha es obligatorio.")]
    public DateTime? Fecha { get; set; }

    [Required(ErrorMessage = "El campo entidad_otorgante es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo entidad_otorgante no puede superar los 45 caracteres.")]
    public string? EntidadOtorgante { get; set; }

    [Required(ErrorMessage = "El campo pais es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo pais no puede superar los 45 caracteres.")]
    public string? Pais { get; set; }

    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo programa debe ser un entero positivo.")]
    public int? Programa { get; set; }
}

/// <summary>
/// Peticion del PUT /api/premio/{id}: todo obligatorio, sin la PK (va en la URL).
/// Fuente: docs/spec_kit/versiones/v2/6_contracts.md, Sección 8.
/// </summary>
public class PremioReemplazo
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo nombre no puede superar los 45 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo descripcion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo descripcion no puede superar los 45 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El campo fecha es obligatorio.")]
    public DateTime? Fecha { get; set; }

    [Required(ErrorMessage = "El campo entidad_otorgante es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo entidad_otorgante no puede superar los 45 caracteres.")]
    public string? EntidadOtorgante { get; set; }

    [Required(ErrorMessage = "El campo pais es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo pais no puede superar los 45 caracteres.")]
    public string? Pais { get; set; }

    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo programa debe ser un entero positivo.")]
    public int? Programa { get; set; }
}

/// <summary>
/// Peticion del PATCH /api/premio/{id}: todo opcional.
/// Un body sin campos es 400 (regla de negocio, la decide el servicio).
/// Fuente: docs/spec_kit/versiones/v2/6_contracts.md, Sección 8.
/// </summary>
public class PremioActualizar
{
    [StringLength(45, ErrorMessage = "El campo nombre no puede superar los 45 caracteres.")]
    public string? Nombre { get; set; }

    [StringLength(45, ErrorMessage = "El campo descripcion no puede superar los 45 caracteres.")]
    public string? Descripcion { get; set; }

    public DateTime? Fecha { get; set; }

    [StringLength(45, ErrorMessage = "El campo entidad_otorgante no puede superar los 45 caracteres.")]
    public string? EntidadOtorgante { get; set; }

    [StringLength(45, ErrorMessage = "El campo pais no puede superar los 45 caracteres.")]
    public string? Pais { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo programa debe ser un entero positivo.")]
    public int? Programa { get; set; }
}
