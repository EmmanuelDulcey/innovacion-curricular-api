// =============================================================================
// QUÉ ES: modelo de la relación an_programa.
// PAPEL: representa la fila SQL y los nombres de sus maestros para la interfaz.
// =============================================================================
namespace ApiInnovacion.Modelos;

public class AnPrograma
{
    public required int AspectoNormativo { get; set; }
    public required int Programa { get; set; }
    public string? AspectoNormativoNombre { get; set; }
    public string? ProgramaNombre { get; set; }
    public bool Activo { get; set; } = true;
}
