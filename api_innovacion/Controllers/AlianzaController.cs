// =============================================================================
// QUÉ ES: controlador HTTP de alianza.
// PAPEL: conserva los sobres de la v1 y permite mantener los datos de una alianza
//       sin cambiar la pareja aliado-programa que la identifica.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

[ApiController]
[Route("api/alianza")]
public class AlianzaController : ControllerBase
{
    private readonly IServicioAlianza _servicio;

    public AlianzaController(IServicioAlianza servicio) => _servicio = servicio;

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
            return Ok(new RespuestaLista<Alianza>
            {
                Tabla = "alianza", Limite = limite, Total = datos.Count, Datos = datos
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpGet("{aliado:int}/{departamento:int}")]
    public async Task<IActionResult> Obtener(int aliado, int departamento)
    {
        try { return Ok(await _servicio.ObtenerPorParejaAsync(aliado, departamento)); }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] AlianzaCrear peticion)
    {
        try
        {
            var creado = await _servicio.CrearAsync(peticion);
            return Ok(new RespuestaCreacion<Alianza>
            {
                Estado = 200, Mensaje = "Registro creado exitosamente.", Datos = creado
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    // La pareja compuesta vive en la ruta para que PUT reemplace solo los datos
    // propios de la alianza y no permita trasladar la relación a otra pareja.
    [HttpPut("{aliado:int}/{departamento:int}")]
    public async Task<IActionResult> Reemplazar(int aliado, int departamento, [FromBody] AlianzaReemplazo peticion)
    {
        try
        {
            return Ok(new RespuestaFilasAfectadas
            {
                Estado = 200,
                Mensaje = "Registro reemplazado exitosamente.",
                FilasAfectadas = await _servicio.ReemplazarAsync(aliado, departamento, peticion)
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    // PATCH mantiene cualquier atributo no enviado y permite limpiar solo los
    // campos opcionales fecha_fin y docente.
    [HttpPatch("{aliado:int}/{departamento:int}")]
    public async Task<IActionResult> Actualizar(int aliado, int departamento, [FromBody] AlianzaActualizar peticion)
    {
        try
        {
            return Ok(new RespuestaFilasAfectadas
            {
                Estado = 200,
                Mensaje = "Registro actualizado exitosamente.",
                FilasAfectadas = await _servicio.ActualizarAsync(aliado, departamento, peticion)
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpDelete("{aliado:int}/{departamento:int}")]
    public async Task<IActionResult> Retirar(int aliado, int departamento)
    {
        try
        {
            return Ok(new RespuestaFilasEliminadas
            {
                Estado = 200, Mensaje = "Relación retirada exitosamente.",
                FilasEliminadas = await _servicio.RetirarAsync(aliado, departamento)
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }
}
