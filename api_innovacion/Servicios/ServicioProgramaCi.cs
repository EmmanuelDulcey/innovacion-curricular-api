// =============================================================================
// QUÉ ES: servicio de la relación programa_ci.
// PAPEL: aplica reglas de negocio y convierte conflictos SQL conocidos en 409.
// =============================================================================
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

public class ServicioProgramaCi : IServicioProgramaCi
{
    private readonly IRepositorioProgramaCi _repositorio;

    public ServicioProgramaCi(IRepositorioProgramaCi repositorio) => _repositorio = repositorio;

    public Task<List<ProgramaCi>> ObtenerTodosAsync(int limite) => _repositorio.ObtenerTodosAsync(limite);

    public async Task<ProgramaCi> ObtenerPorParejaAsync(int programa, int car_innovacion)
    {
        var fila = await _repositorio.ObtenerPorParejaAsync(programa, car_innovacion);
        return fila ?? throw new NoEncontradoExcepcion("No existe una relación activa con esas claves.");
    }

    public async Task<ProgramaCi> AsignarAsync(ProgramaCiCrear peticion)
    {
        var entidad = new ProgramaCi
        {
            Programa = peticion.Programa!.Value,
            CarInnovacion = peticion.CarInnovacion!.Value,
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

    public async Task<int> RetirarAsync(int programa, int car_innovacion)
    {
        var filas = await _repositorio.RetirarAsync(programa, car_innovacion);
        if (filas == 0)
            throw new NoEncontradoExcepcion("No existe una relación activa con esas claves.");
        return filas;
    }
}
