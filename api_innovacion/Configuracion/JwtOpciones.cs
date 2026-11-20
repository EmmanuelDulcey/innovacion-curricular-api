namespace ApiInnovacion.Configuracion;

/// <summary>
/// Configuración del emisor de JWT. El secreto se lee de `Jwt:Secret` y, en
/// Docker, se sobreescribe con la variable de entorno `Jwt__Secret` (o
/// `JWT_SECRET` mapeada en docker-compose). Nunca debe quedar un secreto real
/// en el repositorio: el valor por defecto es solo para desarrollo local.
/// </summary>
public sealed class JwtOpciones
{
    public const string Seccion = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "InnovacionCurricular";
    public string Audience { get; set; } = "InnovacionCurricularFrontend";
    public int ExpirationMinutes { get; set; } = 480;
}
