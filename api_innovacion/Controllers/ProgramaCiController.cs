// =============================================================================
// QUÉ ES: controlador HTTP de programa_ci.
// PAPEL: conserva los sobres de la v1 y expone la pareja sin opción de edición.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

[ApiController]
[Route("api/programa_ci")]
public class ProgramaCiController : ControllerBase
{
    private readonly IServicioProgramaCi _servicio;

    public ProgramaCiController(IServicioProgramaCi servicio) => _servicio = servicio;

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
            return Ok(new RespuestaLista<ProgramaCi>
            {
                Tabla = "programa_ci", Limite = limite, Total = datos.Count, Datos = datos
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpGet("{programa:int}/{car_innovacion:int}")]
    public async Task<IActionResult> Obtener(int programa, int car_innovacion)
    {
        try { return Ok(await _servicio.ObtenerPorParejaAsync(programa, car_innovacion)); }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpPost]
    public async Task<IActionResult> Asignar([FromBody] ProgramaCiCrear peticion)
    {
        try
        {
            var creado = await _servicio.AsignarAsync(peticion);
            return Ok(new RespuestaCreacion<ProgramaCi>
            {
                Estado = 200, Mensaje = "Relación asignada exitosamente.", Datos = creado
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpDelete("{programa:int}/{car_innovacion:int}")]
    public async Task<IActionResult> Retirar(int programa, int car_innovacion)
    {
        try
        {
            return Ok(new RespuestaFilasEliminadas
            {
                Estado = 200, Mensaje = "Relación retirada exitosamente.",
                FilasEliminadas = await _servicio.RetirarAsync(programa, car_innovacion)
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }
}
