// =============================================================================
// QUÉ ES: contrato de negocio de programa_pe.
// PAPEL: separa validaciones y reglas de la API HTTP y del repositorio.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

public interface IServicioProgramaPe
{
    Task<List<ProgramaPe>> ObtenerTodosAsync(int limite);
    Task<ProgramaPe> ObtenerPorParejaAsync(int programa, int practica_estrategia);
    Task<ProgramaPe> AsignarAsync(ProgramaPeCrear peticion);
    Task<int> RetirarAsync(int programa, int practica_estrategia);
}
