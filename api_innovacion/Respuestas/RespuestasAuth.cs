using System.Text.Json.Serialization;

namespace ApiInnovacion.Respuestas;

/// <summary>
/// Datos públicos del usuario autenticado. Nunca incluye la contraseña ni su
/// hash. Fuente: contrato v3 de login/sesión.
/// </summary>
public class UsuarioAutenticadoRespuesta
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("nombre_completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Respuesta del POST /api/login: {estado, mensaje, token, tipo_token, expira,
/// usuario}. El cliente guarda el token y lo envía como Bearer.
/// </summary>
public class RespuestaLogin
{
    [JsonPropertyName("estado")]
    public int Estado { get; set; }

    [JsonPropertyName("mensaje")]
    public string Mensaje { get; set; } = string.Empty;

    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("tipo_token")]
    public string TipoToken { get; set; } = "Bearer";

    [JsonPropertyName("expira")]
    public DateTimeOffset Expira { get; set; }

    [JsonPropertyName("usuario")]
    public UsuarioAutenticadoRespuesta Usuario { get; set; } = new();
}
