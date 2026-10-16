// =============================================================================
// QUÉ ES: servicio de la relación programa_pe.
// PAPEL: aplica reglas de negocio y convierte conflictos SQL conocidos en 409.
// =============================================================================
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

public class ServicioProgramaPe : IServicioProgramaPe
{
    private readonly IRepositorioProgramaPe _repositorio;

    public ServicioProgramaPe(IRepositorioProgramaPe repositorio) => _repositorio = repositorio;

    public Task<List<ProgramaPe>> ObtenerTodosAsync(int limite) => _repositorio.ObtenerTodosAsync(limite);

    public async Task<ProgramaPe> ObtenerPorParejaAsync(int programa, int practica_estrategia)
    {
        var fila = await _repositorio.ObtenerPorParejaAsync(programa, practica_estrategia);
        return fila ?? throw new NoEncontradoExcepcion("No existe una relación activa con esas claves.");
    }

    public async Task<ProgramaPe> AsignarAsync(ProgramaPeCrear peticion)
    {
        var entidad = new ProgramaPe
        {
            Programa = peticion.Programa!.Value,
            PracticaEstrategia = peticion.PracticaEstrategia!.Value,
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

    public async Task<int> RetirarAsync(int programa, int practica_estrategia)
    {
        var filas = await _repositorio.RetirarAsync(programa, practica_estrategia);
        if (filas == 0)
            throw new NoEncontradoExcepcion("No existe una relación activa con esas claves.");
        return filas;
    }
}
