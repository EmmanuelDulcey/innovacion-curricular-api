namespace ApiInnovacion.Servicios;

/// <summary>
/// Revocación de sesión (v3): permite invalidar un JWT antes de su
/// expiración natural. No conoce HTTP; lo usa el controlador al cerrar
/// sesión y el pipeline de autenticación en cada petición.
/// </summary>
public interface IServicioRevocacionToken
{
    Task RevocarAsync(string jti);
    Task<bool> EstaRevocadoAsync(string jti);
}
