namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla `enfoque` (v1, tabla sin FK).
/// La llave primaria es `id`, entero positivo generado por la secuencia de SQL Server.
/// </summary>
public class Enfoque
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public bool Activo { get; set; } = true;
}
