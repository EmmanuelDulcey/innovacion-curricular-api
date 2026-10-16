// =============================================================================
// QUÉ ES: contrato de negocio de programa_ci.
// PAPEL: separa validaciones y reglas de la API HTTP y del repositorio.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

public interface IServicioProgramaCi
{
    Task<List<ProgramaCi>> ObtenerTodosAsync(int limite);
    Task<ProgramaCi> ObtenerPorParejaAsync(int programa, int car_innovacion);
    Task<ProgramaCi> AsignarAsync(ProgramaCiCrear peticion);
    Task<int> RetirarAsync(int programa, int car_innovacion);
}
