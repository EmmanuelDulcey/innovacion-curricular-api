using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

/// <summary>Token JWT firmado y su instante de expiración en UTC.</summary>
public sealed record TokenGenerado(string Token, DateTime Expira);

/// <summary>
/// Contrato del emisor de JWT. El servicio recibe al usuario ya autenticado y
/// sus roles; no valida credenciales (eso es responsabilidad del servicio de
/// autenticación).
/// </summary>
public interface ITokenJwt
{
    TokenGenerado Generar(Usuario usuario, IReadOnlyCollection<string> roles);
}
