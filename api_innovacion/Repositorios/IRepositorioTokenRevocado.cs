namespace ApiInnovacion.Repositorios;

/// <summary>
/// Lista negra de tokens cerrados por logout (v3). Un jti presente aquí es
/// un JWT que ya no debe aceptarse, aunque su firma y su expiración sigan
/// siendo válidas.
/// </summary>
public interface IRepositorioTokenRevocado
{
    Task RevocarAsync(string jti);
    Task<bool> EstaRevocadoAsync(string jti);
}
