namespace ApiInnovacion.Modelos;

/// <summary>
/// Fila de la tabla `usuario` (v3). `Password` guarda SIEMPRE el hash bcrypt,
/// nunca la contraseña en texto plano. Los roles se consultan aparte desde
/// `rol_usuario` para no acoplar la entidad al modelo de autorización.
/// </summary>
public class Usuario
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
    public string? NombreCompleto { get; set; }
    public bool Activo { get; set; }
}
