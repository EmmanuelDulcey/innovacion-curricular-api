using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Controllers;

/// <summary>
/// Controller de `activ_academica` (v2): ruta base api/activ_academica y los
/// 6 verbos. La FK `disenio` es opcional: el body la puede traer null y el
/// servicio la propaga tal cual. No toca SQL: traduce errores de dominio a HTTP.
/// </summary>
[Route("api/activ_academica")]
[ApiController]
public class ActivAcademicaController : ControllerBase
{
    private readonly IServicioActivAcademica _servicio;

    public ActivAcademicaController(IServicioActivAcademica servicio)
    {
        _servicio = servicio;
    }

    // GET /api/activ_academica?limite=N
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

            return Ok(new RespuestaLista<ActivAcademica>
            {
                Tabla = "activ_academica",
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

    // GET /api/activ_academica/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        try
        {
            var registro = await _servicio.ObtenerPorIdAsync(id);
            return Ok(registro);
        }
        catch (Exception ex)
        {
            return TraducirError(ex, "Error del servidor.");
        }
    }

    // POST /api/activ_academica
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ActivAcademicaCrear peticion)
    {
        try
        {
            var creado = await _servicio.CrearAsync(peticion);
            return Ok(new RespuestaCreacion<ActivAcademica>
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

    // PUT /api/activ_academica/{id}  (reemplazo completo)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Reemplazar(int id, [FromBody] ActivAcademicaReemplazo peticion)
    {
        try
        {
            var filas = await _servicio.ReemplazarAsync(id, peticion);
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

    // PATCH /api/activ_academica/{id}  (actualizacion parcial)
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActivAcademicaActualizar peticion)
    {
        try
        {
            var filas = await _servicio.ActualizarAsync(id, peticion);
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

    // DELETE /api/activ_academica/{id}  (bloqueado si tiene parejas activas en aa_rc)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var filas = await _servicio.EliminarAsync(id);
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
