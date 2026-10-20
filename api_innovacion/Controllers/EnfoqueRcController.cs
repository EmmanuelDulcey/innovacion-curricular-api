using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Controllers;

/// <summary>
/// Controller de la puente `enfoque_rc` (v2). Una puente NO tiene PUT ni PATCH
/// (contrato §12): solo listar, consultar la pareja, asignar (POST) y retirar
/// (DELETE por las dos claves, en el orden de la PK). Los mensajes de exito son
/// los de pareja del contrato §8.
/// </summary>
[Route("api/enfoque_rc")]
[ApiController]
public class EnfoqueRcController : ControllerBase
{
    private readonly IServicioEnfoqueRc _servicio;

    public EnfoqueRcController(IServicioEnfoqueRc servicio)
    {
        _servicio = servicio;
    }

    // GET /api/enfoque_rc?limite=N
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        if (limite <= 0)
            return BadRequest(new RespuestaError
            {
                Estado = 400,
                Mensaje = "Parámetros inválidos.",
                Detalle = "El parámetro limite debe ser mayor que 0."
            });

        try
        {
            var datos = await _servicio.ObtenerTodosAsync(limite);
            if (datos.Count == 0)
                return NoContent();

            return Ok(new RespuestaLista<EnfoqueRc>
            {
                Tabla = "enfoque_rc",
                Limite = limite,
                Total = datos.Count,
                Datos = datos
            });
        }
        catch (Exception ex)
        {
            return TraducirError(ex, "Error del servidor.");
        }
    }

    // GET /api/enfoque_rc/{enfoque}/{registroCalificado}
    [HttpGet("{enfoque:int}/{registroCalificado:int}")]
    public async Task<IActionResult> Obtener(int enfoque, int registroCalificado)
    {
        try
        {
            var registro = await _servicio.ObtenerPorIdAsync(enfoque, registroCalificado);
            return Ok(registro);
        }
        catch (Exception ex)
        {
            return TraducirError(ex, "Error del servidor.");
        }
    }

    // POST /api/enfoque_rc  (asignar la pareja)
    [HttpPost]
    public async Task<IActionResult> Asignar([FromBody] EnfoqueRcPareja peticion)
    {
        try
        {
            var asignada = await _servicio.AsignarAsync(peticion);
            return Ok(new RespuestaCreacion<EnfoqueRc>
            {
                Estado = StatusCodes.Status200OK,
                Mensaje = "Pareja asignada exitosamente.",
                Datos = asignada
            });
        }
        catch (Exception ex)
        {
            return TraducirError(ex, "Error al asignar la pareja.");
        }
    }

    // DELETE /api/enfoque_rc/{enfoque}/{registroCalificado}  (retirar la pareja)
    [HttpDelete("{enfoque:int}/{registroCalificado:int}")]
    public async Task<IActionResult> Retirar(int enfoque, int registroCalificado)
    {
        try
        {
            var filas = await _servicio.RetirarAsync(enfoque, registroCalificado);
            return Ok(new RespuestaFilasEliminadas
            {
                Estado = StatusCodes.Status200OK,
                Mensaje = "Pareja retirada exitosamente.",
                FilasEliminadas = filas
            });
        }
        catch (Exception ex)
        {
            return TraducirError(ex, "Error al retirar la pareja.");
        }
    }

    private IActionResult TraducirError(Exception ex, string mensajeContexto)
    {
        if (ex is NoEncontradoExcepcion)
            return StatusCode(StatusCodes.Status404NotFound, new RespuestaError
            {
                Estado = StatusCodes.Status404NotFound,
                Mensaje = "Registro no encontrado.",
                Detalle = ex.Message
            });

        if (ex is ConflictoExcepcion conflicto)
            return StatusCode(StatusCodes.Status409Conflict, new RespuestaError
            {
                Estado = StatusCodes.Status409Conflict,
                Mensaje = conflicto.MensajeRespuesta,
                Detalle = conflicto.Message
            });

        if (ex is ArgumentException)
            return BadRequest(new RespuestaError
            {
                Estado = StatusCodes.Status400BadRequest,
                Mensaje = "Parámetros inválidos.",
                Detalle = ex.Message
            });

        var detalle = ex is SqlException
            ? $"{ex.Message} (detalle de SQL Server)."
            : ex.Message;

        return StatusCode(StatusCodes.Status500InternalServerError, new RespuestaError
        {
            Estado = StatusCodes.Status500InternalServerError,
            Mensaje = mensajeContexto,
            Detalle = detalle
        });
    }
}
