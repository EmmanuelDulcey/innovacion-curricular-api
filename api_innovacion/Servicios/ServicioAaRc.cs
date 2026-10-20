using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Servicio de la puente `aa_rc` (v2). Mismas reglas que enfoque_rc: pareja
/// repetida -> 409, FK inexistente o retirada -> 409 (con el mensaje de la FK
/// que fallo), retirar pareja inexistente -> 404. `componente` y `semestre`
/// viajan en la asignacion y no se editan despues.
/// </summary>
public class ServicioAaRc : IServicioAaRc
{
    private readonly IRepositorioAaRc _repositorio;

    public ServicioAaRc(IRepositorioAaRc repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<List<AaRc>> ObtenerTodosAsync(int limite)
    {
        return _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<AaRc> ObtenerPorIdAsync(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        var registro = await _repositorio.ObtenerPorIdAsync(activAcademicasIdcurso, registroCalificadoCodigo);
        if (registro is null)
            throw new NoEncontradoExcepcion(
                $"No existe un registro activo con activ_academicas_idcurso = {activAcademicasIdcurso} y registro_calificado_codigo = {registroCalificadoCodigo}.");

        return registro;
    }

    public async Task<AaRc> AsignarAsync(AaRcPareja peticion)
    {
        var entidad = new AaRc
        {
            ActivAcademicasIdcurso = peticion.ActivAcademicasIdcurso!.Value,
            RegistroCalificadoCodigo = peticion.RegistroCalificadoCodigo!.Value,
            Componente = peticion.Componente!,
            Semestre = peticion.Semestre!
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

    public async Task<int> RetirarAsync(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        var filas = await _repositorio.RetirarAsync(activAcademicasIdcurso, registroCalificadoCodigo);
        if (filas == 0)
            throw new NoEncontradoExcepcion(
                $"No existe un registro activo con activ_academicas_idcurso = {activAcademicasIdcurso} y registro_calificado_codigo = {registroCalificadoCodigo}.");

        return filas;
    }

    private static Exception TraducirSqlException(SqlException ex)
    {
        return ex.Number switch
        {
            50104 => new ConflictoExcepcion("La actividad académica indicada no existe o está inactiva."),
            50103 => new ConflictoExcepcion("El registro calificado indicado no existe o está inactivo."),
            2627 or 2601 => new ConflictoExcepcion("Esa pareja ya está registrada."),
            547 => new ConflictoExcepcion("Alguna de las dos referencias indicadas no existe o está inactiva."),
            _ => ex
        };
    }
}
