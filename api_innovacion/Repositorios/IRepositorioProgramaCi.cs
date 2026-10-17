// =============================================================================
// QUÉ ES: contrato de persistencia de programa_ci.
// PAPEL: permite probar el servicio sin SQL Server y aísla Dapper.
// =============================================================================
using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

public interface IRepositorioProgramaCi
{
    Task<List<ProgramaCi>> ObtenerTodosAsync(int limite);
    Task<ProgramaCi?> ObtenerPorParejaAsync(int programa, int car_innovacion);
    Task<ProgramaCi> AsignarAsync(ProgramaCi entidad);
    Task<int> RetirarAsync(int programa, int car_innovacion);
}
