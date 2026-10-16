// =============================================================================
// QUÉ ES: contrato de lectura de programas activos.
// PAPEL: mantiene la consulta del desplegable fuera del controlador y servicio.
// =============================================================================
using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

public interface IRepositorioProgramaOpciones
{
    Task<List<ProgramaOpcion>> ObtenerTodosAsync(int limite);
}
