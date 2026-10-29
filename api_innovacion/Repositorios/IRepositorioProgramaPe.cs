// =============================================================================
// QUÉ ES: contrato de persistencia de programa_pe.
// PAPEL: permite probar el servicio sin SQL Server y aísla Dapper.
// =============================================================================
using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

public interface IRepositorioProgramaPe
{
    Task<List<ProgramaPe>> ObtenerTodosAsync(int limite);
    Task<ProgramaPe?> ObtenerPorParejaAsync(int programa, int practica_estrategia);
    Task<ProgramaPe> AsignarAsync(ProgramaPe entidad);
    Task<int> RetirarAsync(int programa, int practica_estrategia);
}
