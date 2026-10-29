// =============================================================================
// QUÉ ES: modelo de la relación programa_ci.
// PAPEL: representa la fila SQL y los nombres de sus maestros para la interfaz.
// =============================================================================
namespace ApiInnovacion.Modelos;

public class ProgramaCi
{
    public required int Programa { get; set; }
    public required int CarInnovacion { get; set; }
    public string? ProgramaNombre { get; set; }
    public string? CarInnovacionNombre { get; set; }
    public bool Activo { get; set; } = true;
}
