using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Reglas de negocio de `aliado`. Recibe la INTERFAZ del repositorio (la inyecta
/// el ensamblador), asi que esta clase no sabe que detras hay SQL Server.
/// Traduce "no existe" a NoEncontradoExcepcion (-> 404) y "PATCH sin campos" a
/// ArgumentException (-> 400) (3_plan.md, Secciones 4.2 y 4.6).
///
/// El mensaje de "no encontrado" nombra `nit` y no `id`, porque asi lo fija el
/// contrato para esta tabla: "No existe un registro activo con nit = X."
/// (6_contracts.md, Secciones 3.7, 6.7 y 7.7).
/// </summary>
public class ServicioAliado : IServicioAliado
{
    private readonly IRepositorioAliado _repositorio;

    public ServicioAliado(IRepositorioAliado repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<Aliado>> ObtenerTodosAsync(int limite)
    {
        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Aliado> ObtenerPorIdAsync(int nit)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(nit);
        if (registro is null)
            throw new NoEncontradoExcepcion($"No existe un registro activo con nit = {nit}.");

        return registro;
    }

    public async Task CrearAsync(AliadoCrear peticion)
    {
        await _repositorio.CrearAsync(new Aliado
        {
            Nit = peticion.Nit!.Value,
            RazonSocial = peticion.RazonSocial!,
            NombreContacto = peticion.NombreContacto!,
            Correo = peticion.Correo!,
            Telefono = peticion.Telefono!,
            Ciudad = peticion.Ciudad!
        });
    }

    public async Task<int> ReemplazarAsync(int nit, AliadoReemplazo peticion)
    {
        // El PUT envia todos los campos modificables: el diccionario va completo.
        // Las claves son los nombres de columna en snake_case.
        var datos = new Dictionary<string, object>
        {
            ["razon_social"] = peticion.RazonSocial!,
            ["nombre_contacto"] = peticion.NombreContacto!,
            ["correo"] = peticion.Correo!,
            ["telefono"] = peticion.Telefono!,
            ["ciudad"] = peticion.Ciudad!
        };

        var filas = await _repositorio.ActualizarAsync(nit, datos);
        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con nit = {nit}.");

        return filas;
    }

    public async Task<int> ActualizarAsync(int nit, AliadoActualizar peticion)
    {
        // El PATCH solo incluye en el diccionario los campos que llegaron.
        var datos = new Dictionary<string, object>();

        if (peticion.RazonSocial is not null) datos["razon_social"] = peticion.RazonSocial;
        if (peticion.NombreContacto is not null) datos["nombre_contacto"] = peticion.NombreContacto;
        if (peticion.Correo is not null) datos["correo"] = peticion.Correo;
        if (peticion.Telefono is not null) datos["telefono"] = peticion.Telefono;
        if (peticion.Ciudad is not null) datos["ciudad"] = peticion.Ciudad;

        // No es un error de forma (eso seria 422) sino de regla de negocio:
        // no hay nada que actualizar (3_plan.md, Seccion 4.2).
        if (datos.Count == 0)
            throw new ArgumentException("No se envió ningún campo para actualizar.");

        var filas = await _repositorio.ActualizarAsync(nit, datos);
        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con nit = {nit}.");

        return filas;
    }

    public async Task<int> EliminarAsync(int nit)
    {
        var filas = await _repositorio.EliminarAsync(nit);
        if (filas == 0)
            throw new NoEncontradoExcepcion($"No existe un registro activo con nit = {nit}.");

        return filas;
    }
}
