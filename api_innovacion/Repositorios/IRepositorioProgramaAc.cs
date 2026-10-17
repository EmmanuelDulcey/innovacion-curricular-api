// =============================================================================
// QUÉ ES: contrato de persistencia de programa_ac.
// PAPEL: permite probar el servicio sin SQL Server y aísla Dapper.
// =============================================================================
using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

public interface IRepositorioProgramaAc
{
    Task<List<ProgramaAc>> ObtenerTodosAsync(int limite);
    Task<ProgramaAc?> ObtenerPorParejaAsync(int programa, int area_conocimiento);
    Task<ProgramaAc> AsignarAsync(ProgramaAc entidad);
    Task<int> RetirarAsync(int programa, int area_conocimiento);
}
