using System.Text.Json.Serialization;

namespace ApiInnovacion.Respuestas;

/// <summary>
/// Envoltura de los listados: {tabla, limite, total, datos}.
/// Los nombres explicitos (JsonPropertyName) se respetan aunque la politica
/// global de nombres sea snake_case. Fuente: 6_contracts.md §0.3.
/// </summary>
public class RespuestaLista<T>
{
    [JsonPropertyName("tabla")]
    public string Tabla { get; set; } = string.Empty;

    [JsonPropertyName("limite")]
    public int Limite { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("datos")]
    public List<T> Datos { get; set; } = new();
}

/// <summary>
/// Exito de POST: {estado, mensaje}. Fuente: 6_contracts.md §4.
/// </summary>
public class RespuestaExito
{
    [JsonPropertyName("estado")]
    public int Estado { get; set; }

    [JsonPropertyName("mensaje")]
    public string Mensaje { get; set; } = string.Empty;
}

/// <summary>
/// Exito de PUT/PATCH: {estado, mensaje, filasAfectadas}. Fuente: 6_contracts.md §5-6.
/// </summary>
public class RespuestaFilasAfectadas
{
    [JsonPropertyName("estado")]
    public int Estado { get; set; }

    [JsonPropertyName("mensaje")]
    public string Mensaje { get; set; } = string.Empty;

    [JsonPropertyName("filasAfectadas")]
    public int FilasAfectadas { get; set; }
}

/// <summary>
/// Exito de DELETE: {estado, mensaje, filasEliminadas}. Fuente: 6_contracts.md §7.
/// </summary>
public class RespuestaFilasEliminadas
{
    [JsonPropertyName("estado")]
    public int Estado { get; set; }

    [JsonPropertyName("mensaje")]
    public string Mensaje { get; set; } = string.Empty;

    [JsonPropertyName("filasEliminadas")]
    public int FilasEliminadas { get; set; }
}

/// <summary>
/// Error general: {estado, mensaje, detalle}. Fuente: 6_contracts.md §0.5.
/// </summary>
public class RespuestaError
{
    [JsonPropertyName("estado")]
    public int Estado { get; set; }

    [JsonPropertyName("mensaje")]
    public string Mensaje { get; set; } = string.Empty;

    [JsonPropertyName("detalle")]
    public string Detalle { get; set; } = string.Empty;

    [JsonPropertyName("errores")]
    public List<string>? Errores { get; set; }
}

/// <summary>
/// Diagnostico GET /: {mensaje, version, contratos}. Fuente: 6_contracts.md §1.
/// </summary>
public class RespuestaDiagnostico
{
    [JsonPropertyName("mensaje")]
    public string Mensaje { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("contratos")]
    public string Contratos { get; set; } = string.Empty;
}