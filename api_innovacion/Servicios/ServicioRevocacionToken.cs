using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Implementación por lista negra en base de datos: revocar guarda el jti;
/// consultar comprueba si ese jti ya fue guardado. Un jti vacío nunca se
/// considera revocado (evita una consulta sin sentido si el token no trae
/// claim `jti`).
/// </summary>
public class ServicioRevocacionToken : IServicioRevocacionToken
{
    private readonly IRepositorioTokenRevocado _repositorio;

    public ServicioRevocacionToken(IRepositorioTokenRevocado repositorio)
    {
        _repositorio = repositorio;
    }

    public Task RevocarAsync(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti))
            return Task.CompletedTask;

        return _repositorio.RevocarAsync(jti);
    }

    public Task<bool> EstaRevocadoAsync(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti))
            return Task.FromResult(false);

        return _repositorio.EstaRevocadoAsync(jti);
    }
}
