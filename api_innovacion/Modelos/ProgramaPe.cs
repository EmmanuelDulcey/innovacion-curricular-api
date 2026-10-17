// =============================================================================
// QUÉ ES: modelo de la relación programa_pe.
// PAPEL: representa la fila SQL y los nombres de sus maestros para la interfaz.
// =============================================================================
namespace ApiInnovacion.Modelos;

public class ProgramaPe
{
    public required int Programa { get; set; }
    public required int PracticaEstrategia { get; set; }
    public string? ProgramaNombre { get; set; }
    public string? PracticaEstrategiaNombre { get; set; }
    public bool Activo { get; set; } = true;
}
