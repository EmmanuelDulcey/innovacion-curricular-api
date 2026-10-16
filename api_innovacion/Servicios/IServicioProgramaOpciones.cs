// =============================================================================
// QUÉ ES: contrato de aplicación para opciones de programa.
// PAPEL: separa la ruta HTTP del acceso a datos.
// =============================================================================
using ApiInnovacion.Modelos;

namespace ApiInnovacion.Servicios;

public interface IServicioProgramaOpciones
{
    Task<List<ProgramaOpcion>> ObtenerTodosAsync(int limite);
}
