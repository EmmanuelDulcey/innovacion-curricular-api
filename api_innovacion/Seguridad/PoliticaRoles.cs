namespace ApiInnovacion.Seguridad;

/// <summary>
/// Regla única de autorización por rol. Escribir (POST/PUT/PATCH/DELETE) está
/// reservado a Administración, Coordinación y Secretaría; Docente y Consulta
/// quedan en solo lectura. La misma regla se replica en el frontend para
/// ocultar acciones, pero la API siempre es la autoridad final.
/// </summary>
public static class PoliticaRoles
{
    public const string Administrador = "Administrador";
    public const string Coordinador = "Coordinador académico";
    public const string Docente = "Docente";
    public const string Secretaria = "Secretaría académica";
    public const string Consulta = "Consulta";

    public static readonly IReadOnlySet<string> RolesEscritura =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Administrador,
            Coordinador,
            Secretaria,
        };

    public static bool PuedeEscribir(IEnumerable<string> roles)
    {
        return roles.Any(RolesEscritura.Contains);
    }
}
