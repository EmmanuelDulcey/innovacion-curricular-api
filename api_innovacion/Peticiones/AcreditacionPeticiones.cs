using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// Peticiones del CRUD de `acreditacion` (v2): Crear, Reemplazo y Actualizar.
/// La PK se llama `resolucion` (no `id`); sus fechas son TEXTO (VARCHAR(45)),
/// igual que en el script (6_contracts.md §9 y 5_data_model.md §3.3).
/// </summary>
public class AcreditacionCrear
{
    [Required(ErrorMessage = "El campo resolucion es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo resolucion debe ser un entero positivo.")]
    public int? Resolucion { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo calificacion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo calificacion no puede superar los 45 caracteres.")]
    public string? Calificacion { get; set; }

    [Required(ErrorMessage = "El campo fecha_inicio es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo fecha_inicio no puede superar los 45 caracteres.")]
    public string? FechaInicio { get; set; }

    [Required(ErrorMessage = "El campo fecha_fin es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo fecha_fin no puede superar los 45 caracteres.")]
    public string? FechaFin { get; set; }

    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo programa debe ser un entero positivo.")]
    public int? Programa { get; set; }
}

/// <summary>
/// Peticion del PUT /api/acreditacion/{resolucion}: reemplazo completo; la PK
/// va en la URL. Fuente: 6_contracts.md §5.
/// </summary>
public class AcreditacionReemplazo
{
    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo calificacion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo calificacion no puede superar los 45 caracteres.")]
    public string? Calificacion { get; set; }

    [Required(ErrorMessage = "El campo fecha_inicio es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo fecha_inicio no puede superar los 45 caracteres.")]
    public string? FechaInicio { get; set; }

    [Required(ErrorMessage = "El campo fecha_fin es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo fecha_fin no puede superar los 45 caracteres.")]
    public string? FechaFin { get; set; }

    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo programa debe ser un entero positivo.")]
    public int? Programa { get; set; }
}

/// <summary>
/// Peticion del PATCH /api/acreditacion/{resolucion}: todo opcional; body sin
/// campos -> 400 (regla de negocio del servicio). Fuente: 6_contracts.md §6.
/// </summary>
public class AcreditacionActualizar
{
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [StringLength(45, ErrorMessage = "El campo calificacion no puede superar los 45 caracteres.")]
    public string? Calificacion { get; set; }

    [StringLength(45, ErrorMessage = "El campo fecha_inicio no puede superar los 45 caracteres.")]
    public string? FechaInicio { get; set; }

    [StringLength(45, ErrorMessage = "El campo fecha_fin no puede superar los 45 caracteres.")]
    public string? FechaFin { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo programa debe ser un entero positivo.")]
    public int? Programa { get; set; }
}
