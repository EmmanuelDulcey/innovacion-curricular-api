namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla `aspecto_normativo` (v1, tabla sin FK).
/// </summary>
public class AspectoNormativo
{
    public required int Id { get; set; }
    public required string Tipo { get; set; }
    public required string Descripcion { get; set; }
    public required string Fuente { get; set; }
    public bool Activo { get; set; } = true;
}
