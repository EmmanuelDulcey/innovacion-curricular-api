// =============================================================================
// QUÉ ES: contrato de persistencia de an_programa.
// PAPEL: permite probar el servicio sin SQL Server y aísla Dapper.
// =============================================================================
using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

public interface IRepositorioAnPrograma
{
    Task<List<AnPrograma>> ObtenerTodosAsync(int limite);
    Task<AnPrograma?> ObtenerPorParejaAsync(int aspecto_normativo, int programa);
    Task<AnPrograma> AsignarAsync(AnPrograma entidad);
    Task<int> RetirarAsync(int aspecto_normativo, int programa);
}
