namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla `enfoque` (v1, tabla sin FK).
/// La llave primaria es `id`, entero positivo asignado por el cliente.
/// </summary>
public class Enfoque
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Descripcion { get; set; }
    public bool Activo { get; set; } = true;
}
