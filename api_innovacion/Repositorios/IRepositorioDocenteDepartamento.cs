// =============================================================================
// QUÉ ES: contrato de persistencia de docente_departamento.
// PAPEL: permite probar el servicio sin SQL Server y aísla Dapper.
// =============================================================================
using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

public interface IRepositorioDocenteDepartamento
{
    Task<List<DocenteDepartamento>> ObtenerTodosAsync(int limite);
    Task<DocenteDepartamento?> ObtenerPorParejaAsync(int docente, int departamento);
    Task<DocenteDepartamento> CrearAsync(DocenteDepartamento entidad);
    Task<int> ActualizarAsync(DocenteDepartamento entidad);
    Task<int> RetirarAsync(int docente, int departamento);
}
