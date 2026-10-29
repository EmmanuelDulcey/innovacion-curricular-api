using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Controllers;

/// <summary>
/// Controller de `acreditacion` (v2): su PK se llama `resolucion`, asi que la
/// ruta es /api/acreditacion/{resolucion} (contrato §0.2). Igual que los demas:
/// no toca SQL, solo traduce errores de dominio a HTTP (409 incluido).
/// </summary>
[Route("api/acreditacion")]
[ApiController]
public class AcreditacionController : ControllerBase
{
    private readonly IServicioAcreditacion _servicio;

    public AcreditacionController(IServicioAcreditacion servicio)
    {
        _servicio = servicio;
    }

    // GET /api/acreditacion?limite=N
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

            return Ok(new RespuestaLista<Acreditacion>
            {
                Tabla = "acreditacion",
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

    // GET /api/acreditacion/{resolucion}
    [HttpGet("{resolucion:int}")]
    public async Task<IActionResult> Obtener(int resolucion)
    {
        try
        {
            var registro = await _servicio.ObtenerPorIdAsync(resolucion);
            return Ok(registro);
        }
        catch (Exception ex)
        {
            return TraducirError(ex, "Error del servidor.");
        }
    }

    // POST /api/acreditacion
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] AcreditacionCrear peticion)
    {
        try
        {
            var creado = await _servicio.CrearAsync(peticion);
            return Ok(new RespuestaCreacion<Acreditacion>
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

    // PUT /api/acreditacion/{resolucion}  (reemplazo completo)
    [HttpPut("{resolucion:int}")]
    public async Task<IActionResult> Reemplazar(int resolucion, [FromBody] AcreditacionReemplazo peticion)
    {
        try
        {
            var filas = await _servicio.ReemplazarAsync(resolucion, peticion);
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

    // PATCH /api/acreditacion/{resolucion}  (actualizacion parcial)
    [HttpPatch("{resolucion:int}")]
    public async Task<IActionResult> Actualizar(int resolucion, [FromBody] AcreditacionActualizar peticion)
    {
        try
        {
            var filas = await _servicio.ActualizarAsync(resolucion, peticion);
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

    // DELETE /api/acreditacion/{resolucion}  (borrado logico; dispara el contador)
    [HttpDelete("{resolucion:int}")]
    public async Task<IActionResult> Eliminar(int resolucion)
    {
        try
        {
            var filas = await _servicio.EliminarAsync(resolucion);
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
