// =============================================================================
// QUÉ ES: contrato de negocio de programa_ac.
// PAPEL: separa validaciones y reglas de la API HTTP y del repositorio.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

public interface IServicioProgramaAc
{
    Task<List<ProgramaAc>> ObtenerTodosAsync(int limite);
    Task<ProgramaAc> ObtenerPorParejaAsync(int programa, int area_conocimiento);
    Task<ProgramaAc> AsignarAsync(ProgramaAcCrear peticion);
    Task<int> RetirarAsync(int programa, int area_conocimiento);
}
