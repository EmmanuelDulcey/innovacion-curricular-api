// =============================================================================
// QUÉ ES: modelo del catálogo local de docentes.
// PAPEL: separa el ID interno generado del documento de identidad real.
// =============================================================================
namespace ApiInnovacion.Modelos;

public class Docente
{
    public int Id { get; set; }
    public string Documento { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
