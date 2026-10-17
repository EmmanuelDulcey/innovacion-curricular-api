using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Servicio de la puente `enfoque_rc` (v2). Tres traducciones propias de una
/// puente (contrato §8.1 y §10):
///   * una FK inexistente o retirada -> 409 con el mensaje de ESA fk (los dos
///     THROW del procedimiento llevan numeros distintos justo para eso);
///   * una pareja repetida -> 409 "Esa pareja ya está registrada." (el
///     contrato literal; aplica tambien si la pareja existe retirada, porque
///     la PK compuesta no distingue activo);
///   * retirar una pareja que no existe o ya esta retirada -> 404.
/// </summary>
public class ServicioEnfoqueRc : IServicioEnfoqueRc
{
    private readonly IRepositorioEnfoqueRc _repositorio;

    public ServicioEnfoqueRc(IRepositorioEnfoqueRc repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<List<EnfoqueRc>> ObtenerTodosAsync(int limite)
    {
        return _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<EnfoqueRc> ObtenerPorIdAsync(int enfoque, int registroCalificado)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(enfoque, registroCalificado);
        if (registro is null)
            throw new NoEncontradoExcepcion(
                $"No existe un registro activo con enfoque = {enfoque} y registro_calificado = {registroCalificado}.");

        return registro;
    }

    public async Task<EnfoqueRc> AsignarAsync(EnfoqueRcPareja peticion)
    {
        var entidad = new EnfoqueRc
        {
            Enfoque = peticion.Enfoque!.Value,
            RegistroCalificado = peticion.RegistroCalificado!.Value
        };

        try
        {
            return await _repositorio.AsignarAsync(entidad);
        }
        catch (SqlException ex)
        {
            throw TraducirSqlException(ex);
        }
    }

    public async Task<int> RetirarAsync(int enfoque, int registroCalificado)
    {
        // Retirar pareja inexistente (o ya retirada) -> 404; el procedimiento
        // devuelve @@ROWCOUNT y no hay otra causa posible de 0 filas.
        var filas = await _repositorio.RetirarAsync(enfoque, registroCalificado);
        if (filas == 0)
            throw new NoEncontradoExcepcion(
                $"No existe un registro activo con enfoque = {enfoque} y registro_calificado = {registroCalificado}.");

        return filas;
    }

    private static Exception TraducirSqlException(SqlException ex)
    {
        return ex.Number switch
        {
            50105 => new ConflictoExcepcion("El enfoque indicado no existe o está inactivo."),
            50103 => new ConflictoExcepcion("El registro calificado indicado no existe o está inactivo."),
            // La PK compuesta rechaza la pareja repetida: exista activa o retirada.
            2627 or 2601 => new ConflictoExcepcion("Esa pareja ya está registrada."),
            // Respaldo: violacion real de una FK (carrera entre la validacion y el INSERT).
            547 => new ConflictoExcepcion("Alguna de las dos referencias indicadas no existe o está inactiva."),
            _ => ex
        };
    }
}
