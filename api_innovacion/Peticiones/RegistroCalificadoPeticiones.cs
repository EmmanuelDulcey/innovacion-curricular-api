using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// Peticiones del CRUD de `registro_calificado` (v2): Crear, Reemplazo y
/// Actualizar. La PK se llama `codigo` y aqui las fechas SI son DATE en la base
/// (DateTime en C#), a diferencia de programa y acreditacion que las guardan
/// como texto (5_data_model.md §3.4 y 4_research.md D13: no se "corrige" el
/// esquema, se respeta el script).
/// </summary>
public class RegistroCalificadoCrear
{
    [Required(ErrorMessage = "El campo codigo es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo codigo debe ser un entero positivo.")]
    public int? Codigo { get; set; }

    [Required(ErrorMessage = "El campo cant_creditos es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo cant_creditos no puede superar los 45 caracteres.")]
    public string? CantCreditos { get; set; }

    [Required(ErrorMessage = "El campo hora_acom es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo hora_acom no puede superar los 45 caracteres.")]
    public string? HoraAcom { get; set; }

    [Required(ErrorMessage = "El campo hora_ind es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo hora_ind no puede superar los 45 caracteres.")]
    public string? HoraInd { get; set; }

    [Required(ErrorMessage = "El campo metodologia es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo metodologia no puede superar los 45 caracteres.")]
    public string? Metodologia { get; set; }

    [Required(ErrorMessage = "El campo fecha_inicio es obligatorio.")]
    public DateTime? FechaInicio { get; set; }

    [Required(ErrorMessage = "El campo fecha_fin es obligatorio.")]
    public DateTime? FechaFin { get; set; }

    [Required(ErrorMessage = "El campo duracion_anios es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo duracion_anios no puede superar los 45 caracteres.")]
    public string? DuracionAnios { get; set; }

    [Required(ErrorMessage = "El campo duracion_semestres es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo duracion_semestres no puede superar los 45 caracteres.")]
    public string? DuracionSemestres { get; set; }

    [Required(ErrorMessage = "El campo tipo_titulacion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo_titulacion no puede superar los 45 caracteres.")]
    public string? TipoTitulacion { get; set; }

    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo programa debe ser un entero positivo.")]
    public int? Programa { get; set; }
}

/// <summary>
/// Peticion del PUT /api/registro_calificado/{codigo}: reemplazo completo; la
/// PK va en la URL. Fuente: 6_contracts.md §5.
/// </summary>
public class RegistroCalificadoReemplazo
{
    [Required(ErrorMessage = "El campo cant_creditos es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo cant_creditos no puede superar los 45 caracteres.")]
    public string? CantCreditos { get; set; }

    [Required(ErrorMessage = "El campo hora_acom es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo hora_acom no puede superar los 45 caracteres.")]
    public string? HoraAcom { get; set; }

    [Required(ErrorMessage = "El campo hora_ind es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo hora_ind no puede superar los 45 caracteres.")]
    public string? HoraInd { get; set; }

    [Required(ErrorMessage = "El campo metodologia es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo metodologia no puede superar los 45 caracteres.")]
    public string? Metodologia { get; set; }

    [Required(ErrorMessage = "El campo fecha_inicio es obligatorio.")]
    public DateTime? FechaInicio { get; set; }

    [Required(ErrorMessage = "El campo fecha_fin es obligatorio.")]
    public DateTime? FechaFin { get; set; }

    [Required(ErrorMessage = "El campo duracion_anios es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo duracion_anios no puede superar los 45 caracteres.")]
    public string? DuracionAnios { get; set; }

    [Required(ErrorMessage = "El campo duracion_semestres es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo duracion_semestres no puede superar los 45 caracteres.")]
    public string? DuracionSemestres { get; set; }

    [Required(ErrorMessage = "El campo tipo_titulacion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo tipo_titulacion no puede superar los 45 caracteres.")]
    public string? TipoTitulacion { get; set; }

    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo programa debe ser un entero positivo.")]
    public int? Programa { get; set; }
}

/// <summary>
/// Peticion del PATCH /api/registro_calificado/{codigo}: todo opcional; body sin
/// campos -> 400 (regla de negocio del servicio). Fuente: 6_contracts.md §6.
/// </summary>
public class RegistroCalificadoActualizar
{
    [StringLength(45, ErrorMessage = "El campo cant_creditos no puede superar los 45 caracteres.")]
    public string? CantCreditos { get; set; }

    [StringLength(45, ErrorMessage = "El campo hora_acom no puede superar los 45 caracteres.")]
    public string? HoraAcom { get; set; }

    [StringLength(45, ErrorMessage = "El campo hora_ind no puede superar los 45 caracteres.")]
    public string? HoraInd { get; set; }

    [StringLength(45, ErrorMessage = "El campo metodologia no puede superar los 45 caracteres.")]
    public string? Metodologia { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    [StringLength(45, ErrorMessage = "El campo duracion_anios no puede superar los 45 caracteres.")]
    public string? DuracionAnios { get; set; }

    [StringLength(45, ErrorMessage = "El campo duracion_semestres no puede superar los 45 caracteres.")]
    public string? DuracionSemestres { get; set; }

    [StringLength(45, ErrorMessage = "El campo tipo_titulacion no puede superar los 45 caracteres.")]
    public string? TipoTitulacion { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo programa debe ser un entero positivo.")]
    public int? Programa { get; set; }
}
