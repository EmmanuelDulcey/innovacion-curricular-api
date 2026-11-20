using System.Text;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Valida usuario + contraseña contra el hash bcrypt almacenado y emite el JWT
/// con los roles vigentes. Aplica tres reglas de seguridad:
///  - mensaje genérico para no revelar si el usuario existe o está inactivo;
///  - verificación de hash incluso cuando el usuario no existe, para no filtrar
///    información por diferencias de tiempo;
///  - límite de 72 bytes de contraseña (el máximo que procesa bcrypt).
/// </summary>
public class ServicioAutenticacion : IServicioAutenticacion
{
    // Hash bcrypt válido de una contraseña aleatoria que nunca se usa para
    // autenticar: solo iguala el costo de verificación cuando el usuario no
    // existe. Evita distinguir usuarios válidos por tiempo de respuesta.
    private const string HashDeRelleno =
        "$2a$12$3yZCpJtn0zU9CKaUnhWHSOon1wlPz7xkHsJ3zy4FSTERsyiuK0ZDu";

    private readonly IRepositorioUsuario _repositorio;
    private readonly IServicioHashContrasena _hash;
    private readonly ITokenJwt _token;

    public ServicioAutenticacion(
        IRepositorioUsuario repositorio,
        IServicioHashContrasena hash,
        ITokenJwt token)
    {
        _repositorio = repositorio;
        _hash = hash;
        _token = token;
    }

    public async Task<SesionUsuario> AutenticarAsync(LoginPeticion peticion)
    {
        var username = peticion.Username?.Trim() ?? string.Empty;
        var contrasena = peticion.Password ?? string.Empty;

        if (username.Length == 0 || contrasena.Length == 0)
            throw new ArgumentException("El usuario y la contraseña son obligatorios.");

        if (Encoding.UTF8.GetByteCount(contrasena) > 72)
            throw new ArgumentException("La contraseña no puede superar los 72 caracteres.");

        var usuario = await _repositorio.ObtenerPorUsernameAsync(username);
        var hash = usuario?.Password ?? HashDeRelleno;
        var contrasenaValida = _hash.Verificar(contrasena, hash);

        if (usuario is null || !usuario.Activo || !contrasenaValida)
            throw new NoAutorizadoExcepcion("Usuario o contraseña incorrectos.");

        var roles = await _repositorio.ObtenerRolesAsync(usuario.Id);
        var token = _token.Generar(usuario, roles);

        return new SesionUsuario(usuario, roles, token.Token, token.Expira);
    }
}
