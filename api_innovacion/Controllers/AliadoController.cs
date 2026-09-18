using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Controllers;

/// <summary>
/// Capa HTTP de `aliado`. No toca SQL: delega todo en el servicio y se limita a
/// traducir el resultado a codigos HTTP (1_constitution.md, Articulo 3).
///
/// Particularidad: la llave primaria de esta tabla es `nit`, asi que las rutas
/// son /api/aliado/{nit} y no /api/aliado/{id} (6_contracts.md, Seccion 0.2).
/// La restriccion {nit:int} hace que una ruta no entera no llegue al metodo.
///
/// Codigos (3_plan.md, Seccion 4.6 y 6_contracts.md, Seccion 12):
///   200 exito, 204 lista vacia, 400 regla de negocio, 404 no existe,
///   422 body invalido (lo produce ASP.NET antes de entrar aqui), 500 error del motor.
/// </summary>
[Route("api/aliado")]
[ApiController]
public class AliadoController : ControllerBase
{
    private readonly IServicioAliado _servicio;

    public AliadoController(IServicioAliado servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        // Regla de negocio: el limite debe ser mayor que cero (6_contracts.md, Seccion 2).
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

            // Tabla sin registros activos -> 204 sin cuerpo (6_contracts.md, Seccion 2).
            if (datos.Count == 0)
                return NoContent();

            return Ok(new RespuestaLista<Aliado>
            {
                Tabla = "aliado",
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

    [HttpGet("{nit:int}")]
    public async Task<IActionResult> Obtener(int nit)
    {
        try { return Ok(await _servicio.ObtenerPorIdAsync(nit)); }
        catch (Exception ex) { return TraducirError(ex, "Error del servidor."); }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] AliadoCrear peticion)
    {
        try
        {
            await _servicio.CrearAsync(peticion);
            return Ok(new RespuestaExito { Estado = 200, Mensaje = "Registro creado exitosamente." });
        }
        // Una llave primaria duplicada la rechaza SQL Server y sale como 500 con
        // el detalle del motor (6_contracts.md, Seccion 4.7).
        catch (Exception ex) { return TraducirError(ex, "Error al crear el registro."); }
    }

    [HttpPut("{nit:int}")]
    public async Task<IActionResult> Reemplazar(int nit, [FromBody] AliadoReemplazo peticion)
    {
        try
        {
            return Ok(new RespuestaFilasAfectadas
            {
                Estado = 200,
                Mensaje = "Registro reemplazado exitosamente.",
                FilasAfectadas = await _servicio.ReemplazarAsync(nit, peticion)
            });
        }
        catch (Exception ex) { return TraducirError(ex, "Error al actualizar el registro."); }
    }

    [HttpPatch("{nit:int}")]
    public async Task<IActionResult> Actualizar(int nit, [FromBody] AliadoActualizar peticion)
    {
        try
        {
            return Ok(new RespuestaFilasAfectadas
            {
                Estado = 200,
                Mensaje = "Registro actualizado exitosamente.",
                FilasAfectadas = await _servicio.ActualizarAsync(nit, peticion)
            });
        }
        catch (Exception ex) { return TraducirError(ex, "Error al actualizar el registro."); }
    }

    [HttpDelete("{nit:int}")]
    public async Task<IActionResult> Eliminar(int nit)
    {
        try
        {
            return Ok(new RespuestaFilasEliminadas
            {
                Estado = 200,
                Mensaje = "Registro eliminado exitosamente.",
                FilasEliminadas = await _servicio.EliminarAsync(nit)
            });
        }
        catch (Exception ex) { return TraducirError(ex, "Error al eliminar el registro."); }
    }

    /// <summary>
    /// Traduce la excepcion de negocio (o del motor) al codigo HTTP que fija la
    /// constitucion, Articulo 3: NoEncontrado -> 404, ArgumentException -> 400,
    /// SqlException (y cualquier otra) -> 500 con el detalle del motor.
    /// </summary>
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
