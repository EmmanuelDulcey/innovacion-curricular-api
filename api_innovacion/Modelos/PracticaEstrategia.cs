namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla `practica_estrategia` (v1, tabla sin FK).
/// </summary>
public class PracticaEstrategia
{
    public required int Id { get; set; }
    public required string Tipo { get; set; }
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public bool Activo { get; set; } = true;
}
