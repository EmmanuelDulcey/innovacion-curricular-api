using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiInnovacion.Configuracion;
using ApiInnovacion.Modelos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Emite un JWT HS256 con los datos mínimos de sesión: identificador, usuario,
/// correo, nombre, roles y jti. Los nombres de los claims son cortos y
/// estables (`nameid`, `unique_name`, `role`), y el pipeline los valida con
/// `MapInboundClaims = false` para no depender del mapeo automático de .NET.
/// </summary>
public class ServicioTokenJwt : ITokenJwt
{
    private readonly JwtOpciones _opciones;

    public ServicioTokenJwt(IOptions<JwtOpciones> opciones)
    {
        _opciones = opciones.Value;
    }

    public TokenGenerado Generar(Usuario usuario, IReadOnlyCollection<string> roles)
    {
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Secret));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddMinutes(_opciones.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new("nameid", usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new("unique_name", usuario.Username),
            new("email", usuario.Email),
            new("nombre_completo", usuario.NombreCompleto ?? usuario.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(rol => new Claim("role", rol)));

        var token = new JwtSecurityToken(
            issuer: _opciones.Issuer,
            audience: _opciones.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expira,
            signingCredentials: credenciales);

        return new TokenGenerado(new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
