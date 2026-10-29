// =============================================================================
// QUÉ ES: controlador HTTP de programa_pe.
// PAPEL: conserva los sobres de la v1 y expone la pareja sin opción de edición.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

[ApiController]
[Route("api/programa_pe")]
public class ProgramaPeController : ControllerBase
{
    private readonly IServicioProgramaPe _servicio;

    public ProgramaPeController(IServicioProgramaPe servicio) => _servicio = servicio;

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
            return Ok(new RespuestaLista<ProgramaPe>
            {
                Tabla = "programa_pe", Limite = limite, Total = datos.Count, Datos = datos
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpGet("{programa:int}/{practica_estrategia:int}")]
    public async Task<IActionResult> Obtener(int programa, int practica_estrategia)
    {
        try { return Ok(await _servicio.ObtenerPorParejaAsync(programa, practica_estrategia)); }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpPost]
    public async Task<IActionResult> Asignar([FromBody] ProgramaPeCrear peticion)
    {
        try
        {
            var creado = await _servicio.AsignarAsync(peticion);
            return Ok(new RespuestaCreacion<ProgramaPe>
            {
                Estado = 200, Mensaje = "Relación asignada exitosamente.", Datos = creado
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpDelete("{programa:int}/{practica_estrategia:int}")]
    public async Task<IActionResult> Retirar(int programa, int practica_estrategia)
    {
        try
        {
            return Ok(new RespuestaFilasEliminadas
            {
                Estado = 200, Mensaje = "Relación retirada exitosamente.",
                FilasEliminadas = await _servicio.RetirarAsync(programa, practica_estrategia)
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }
}
