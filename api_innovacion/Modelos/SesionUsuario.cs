namespace ApiInnovacion.Modelos;

/// <summary>
/// Resultado interno de un inicio de sesión: el usuario autenticado, sus roles
/// vigentes y el JWT firmado con su fecha de expiración. El controlador lo
/// convierte en la respuesta HTTP; los servicios no conocen HTTP.
/// </summary>
public sealed record SesionUsuario(
    Usuario Usuario,
    IReadOnlyList<string> Roles,
    string Token,
    DateTime Expira);
