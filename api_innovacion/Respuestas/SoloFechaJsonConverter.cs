using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiInnovacion.Respuestas;

/// <summary>
/// Serializa los <see cref="DateTime"/> como fecha simple "yyyy-MM-dd", sin hora
/// ni zona. Las columnas DATE de la base (fecha_fun, premio.fecha) no llevan
/// hora; sin esto el JSON agregaria "T00:00:00" y el &lt;input type="date"&gt; del
/// frontend no lo aceptaria. Fuente: docs/spec_kit/versiones/v2/6_contracts.md §0.4.
/// </summary>
public class SoloFechaJsonConverter : JsonConverter<DateTime>
{
    private const string Formato = "yyyy-MM-dd";

    public override DateTime Read(ref Utf8JsonReader reader, Type tipoDestino, JsonSerializerOptions opciones)
    {
        return DateTime.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime valor, JsonSerializerOptions opciones)
    {
        writer.WriteStringValue(valor.ToString(Formato, CultureInfo.InvariantCulture));
    }
}
