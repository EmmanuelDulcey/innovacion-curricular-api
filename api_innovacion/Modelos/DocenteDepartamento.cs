// =============================================================================
// QUÉ ES: modelo de la relación docente_departamento.
// PAPEL: representa la fila SQL y los nombres de sus maestros para la interfaz.
// =============================================================================
namespace ApiInnovacion.Modelos;

public class DocenteDepartamento
{
    public required int Docente { get; set; }
    public required int Departamento { get; set; }
    public required string Dedicacion { get; set; } = string.Empty;
    public required string Modalidad { get; set; } = string.Empty;
    public required DateTime FechaIngreso { get; set; }
    public DateTime? FechaSalida { get; set; }
    public string? DocenteNombre { get; set; }
    public string? DepartamentoNombre { get; set; }
    public bool Activo { get; set; } = true;
}
