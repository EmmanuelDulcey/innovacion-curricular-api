using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Controllers;

/// <summary>
/// Autenticación de la v3: `POST /api/login` emite el JWT; `GET /api/auth/me`
/// devuelve la sesión a partir de los claims y `POST /api/auth/logout` la
/// revoca (ver <see cref="IServicioRevocacionToken"/>): a partir de ese
/// momento el mismo token deja de servir en cualquier ruta, aunque no haya
/// expirado. Los errores de credenciales son 401 genéricos; los fallos del
/// motor nunca filtran detalles internos.
/// </summary>
[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly IServicioAutenticacion _servicio;
    private readonly IServicioRevocacionToken _revocacion;

    public AuthController(IServicioAutenticacion servicio, IServicioRevocacionToken revocacion)
    {
        _servicio = servicio;
        _revocacion = revocacion;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginPeticion peticion)
    {
        try
        {
            var sesion = await _servicio.AutenticarAsync(peticion);
            return Ok(new RespuestaLogin
            {
                Estado = StatusCodes.Status200OK,
                Mensaje = "Inicio de sesión exitoso.",
                Token = sesion.Token,
                // DateTimeOffset evita el converter global de fechas (que
                // recorta los DateTime a yyyy-MM-dd) y conserva hora y zona.
                Expira = new DateTimeOffset(sesion.Expira),
                Usuario = MapearUsuario(sesion.Usuario, sesion.Roles),
            });
        }
        catch (NoAutorizadoExcepcion)
        {
            return Unauthorized(new RespuestaError
            {
                Estado = StatusCodes.Status401Unauthorized,
                Mensaje = "Usuario o contraseña incorrectos.",
                Detalle = string.Empty,
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new RespuestaError
            {
                Estado = StatusCodes.Status400BadRequest,
                Mensaje = "Parámetros inválidos.",
                Detalle = ex.Message,
            });
        }
        catch (SqlException)
        {
            return ErrorInterno();
        }
        catch (Exception)
        {
            return ErrorInterno();
        }
    }

    [HttpGet("auth/me")]
    [Authorize]
    public IActionResult Me()
    {
        var id = User.FindFirstValue("nameid");
        var username = User.FindFirstValue("unique_name") ?? User.Identity?.Name ?? string.Empty;
        var email = User.FindFirstValue("email") ?? string.Empty;
        var nombre = User.FindFirstValue("nombre_completo") ?? username;
        var roles = User.FindAll("role").Select(claim => claim.Value).Distinct().ToList();

        return Ok(new RespuestaCreacion<UsuarioAutenticadoRespuesta>
        {
            Estado = StatusCodes.Status200OK,
            Mensaje = "Sesión activa.",
            Datos = new UsuarioAutenticadoRespuesta
            {
                Id = int.TryParse(id, out var numero) ? numero : 0,
                Username = username,
                Email = email,
                NombreCompleto = nombre,
                Roles = roles,
            },
        });
    }

    [HttpPost("auth/logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // Revoca el jti del token actual: a partir de aquí, OnTokenValidated
        // (Program.cs) lo rechaza en cualquier ruta aunque su firma y su
        // expiración sigan siendo válidas. Un token sin jti (no debería pasar
        // con ServicioTokenJwt) simplemente no se puede revocar.
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (!string.IsNullOrEmpty(jti))
            await _revocacion.RevocarAsync(jti);

        return Ok(new RespuestaExito
        {
            Estado = StatusCodes.Status200OK,
            Mensaje = "Sesión cerrada.",
        });
    }

    private IActionResult ErrorInterno()
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new RespuestaError
        {
            Estado = StatusCodes.Status500InternalServerError,
            Mensaje = "Error del servidor.",
            Detalle = "No se pudo completar el inicio de sesión.",
        });
    }

    private static UsuarioAutenticadoRespuesta MapearUsuario(Usuario usuario, IReadOnlyCollection<string> roles)
    {
        return new UsuarioAutenticadoRespuesta
        {
            Id = usuario.Id,
            Username = usuario.Username,
            Email = usuario.Email,
            NombreCompleto = usuario.NombreCompleto ?? usuario.Username,
            Roles = roles.ToList(),
        };
    }
}
