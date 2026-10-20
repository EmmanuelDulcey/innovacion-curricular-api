using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// Peticiones del CRUD de `programa` (v2). Tres clases por tabla, como el molde
/// de la v1: Crear (POST), Reemplazo (PUT) y Actualizar (PATCH).
///
/// Reglas de 6_contracts.md §9.2: las fechas y los contadores son TEXTO
/// (VARCHAR(45)); `fecha_cierre` es el unico campo opcional; `id` y `facultad`
/// son enteros positivos. `num_acreditaciones` NO aparece en ninguna peticion:
/// lo mantiene el disparador y la API nunca lo escribe.
/// Los largos maximos siguen las columnas del script (5_data_model.md §6: la
/// primera muralla valida forma, tipos y largos con 422 antes de tocar la base).
/// </summary>
public class ProgramaCrear
{
    // El id interno no se recibe: lo genera SQL Server (6_contracts.md §4).

    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo nombre no puede superar los 60 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo nivel es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo nivel no puede superar los 45 caracteres.")]
    public string? Nivel { get; set; }

    [Required(ErrorMessage = "El campo fecha_creacion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo fecha_creacion no puede superar los 45 caracteres.")]
    public string? FechaCreacion { get; set; }

    // Opcional: el Frontend envia null (o lo omite), nunca cadena vacia.
    [StringLength(45, ErrorMessage = "El campo fecha_cierre no puede superar los 45 caracteres.")]
    public string? FechaCierre { get; set; }

    [Required(ErrorMessage = "El campo numero_cohortes es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo numero_cohortes no puede superar los 45 caracteres.")]
    public string? NumeroCohortes { get; set; }

    [Required(ErrorMessage = "El campo cant_graduados es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo cant_graduados no puede superar los 45 caracteres.")]
    public string? CantGraduados { get; set; }

    [Required(ErrorMessage = "El campo fecha_actualizacion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo fecha_actualizacion no puede superar los 45 caracteres.")]
    public string? FechaActualizacion { get; set; }

    [Required(ErrorMessage = "El campo ciudad es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo ciudad no puede superar los 45 caracteres.")]
    public string? Ciudad { get; set; }

    [Required(ErrorMessage = "El campo facultad es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo facultad debe ser un entero positivo.")]
    public int? Facultad { get; set; }
}

/// <summary>
/// Peticion del PUT /api/programa/{id}: todos los campos modificables son
/// obligatorios; la PK va en la URL. Fuente: 6_contracts.md §5 y §9.2.
/// </summary>
public class ProgramaReemplazo
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo nombre no puede superar los 60 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo nivel es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo nivel no puede superar los 45 caracteres.")]
    public string? Nivel { get; set; }

    [Required(ErrorMessage = "El campo fecha_creacion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo fecha_creacion no puede superar los 45 caracteres.")]
    public string? FechaCreacion { get; set; }

    [StringLength(45, ErrorMessage = "El campo fecha_cierre no puede superar los 45 caracteres.")]
    public string? FechaCierre { get; set; }

    [Required(ErrorMessage = "El campo numero_cohortes es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo numero_cohortes no puede superar los 45 caracteres.")]
    public string? NumeroCohortes { get; set; }

    [Required(ErrorMessage = "El campo cant_graduados es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo cant_graduados no puede superar los 45 caracteres.")]
    public string? CantGraduados { get; set; }

    [Required(ErrorMessage = "El campo fecha_actualizacion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo fecha_actualizacion no puede superar los 45 caracteres.")]
    public string? FechaActualizacion { get; set; }

    [Required(ErrorMessage = "El campo ciudad es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo ciudad no puede superar los 45 caracteres.")]
    public string? Ciudad { get; set; }

    [Required(ErrorMessage = "El campo facultad es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo facultad debe ser un entero positivo.")]
    public int? Facultad { get; set; }
}

/// <summary>
/// Peticion del PATCH /api/programa/{id}: todo opcional. El servicio lee la
/// fila actual, combina solo los campos enviados y llama al procedimiento de
/// actualizacion; un body sin campos es 400 (lo decide el servicio).
/// Fuente: 6_contracts.md §6 y §9.2.
/// </summary>
public class ProgramaActualizar
{
    [StringLength(60, ErrorMessage = "El campo nombre no puede superar los 60 caracteres.")]
    public string? Nombre { get; set; }

    [StringLength(45, ErrorMessage = "El campo tipo no puede superar los 45 caracteres.")]
    public string? Tipo { get; set; }

    [StringLength(45, ErrorMessage = "El campo nivel no puede superar los 45 caracteres.")]
    public string? Nivel { get; set; }

    [StringLength(45, ErrorMessage = "El campo fecha_creacion no puede superar los 45 caracteres.")]
    public string? FechaCreacion { get; set; }

    [StringLength(45, ErrorMessage = "El campo fecha_cierre no puede superar los 45 caracteres.")]
    public string? FechaCierre { get; set; }

    [StringLength(45, ErrorMessage = "El campo numero_cohortes no puede superar los 45 caracteres.")]
    public string? NumeroCohortes { get; set; }

    [StringLength(45, ErrorMessage = "El campo cant_graduados no puede superar los 45 caracteres.")]
    public string? CantGraduados { get; set; }

    [StringLength(45, ErrorMessage = "El campo fecha_actualizacion no puede superar los 45 caracteres.")]
    public string? FechaActualizacion { get; set; }

    [StringLength(45, ErrorMessage = "El campo ciudad no puede superar los 45 caracteres.")]
    public string? Ciudad { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo facultad debe ser un entero positivo.")]
    public int? Facultad { get; set; }
}
