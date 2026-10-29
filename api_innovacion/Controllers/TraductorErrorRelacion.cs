// =============================================================================
// QUÉ ES: conversión HTTP de los errores de las relaciones de v2.
// PAPEL: deja la lógica de negocio en servicios y conserva el sobre de la v1.
// =============================================================================
using ApiInnovacion.Excepciones;
using ApiInnovacion.Respuestas;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

internal static class TraductorErrorRelacion
{
    public static IActionResult Convertir(Exception ex)
    {
        return ex switch
        {
            NoEncontradoExcepcion => new ObjectResult(new RespuestaError
            {
                Estado = 404, Mensaje = "Registro no encontrado.", Detalle = ex.Message
            }) { StatusCode = 404 },
            ConflictoExcepcion conflicto => new ObjectResult(new RespuestaError
            {
                Estado = 409, Mensaje = conflicto.MensajeRespuesta, Detalle = conflicto.Message
            }) { StatusCode = 409 },
            ArgumentException => new BadRequestObjectResult(new RespuestaError
            {
                Estado = 400, Mensaje = "Parámetros inválidos.", Detalle = ex.Message
            }),
            // El detalle interno no se expone en errores inesperados.
            _ => new ObjectResult(new RespuestaError
            {
                Estado = 500, Mensaje = "Error del servidor.",
                Detalle = "No se pudo completar la operación."
            }) { StatusCode = 500 }
        };
    }
}
