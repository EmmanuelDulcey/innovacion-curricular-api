namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla `registro_calificado` (v2): detalle de `programa` cuya PK
/// se llama `codigo`. Aqui las fechas SI son DATE en la base, asi que viajan
/// como DateTime y SoloFechaJsonConverter las serializa "yyyy-MM-dd"
/// (5_data_model.md §3.4; el script mezcla DATE y VARCHAR por tabla).
///
/// Es padre de las puentes enfoque_rc y aa_rc: no se retira con parejas activas.
/// La tabla no tiene columna `nombre`; el texto legible que se muestra en los
/// desplegables es "codigo - tipo_titulacion" (lo arma el Frontend y el alias
/// registro_calificado_nombre de los listados de las puentes).
/// </summary>
public class RegistroCalificado
{
    public required int Codigo { get; set; }
    public required string CantCreditos { get; set; }
    public required string HoraAcom { get; set; }
    public required string HoraInd { get; set; }
    public required string Metodologia { get; set; }
    public required DateTime FechaInicio { get; set; }   // DATE
    public required DateTime FechaFin { get; set; }      // DATE
    public required string DuracionAnios { get; set; }
    public required string DuracionSemestres { get; set; }
    public required string TipoTitulacion { get; set; }
    public required int Programa { get; set; }           // clave que se envia
    public string? ProgramaNombre { get; set; }          // alias del JOIN (solo lectura)
    public bool Activo { get; set; } = true;
}
