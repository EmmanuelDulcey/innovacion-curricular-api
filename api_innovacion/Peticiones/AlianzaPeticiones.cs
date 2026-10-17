// =============================================================================
// QUÉ ES: cuerpo de creación de una alianza.
// PAPEL: valida presencia, longitud y claves positivas antes de entrar al servicio.
// =============================================================================
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiInnovacion.Peticiones;

public class AlianzaCrear
{
    [Required(ErrorMessage = "El campo aliado es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo aliado debe ser un entero positivo.")]
    public int? Aliado { get; set; }

    [Required(ErrorMessage = "El campo departamento es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo departamento debe ser un entero positivo.")]
    public int? Departamento { get; set; }

    [Required(ErrorMessage = "El campo fecha_inicio es obligatorio.")]
    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo docente debe ser un entero positivo.")]
    public int? Docente { get; set; }
}

// =============================================================================
// QUÉ ES: cuerpo de reemplazo completo de los datos propios de una alianza.
// PAPEL: mantiene la pareja aliado-programa en la ruta y valida el resto del PUT.
// =============================================================================
public class AlianzaReemplazo
{
    [Required(ErrorMessage = "El campo fecha_inicio es obligatorio.")]
    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo docente debe ser un entero positivo.")]
    public int? Docente { get; set; }
}

// =============================================================================
// QUÉ ES: cuerpo parcial para actualizar una alianza existente.
// PAPEL: distingue campo ausente de null explícito para permitir limpiar datos
// opcionales sin cambiar la pareja que identifica la alianza.
// =============================================================================
public class AlianzaActualizar : IValidatableObject
{
    private DateTime? _fechaInicio;
    private DateTime? _fechaFin;
    private int? _docente;

    public DateTime? FechaInicio
    {
        get => _fechaInicio;
        set { _fechaInicio = value; FechaInicioEnviada = true; }
    }

    public DateTime? FechaFin
    {
        get => _fechaFin;
        set { _fechaFin = value; FechaFinEnviada = true; }
    }

    [Range(1, int.MaxValue, ErrorMessage = "El campo docente debe ser un entero positivo.")]
    public int? Docente
    {
        get => _docente;
        set { _docente = value; DocenteEnviado = true; }
    }

    [JsonIgnore]
    public bool FechaInicioEnviada { get; private set; }

    [JsonIgnore]
    public bool FechaFinEnviada { get; private set; }

    [JsonIgnore]
    public bool DocenteEnviado { get; private set; }

    [JsonIgnore]
    public bool TieneCamposEnviados => FechaInicioEnviada || FechaFinEnviada || DocenteEnviado;

    // Un null explícito no puede borrar el único dato obligatorio de fecha.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaInicioEnviada && FechaInicio is null)
            yield return new ValidationResult("El campo fecha_inicio no puede quedar vacío.", new[] { nameof(FechaInicio) });
    }
}
