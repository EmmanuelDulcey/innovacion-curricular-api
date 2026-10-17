namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla `acreditacion` (v2): detalle de `programa` cuya PK se
/// llama `resolucion` (no `id`). Sus fechas son TEXTO (VARCHAR(45)) segun el
/// script provisto (5_data_model.md §3.3).
///
/// Es la tabla del disparador: cada INSERT, UPDATE o retirada (activo = 0)
/// recalcula `programa.num_acreditaciones`; la API nunca escribe ese contador.
/// </summary>
public class Acreditacion
{
    public required int Resolucion { get; set; }
    public required string Tipo { get; set; }
    public required string Calificacion { get; set; }
    public required string FechaInicio { get; set; }  // texto, no DATE
    public required string FechaFin { get; set; }     // texto, no DATE
    public required int Programa { get; set; }        // clave que se envia
    public string? ProgramaNombre { get; set; }       // alias del JOIN (solo lectura)
    public bool Activo { get; set; } = true;
}
