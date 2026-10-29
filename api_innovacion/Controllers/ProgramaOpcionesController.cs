// =============================================================================
// QUÉ ES: ruta de solo lectura para los programas activos.
// PAPEL: alimenta desplegables de v2 sin ocupar las rutas CRUD de programa.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

[ApiController]
[Route("api/opciones/programa")]
public class ProgramaOpcionesController : ControllerBase
{
    private readonly IServicioProgramaOpciones _servicio;

    public ProgramaOpcionesController(IServicioProgramaOpciones servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        if (limite <= 0)
            return BadRequest(new RespuestaError
            {
                Estado = 400, Mensaje = "Parámetros inválidos.",
                Detalle = "El parámetro limite debe ser mayor que 0."
            });

        try
        {
            var datos = await _servicio.ObtenerTodosAsync(limite);
            if (datos.Count == 0) return NoContent();
            return Ok(new RespuestaLista<ProgramaOpcion>
            {
                Tabla = "programa", Limite = limite, Total = datos.Count, Datos = datos
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }
}
