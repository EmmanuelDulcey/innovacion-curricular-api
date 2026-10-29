// =============================================================================
// QUÉ ES: endpoints del catálogo local de docentes.
// PAPEL: traduce HTTP y excepciones; toda regla y persistencia vive en capas.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

[ApiController]
[Route("api/docente")]
public class DocenteController : ControllerBase
{
    private readonly IServicioDocente _servicio;

    public DocenteController(IServicioDocente servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        if (limite <= 0)
            return BadRequest(new RespuestaError { Estado = 400, Mensaje = "Parámetros inválidos.", Detalle = "El límite debe ser mayor que 0." });
        try
        {
            var datos = await _servicio.ObtenerTodosAsync(limite);
            if (datos.Count == 0) return NoContent();
            return Ok(new RespuestaLista<Docente> { Tabla = "docente", Limite = limite, Total = datos.Count, Datos = datos });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        try { return Ok(await _servicio.ObtenerPorIdAsync(id)); }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] DocenteCrear peticion)
    {
        try
        {
            return Ok(new RespuestaCreacion<Docente>
            {
                Estado = StatusCodes.Status200OK,
                Mensaje = "Docente creado exitosamente.",
                Datos = await _servicio.CrearAsync(peticion)
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Reemplazar(int id, [FromBody] DocenteReemplazo peticion)
    {
        try
        {
            return Ok(new RespuestaFilasAfectadas
            {
                Estado = StatusCodes.Status200OK,
                Mensaje = "Docente actualizado exitosamente.",
                FilasAfectadas = await _servicio.ReemplazarAsync(id, peticion)
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] DocenteActualizar peticion)
    {
        try
        {
            return Ok(new RespuestaFilasAfectadas
            {
                Estado = StatusCodes.Status200OK,
                Mensaje = "Docente actualizado exitosamente.",
                FilasAfectadas = await _servicio.ActualizarAsync(id, peticion)
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            return Ok(new RespuestaFilasEliminadas
            {
                Estado = StatusCodes.Status200OK,
                Mensaje = "Docente desactivado exitosamente.",
                FilasEliminadas = await _servicio.EliminarAsync(id)
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }
}
