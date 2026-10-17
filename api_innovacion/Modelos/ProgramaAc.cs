// =============================================================================
// QUÉ ES: modelo de la relación programa_ac.
// PAPEL: representa la fila SQL y los nombres de sus maestros para la interfaz.
// =============================================================================
namespace ApiInnovacion.Modelos;

public class ProgramaAc
{
    public required int Programa { get; set; }
    public required int AreaConocimiento { get; set; }
    public string? ProgramaNombre { get; set; }
    public string? AreaConocimientoNombre { get; set; }
    public bool Activo { get; set; } = true;
}
