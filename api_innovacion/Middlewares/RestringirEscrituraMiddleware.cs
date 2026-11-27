using System.Security.Claims;
using ApiInnovacion.Respuestas;
using ApiInnovacion.Seguridad;

namespace ApiInnovacion.Middlewares;

/// <summary>
/// Impide que un usuario autenticado con rol de solo lectura (Docente,
/// Consulta o sin rol) modifique datos. Se ejecuta DESPUÉS de la autorización
/// JWT, así que solo ve peticiones ya autenticadas. Responde 403 con el mismo
/// sobre de error del resto de la API.
/// </summary>
public sealed class RestringirEscrituraMiddleware
{
    private readonly RequestDelegate _next;

    public RestringirEscrituraMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        if (contexto.User.Identity?.IsAuthenticated == true
            && !EsLectura(contexto.Request.Method)
            && !PoliticaRoles.PuedeEscribir(contexto.User.FindAll("role").Select(claim => claim.Value)))
        {
            contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
            contexto.Response.ContentType = "application/json; charset=utf-8";
            await contexto.Response.WriteAsJsonAsync(new RespuestaError
            {
                Estado = StatusCodes.Status403Forbidden,
                Mensaje = "No tiene permisos para modificar registros.",
                Detalle = "Su rol es de solo lectura.",
            });
            return;
        }

        await _next(contexto);
    }

    private static bool EsLectura(string metodo)
    {
        return HttpMethods.IsGet(metodo)
            || HttpMethods.IsHead(metodo)
            || HttpMethods.IsOptions(metodo);
    }
}
