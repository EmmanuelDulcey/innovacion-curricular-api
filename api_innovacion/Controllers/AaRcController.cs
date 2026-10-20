using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Controllers;

/// <summary>
/// Controller de la puente `aa_rc` (v2). Ruta con las DOS claves largas en el
/// orden de la PK: /api/aa_rc/{activ_academicas_idcurso}/{registro_calificado_codigo}
/// (contrato §0.2). Sin PUT ni PATCH: la pareja se asigna y se retira; sus
/// atributos `componente` y `semestre` viajan en el POST.
/// </summary>
[Route("api/aa_rc")]
[ApiController]
public class AaRcController : ControllerBase
{
    private readonly IServicioAaRc _servicio;

    public AaRcController(IServicioAaRc servicio)
    {
        _servicio = servicio;
    }

    // GET /api/aa_rc?limite=N
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

            return Ok(new RespuestaLista<AaRc>
            {
                Tabla = "aa_rc",
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

    // GET /api/aa_rc/{activAcademicasIdcurso}/{registroCalificadoCodigo}
    [HttpGet("{activAcademicasIdcurso:int}/{registroCalificadoCodigo:int}")]
    public async Task<IActionResult> Obtener(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        try
        {
            var registro = await _servicio.ObtenerPorIdAsync(activAcademicasIdcurso, registroCalificadoCodigo);
            return Ok(registro);
        }
        catch (Exception ex)
        {
            return TraducirError(ex, "Error del servidor.");
        }
    }

    // POST /api/aa_rc  (asignar la pareja con componente y semestre)
    [HttpPost]
    public async Task<IActionResult> Asignar([FromBody] AaRcPareja peticion)
    {
        try
        {
            var asignada = await _servicio.AsignarAsync(peticion);
            return Ok(new RespuestaCreacion<AaRc>
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

    // DELETE /api/aa_rc/{activAcademicasIdcurso}/{registroCalificadoCodigo}
    [HttpDelete("{activAcademicasIdcurso:int}/{registroCalificadoCodigo:int}")]
    public async Task<IActionResult> Retirar(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        try
        {
            var filas = await _servicio.RetirarAsync(activAcademicasIdcurso, registroCalificadoCodigo);
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
