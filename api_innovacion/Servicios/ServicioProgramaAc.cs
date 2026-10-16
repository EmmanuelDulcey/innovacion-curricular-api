// =============================================================================
// QUÉ ES: servicio de la relación programa_ac.
// PAPEL: aplica reglas de negocio y convierte conflictos SQL conocidos en 409.
// =============================================================================
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

public class ServicioProgramaAc : IServicioProgramaAc
{
    private readonly IRepositorioProgramaAc _repositorio;

    public ServicioProgramaAc(IRepositorioProgramaAc repositorio) => _repositorio = repositorio;

    public Task<List<ProgramaAc>> ObtenerTodosAsync(int limite) => _repositorio.ObtenerTodosAsync(limite);

    public async Task<ProgramaAc> ObtenerPorParejaAsync(int programa, int area_conocimiento)
    {
        var fila = await _repositorio.ObtenerPorParejaAsync(programa, area_conocimiento);
        return fila ?? throw new NoEncontradoExcepcion("No existe una relación activa con esas claves.");
    }

    public async Task<ProgramaAc> AsignarAsync(ProgramaAcCrear peticion)
    {
        var entidad = new ProgramaAc
        {
            Programa = peticion.Programa!.Value,
            AreaConocimiento = peticion.AreaConocimiento!.Value,
        };

        try
        {
            return await _repositorio.AsignarAsync(entidad);
        }
        catch (SqlException ex) when (TraductorSqlRelacion.EsConflicto(ex))
        {
            throw TraductorSqlRelacion.Convertir(ex);
        }
    }

    public async Task<int> RetirarAsync(int programa, int area_conocimiento)
    {
        var filas = await _repositorio.RetirarAsync(programa, area_conocimiento);
        if (filas == 0)
            throw new NoEncontradoExcepcion("No existe una relación activa con esas claves.");
        return filas;
    }
}
