// =============================================================================
// QUÉ ES: contrato de negocio de an_programa.
// PAPEL: separa validaciones y reglas de la API HTTP y del repositorio.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

public interface IServicioAnPrograma
{
    Task<List<AnPrograma>> ObtenerTodosAsync(int limite);
    Task<AnPrograma> ObtenerPorParejaAsync(int aspecto_normativo, int programa);
    Task<AnPrograma> AsignarAsync(AnProgramaCrear peticion);
    Task<int> RetirarAsync(int aspecto_normativo, int programa);
}
