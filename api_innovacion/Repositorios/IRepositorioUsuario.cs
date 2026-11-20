using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Acceso a los usuarios y sus roles. Las consultas se resuelven con
/// procedimientos almacenados (misma regla que el resto de la v2/v3); el
/// repositorio nunca devuelve la contraseña hacia capas superiores más allá
/// del servicio de autenticación.
/// </summary>
public interface IRepositorioUsuario
{
    Task<Usuario?> ObtenerPorUsernameAsync(string username);
    Task<List<string>> ObtenerRolesAsync(int usuarioId);
}
