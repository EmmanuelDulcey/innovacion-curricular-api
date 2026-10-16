// =============================================================================
// QUÉ ES: cuerpo de creación de una vinculación docente-programa.
// PAPEL: valida presencia, longitud y claves positivas antes de entrar al servicio.
// =============================================================================
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiInnovacion.Peticiones;

public class DocenteDepartamentoCrear
{
    [Required(ErrorMessage = "El campo docente es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo docente debe ser un entero positivo.")]
    public int? Docente { get; set; }

    [Required(ErrorMessage = "El campo departamento es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo departamento debe ser un entero positivo.")]
    public int? Departamento { get; set; }

    [Required(ErrorMessage = "El campo dedicacion es obligatorio.")]
    [StringLength(15, ErrorMessage = "El campo dedicacion no puede superar los 15 caracteres.")]
    public string? Dedicacion { get; set; }

    [Required(ErrorMessage = "El campo modalidad es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo modalidad no puede superar los 45 caracteres.")]
    public string? Modalidad { get; set; }

    [Required(ErrorMessage = "El campo fecha_ingreso es obligatorio.")]
    public DateTime? FechaIngreso { get; set; }

    public DateTime? FechaSalida { get; set; }
}

// =============================================================================
// QUÉ ES: cuerpo de reemplazo completo de los datos propios de una vinculación.
// PAPEL: conserva docente-programa como identidad y valida el resto del PUT.
// =============================================================================
public class DocenteDepartamentoReemplazo
{
    [Required(ErrorMessage = "El campo dedicacion es obligatorio.")]
    [StringLength(15, ErrorMessage = "El campo dedicacion no puede superar los 15 caracteres.")]
    public string? Dedicacion { get; set; }

    [Required(ErrorMessage = "El campo modalidad es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo modalidad no puede superar los 45 caracteres.")]
    public string? Modalidad { get; set; }

    [Required(ErrorMessage = "El campo fecha_ingreso es obligatorio.")]
    public DateTime? FechaIngreso { get; set; }

    public DateTime? FechaSalida { get; set; }
}

// =============================================================================
// QUÉ ES: cuerpo parcial para actualizar una vinculación existente.
// PAPEL: permite cambiar sus atributos sin alterar la pareja de claves.
// =============================================================================
public class DocenteDepartamentoActualizar : IValidatableObject
{
    private string? _dedicacion;
    private string? _modalidad;
    private DateTime? _fechaIngreso;
    private DateTime? _fechaSalida;

    [StringLength(15, ErrorMessage = "El campo dedicacion no puede superar los 15 caracteres.")]
    public string? Dedicacion
    {
        get => _dedicacion;
        set { _dedicacion = value; DedicacionEnviada = true; }
    }

    [StringLength(45, ErrorMessage = "El campo modalidad no puede superar los 45 caracteres.")]
    public string? Modalidad
    {
        get => _modalidad;
        set { _modalidad = value; ModalidadEnviada = true; }
    }

    public DateTime? FechaIngreso
    {
        get => _fechaIngreso;
        set { _fechaIngreso = value; FechaIngresoEnviada = true; }
    }

    public DateTime? FechaSalida
    {
        get => _fechaSalida;
        set { _fechaSalida = value; FechaSalidaEnviada = true; }
    }

    [JsonIgnore]
    public bool DedicacionEnviada { get; private set; }

    [JsonIgnore]
    public bool ModalidadEnviada { get; private set; }

    [JsonIgnore]
    public bool FechaIngresoEnviada { get; private set; }

    [JsonIgnore]
    public bool FechaSalidaEnviada { get; private set; }

    [JsonIgnore]
    public bool TieneCamposEnviados => DedicacionEnviada || ModalidadEnviada || FechaIngresoEnviada || FechaSalidaEnviada;

    // Solo se rechaza null cuando el cliente intenta borrar un campo obligatorio.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DedicacionEnviada && string.IsNullOrWhiteSpace(Dedicacion))
            yield return new ValidationResult("El campo dedicacion no puede quedar vacío.", new[] { nameof(Dedicacion) });

        if (ModalidadEnviada && string.IsNullOrWhiteSpace(Modalidad))
            yield return new ValidationResult("El campo modalidad no puede quedar vacío.", new[] { nameof(Modalidad) });

        if (FechaIngresoEnviada && FechaIngreso is null)
            yield return new ValidationResult("El campo fecha_ingreso no puede quedar vacío.", new[] { nameof(FechaIngreso) });
    }
}
