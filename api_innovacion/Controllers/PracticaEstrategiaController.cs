using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Controllers;

[Route("api/practica_estrategia")]
[ApiController]
public class PracticaEstrategiaController : ControllerBase
{
    private readonly IServicioPracticaEstrategia _servicio;

    public PracticaEstrategiaController(IServicioPracticaEstrategia servicio)
    {
        _servicio = servicio;
    }

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

            return Ok(new RespuestaLista<PracticaEstrategia>
            {
                Tabla = "practica_estrategia",
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        try { return Ok(await _servicio.ObtenerPorIdAsync(id)); }
        catch (Exception ex) { return TraducirError(ex, "Error del servidor."); }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] PracticaEstrategiaCrear peticion)
    {
        try
        {
            await _servicio.CrearAsync(peticion);
            return Ok(new RespuestaExito { Estado = 200, Mensaje = "Registro creado exitosamente." });
        }
        catch (Exception ex) { return TraducirError(ex, "Error al crear el registro."); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Reemplazar(int id, [FromBody] PracticaEstrategiaReemplazo peticion)
    {
        try
        {
            return Ok(new RespuestaFilasAfectadas
            {
                Estado = 200,
                Mensaje = "Registro reemplazado exitosamente.",
                FilasAfectadas = await _servicio.ReemplazarAsync(id, peticion)
            });
        }
        catch (Exception ex) { return TraducirError(ex, "Error al actualizar el registro."); }
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] PracticaEstrategiaActualizar peticion)
    {
        try
        {
            return Ok(new RespuestaFilasAfectadas
            {
                Estado = 200,
                Mensaje = "Registro actualizado exitosamente.",
                FilasAfectadas = await _servicio.ActualizarAsync(id, peticion)
            });
        }
        catch (Exception ex) { return TraducirError(ex, "Error al actualizar el registro."); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            return Ok(new RespuestaFilasEliminadas
            {
                Estado = 200,
                Mensaje = "Registro eliminado exitosamente.",
                FilasEliminadas = await _servicio.EliminarAsync(id)
            });
        }
        catch (Exception ex) { return TraducirError(ex, "Error al eliminar el registro."); }
    }

    private IActionResult TraducirError(Exception ex, string mensajeContexto)
    {
        if (ex is NoEncontradoExcepcion)
            return StatusCode(404, new RespuestaError { Estado = 404, Mensaje = "Registro no encontrado.", Detalle = ex.Message });

        if (ex is ArgumentException)
            return BadRequest(new RespuestaError { Estado = 400, Mensaje = "Parámetros inválidos.", Detalle = ex.Message });

        var detalle = ex is SqlException ? $"{ex.Message} (detalle de SQL Server)." : ex.Message;
        return StatusCode(500, new RespuestaError { Estado = 500, Mensaje = mensajeContexto, Detalle = detalle });
    }
}
