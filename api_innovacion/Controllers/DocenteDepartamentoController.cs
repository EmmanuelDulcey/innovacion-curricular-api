// =============================================================================
// QUÉ ES: controlador HTTP de docente_departamento.
// PAPEL: conserva los sobres de la v1 y mantiene los atributos de la vinculación
//       sin cambiar la pareja docente-programa que la identifica.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiInnovacion.Controllers;

[ApiController]
[Route("api/docente_departamento")]
public class DocenteDepartamentoController : ControllerBase
{
    private readonly IServicioDocenteDepartamento _servicio;

    public DocenteDepartamentoController(IServicioDocenteDepartamento servicio) => _servicio = servicio;

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
            return Ok(new RespuestaLista<DocenteDepartamento>
            {
                Tabla = "docente_departamento", Limite = limite, Total = datos.Count, Datos = datos
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpGet("{docente:int}/{departamento:int}")]
    public async Task<IActionResult> Obtener(int docente, int departamento)
    {
        try { return Ok(await _servicio.ObtenerPorParejaAsync(docente, departamento)); }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] DocenteDepartamentoCrear peticion)
    {
        try
        {
            var creado = await _servicio.CrearAsync(peticion);
            return Ok(new RespuestaCreacion<DocenteDepartamento>
            {
                Estado = 200, Mensaje = "Registro creado exitosamente.", Datos = creado
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    // PUT reemplaza todos los atributos editables y conserva inmutables las
    // claves docente-programa, que se reciben en la ruta.
    [HttpPut("{docente:int}/{departamento:int}")]
    public async Task<IActionResult> Reemplazar(int docente, int departamento, [FromBody] DocenteDepartamentoReemplazo peticion)
    {
        try
        {
            return Ok(new RespuestaFilasAfectadas
            {
                Estado = 200,
                Mensaje = "Registro reemplazado exitosamente.",
                FilasAfectadas = await _servicio.ReemplazarAsync(docente, departamento, peticion)
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    // PATCH modifica únicamente los atributos recibidos; el servicio valida el
    // estado final antes de persistirlo.
    [HttpPatch("{docente:int}/{departamento:int}")]
    public async Task<IActionResult> Actualizar(int docente, int departamento, [FromBody] DocenteDepartamentoActualizar peticion)
    {
        try
        {
            return Ok(new RespuestaFilasAfectadas
            {
                Estado = 200,
                Mensaje = "Registro actualizado exitosamente.",
                FilasAfectadas = await _servicio.ActualizarAsync(docente, departamento, peticion)
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }

    [HttpDelete("{docente:int}/{departamento:int}")]
    public async Task<IActionResult> Retirar(int docente, int departamento)
    {
        try
        {
            return Ok(new RespuestaFilasEliminadas
            {
                Estado = 200, Mensaje = "Relación retirada exitosamente.",
                FilasEliminadas = await _servicio.RetirarAsync(docente, departamento)
            });
        }
        catch (Exception ex) { return TraductorErrorRelacion.Convertir(ex); }
    }
}
