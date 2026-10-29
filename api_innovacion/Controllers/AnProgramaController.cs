// =============================================================================
// QUÉ ES: controlador HTTP de an_programa.
// PAPEL: conserva los sobres de la v1 y expone la pareja sin opción de edición.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

[ApiController]
[Route("api/an_programa")]
public class AnProgramaController : ControllerBase
{
    private readonly IServicioAnPrograma _servicio;

    public AnProgramaController(IServicioAnPrograma servicio) => _servicio = servicio;

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
            return Ok(new RespuestaLista<AnPrograma>
            {
                Tabla = "an_programa", Limite = limite, Total = datos.Count, Datos = datos
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpGet("{aspecto_normativo:int}/{programa:int}")]
    public async Task<IActionResult> Obtener(int aspecto_normativo, int programa)
    {
        try { return Ok(await _servicio.ObtenerPorParejaAsync(aspecto_normativo, programa)); }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpPost]
    public async Task<IActionResult> Asignar([FromBody] AnProgramaCrear peticion)
    {
        try
        {
            var creado = await _servicio.AsignarAsync(peticion);
            return Ok(new RespuestaCreacion<AnPrograma>
            {
                Estado = 200, Mensaje = "Relación asignada exitosamente.", Datos = creado
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpDelete("{aspecto_normativo:int}/{programa:int}")]
    public async Task<IActionResult> Retirar(int aspecto_normativo, int programa)
    {
        try
        {
            return Ok(new RespuestaFilasEliminadas
            {
                Estado = 200, Mensaje = "Relación retirada exitosamente.",
                FilasEliminadas = await _servicio.RetirarAsync(aspecto_normativo, programa)
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }
}
