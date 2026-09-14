using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// Peticion del POST /api/area_conocimiento: todo obligatorio, incluida la PK (id).
/// Fuente: 6_contracts.md §8.1.
/// </summary>
public class AreaConocimientoCrear
{
    [Required(ErrorMessage = "El campo id es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo id debe ser un entero positivo.")]
    public int? Id { get; set; }

    [Required(ErrorMessage = "El campo gran_area es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo gran_area no puede superar los 60 caracteres.")]
    public string? GranArea { get; set; }

    [Required(ErrorMessage = "El campo area es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo area no puede superar los 60 caracteres.")]
    public string? Area { get; set; }

    [Required(ErrorMessage = "El campo disciplina es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo disciplina no puede superar los 60 caracteres.")]
    public string? Disciplina { get; set; }
}

/// <summary>
/// Peticion del PUT /api/area_conocimiento/{id}: todo obligatorio, sin la PK (va en la URL).
/// Fuente: 6_contracts.md §8.1.
/// </summary>
public class AreaConocimientoReemplazo
{
    [Required(ErrorMessage = "El campo gran_area es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo gran_area no puede superar los 60 caracteres.")]
    public string? GranArea { get; set; }

    [Required(ErrorMessage = "El campo area es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo area no puede superar los 60 caracteres.")]
    public string? Area { get; set; }

    [Required(ErrorMessage = "El campo disciplina es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo disciplina no puede superar los 60 caracteres.")]
    public string? Disciplina { get; set; }
}

/// <summary>
/// Peticion del PATCH /api/area_conocimiento/{id}: todo opcional.
/// Un body sin campos es 400 (regla de negocio, la decide el servicio).
/// Fuente: 6_contracts.md §8.1.
/// </summary>
public class AreaConocimientoActualizar
{
    [StringLength(60, ErrorMessage = "El campo gran_area no puede superar los 60 caracteres.")]
    public string? GranArea { get; set; }

    [StringLength(60, ErrorMessage = "El campo area no puede superar los 60 caracteres.")]
    public string? Area { get; set; }

    [StringLength(60, ErrorMessage = "El campo disciplina no puede superar los 60 caracteres.")]
    public string? Disciplina { get; set; }
}