using System.Text.Json;

namespace ApiInnovacion.Respuestas;

/// <summary>
/// Convierte nombres de propiedades PascalCase a snake_case.
/// La API responde las entidades en snake_case (6_contracts.md §2); las clases
/// entidad usan PascalCase en C#, asi que la serializacion aplica esta policy.
/// </summary>
public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        return string.Concat(
            name.Select((caracter, indice) =>
                indice > 0 && char.IsUpper(caracter)
                    ? "_" + char.ToLowerInvariant(caracter)
                    : char.ToLowerInvariant(caracter).ToString()));
    }
}