namespace ApiInnovacion.Excepciones;

/// <summary>
/// Excepcion de negocio de la v2: la operacion choca con una regla de la base
/// (FK inexistente, clave duplicada o retiro bloqueado por hijos activos). El
/// controlador la traduce a HTTP 409.
///
/// <c>Mensaje</c> (heredado) es el detalle tecnico/legible del choque;
/// <c>MensajeRespuesta</c> es el mensaje corto que viaja en el campo `mensaje`
/// de la respuesta. Fuente: docs/spec_kit/versiones/v2/3_plan.md §4.6.
/// </summary>
public class ConflictoExcepcion : Exception
{
    public string MensajeRespuesta { get; }

    public ConflictoExcepcion(string detalle, string mensajeRespuesta = "Conflicto con los datos.")
        : base(detalle)
    {
        MensajeRespuesta = mensajeRespuesta;
    }
}
