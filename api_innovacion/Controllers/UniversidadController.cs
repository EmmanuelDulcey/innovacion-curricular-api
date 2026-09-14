using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Controllers;

/// <summary>
/// Controller de `universidad`: ruta base api/universidad y los 6 verbos del
/// CRUD. try/catch estricto traduciendo excepciones a codigos HTTP
/// (422 / 400 / 404 / 500). Fuente: 6_contracts.md y 8_tasks.md Fase 5.
/// </summary>
[Route("api/universidad")]
[ApiController]
public class UniversidadController : ControllerBase
{
    private readonly IServicioUniversidad _servicio;

    public UniversidadController(IServicioUniversidad servicio)
    {
        _servicio = servicio;
    }

    // GET /api/universidad?limite=N  (RF1)
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

            return Ok(new RespuestaLista<Universidad>
            {
                Tabla = "universidad",
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

    // GET /api/universidad/{id}  (RF2)
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

    // POST /api/universidad  (RF3)
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] UniversidadCrear peticion)
    {
        try
        {
            await _servicio.CrearAsync(peticion);
            return Ok(new RespuestaExito
            {
                Estado = StatusCodes.Status200OK,
                Mensaje = "Registro creado exitosamente."
            });
        }
        catch (Exception ex)
        {
            return TraducirError(ex, "Error al crear el registro.");
        }
    }

    // PUT /api/universidad/{id}  (RF4 - reemplazo completo)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Reemplazar(int id, [FromBody] UniversidadReemplazo peticion)
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

    // PATCH /api/universidad/{id}  (RF5 - actualizacion parcial)
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] UniversidadActualizar peticion)
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

    // DELETE /api/universidad/{id}  (RF6 - borrado logico)
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

    // Traduccion estricta de excepciones a codigos HTTP (3_plan.md, Seccion 4.6).
    private IActionResult TraducirError(Exception ex, string mensajeContexto)
    {
        if (ex is NoEncontradoExcepcion)
            return StatusCode(StatusCodes.Status404NotFound, new RespuestaError
            {
                Estado = StatusCodes.Status404NotFound,
                Mensaje = "Registro no encontrado.",
                Detalle = ex.Message
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