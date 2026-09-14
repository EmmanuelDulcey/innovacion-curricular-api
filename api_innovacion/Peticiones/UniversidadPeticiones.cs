using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// Peticion del POST /api/universidad: todo obligatorio, incluida la PK (id).
/// Fuente: 6_contracts.md §8.2.
/// </summary>
public class UniversidadCrear
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

    [Required(ErrorMessage = "El campo ciudad es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo ciudad no puede superar los 45 caracteres.")]
    public string? Ciudad { get; set; }
}

/// <summary>
/// Peticion del PUT /api/universidad/{id}: todo obligatorio, sin la PK (va en la URL).
/// Fuente: 6_contracts.md §8.2.
/// </summary>
public class UniversidadReemplazo
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo nombre no puede superar los 60 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo ciudad es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo ciudad no puede superar los 45 caracteres.")]
    public string? Ciudad { get; set; }
}

/// <summary>
/// Peticion del PATCH /api/universidad/{id}: todo opcional.
/// Un body sin campos es 400 (regla de negocio, la decide el servicio).
/// Fuente: 6_contracts.md §8.2.
/// </summary>
public class UniversidadActualizar
{
    [StringLength(60, ErrorMessage = "El campo nombre no puede superar los 60 caracteres.")]
    public string? Nombre { get; set; }

    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [StringLength(45, ErrorMessage = "El campo ciudad no puede superar los 45 caracteres.")]
    public string? Ciudad { get; set; }
}