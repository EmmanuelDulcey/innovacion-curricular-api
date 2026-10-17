// =============================================================================
// QUÉ ES: modelo de la relación alianza.
// PAPEL: representa la fila SQL y los nombres de sus maestros para la interfaz.
// =============================================================================
namespace ApiInnovacion.Modelos;

public class Alianza
{
    public required int Aliado { get; set; }
    public required int Departamento { get; set; }
    public required DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? Docente { get; set; }
    public string? DocenteNombre { get; set; }
    public string? AliadoNombre { get; set; }
    public string? DepartamentoNombre { get; set; }
    public bool Activo { get; set; } = true;
}
