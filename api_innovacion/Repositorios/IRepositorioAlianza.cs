// =============================================================================
// QUÉ ES: contrato de persistencia de alianza.
// PAPEL: permite probar el servicio sin SQL Server y aísla Dapper.
// =============================================================================
using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

public interface IRepositorioAlianza
{
    Task<List<Alianza>> ObtenerTodosAsync(int limite);
    Task<Alianza?> ObtenerPorParejaAsync(int aliado, int departamento);
    Task<Alianza> CrearAsync(Alianza entidad);
    Task<int> ActualizarAsync(Alianza entidad);
    Task<int> RetirarAsync(int aliado, int departamento);
}
