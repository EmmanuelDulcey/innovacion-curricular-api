using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Controllers;

/// <summary>
/// Controller de `registro_calificado` (v2): PK `codigo`, ruta
/// /api/registro_calificado/{codigo}. No toca SQL: traduce errores de dominio
/// a HTTP, incluido el 409 tanto de FK inexistente como de padre con puentes
/// activas al retirar.
/// </summary>
[Route("api/registro_calificado")]
[ApiController]
public class RegistroCalificadoController : ControllerBase
{
    private readonly IServicioRegistroCalificado _servicio;

    public RegistroCalificadoController(IServicioRegistroCalificado servicio)
    {
        _servicio = servicio;
    }

    // GET /api/registro_calificado?limite=N
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

            return Ok(new RespuestaLista<RegistroCalificado>
            {
                Tabla = "registro_calificado",
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

    // GET /api/registro_calificado/{codigo}
    [HttpGet("{codigo:int}")]
    public async Task<IActionResult> Obtener(int codigo)
    {
        try
        {
            var registro = await _servicio.ObtenerPorIdAsync(codigo);
            return Ok(registro);
        }
        catch (Exception ex)
        {
            return TraducirError(ex, "Error del servidor.");
        }
    }

    // POST /api/registro_calificado
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] RegistroCalificadoCrear peticion)
    {
        try
        {
            var creado = await _servicio.CrearAsync(peticion);
            return Ok(new RespuestaCreacion<RegistroCalificado>
            {
                Estado = StatusCodes.Status200OK,
                Mensaje = "Registro creado exitosamente.",
                Datos = creado
            });
        }
        catch (Exception ex)
        {
            return TraducirError(ex, "Error al crear el registro.");
        }
    }

    // PUT /api/registro_calificado/{codigo}  (reemplazo completo)
    [HttpPut("{codigo:int}")]
    public async Task<IActionResult> Reemplazar(int codigo, [FromBody] RegistroCalificadoReemplazo peticion)
    {
        try
        {
            var filas = await _servicio.ReemplazarAsync(codigo, peticion);
            return Ok(new RespuestaFilasAfectadas
            {
                Estado = StatusCodes.Status200OK,
                Mensaje = "Registro reemplazado exitosamente.",
                FilasAfectadas = filas
            });
        }
        catch (Exception ex)
        {
            return TraducirError(ex, "Error al actualizar el registro.");
        }
    }

    // PATCH /api/registro_calificado/{codigo}  (actualizacion parcial)
    [HttpPatch("{codigo:int}")]
    public async Task<IActionResult> Actualizar(int codigo, [FromBody] RegistroCalificadoActualizar peticion)
    {
        try
        {
            var filas = await _servicio.ActualizarAsync(codigo, peticion);
            return Ok(new RespuestaFilasAfectadas
            {
                Estado = StatusCodes.Status200OK,
                Mensaje = "Registro actualizado exitosamente.",
                FilasAfectadas = filas
            });
        }
        catch (Exception ex)
        {
            return TraducirError(ex, "Error al actualizar el registro.");
        }
    }

    // DELETE /api/registro_calificado/{codigo}  (bloqueado si tiene puentes activas)
    [HttpDelete("{codigo:int}")]
    public async Task<IActionResult> Eliminar(int codigo)
    {
        try
        {
            var filas = await _servicio.EliminarAsync(codigo);
            return Ok(new RespuestaFilasEliminadas
            {
                Estado = StatusCodes.Status200OK,
                Mensaje = "Registro eliminado exitosamente.",
                FilasEliminadas = filas
            });
        }
        catch (Exception ex)
        {
            return TraducirError(ex, "Error al eliminar el registro.");
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
