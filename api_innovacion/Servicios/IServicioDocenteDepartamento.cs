// =============================================================================
// QUÉ ES: contrato de negocio de docente_departamento.
// PAPEL: separa validaciones y reglas de la API HTTP y del repositorio.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

public interface IServicioDocenteDepartamento
{
    Task<List<DocenteDepartamento>> ObtenerTodosAsync(int limite);
    Task<DocenteDepartamento> ObtenerPorParejaAsync(int docente, int departamento);
    Task<DocenteDepartamento> CrearAsync(DocenteDepartamentoCrear peticion);
    Task<int> ReemplazarAsync(int docente, int departamento, DocenteDepartamentoReemplazo peticion);
    Task<int> ActualizarAsync(int docente, int departamento, DocenteDepartamentoActualizar peticion);
    Task<int> RetirarAsync(int docente, int departamento);
}
