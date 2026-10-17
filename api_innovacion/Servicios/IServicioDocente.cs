// =============================================================================
// QUÉ ES: contrato de las reglas de negocio del catálogo de docentes.
// PAPEL: mantiene las reglas separadas del controlador y del repositorio.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

public interface IServicioDocente
{
    Task<List<Docente>> ObtenerTodosAsync(int limite);
    Task<Docente> ObtenerPorIdAsync(int id);
    Task<Docente> CrearAsync(DocenteCrear peticion);
    Task<int> ReemplazarAsync(int id, DocenteReemplazo peticion);
    Task<int> ActualizarAsync(int id, DocenteActualizar peticion);
    Task<int> EliminarAsync(int id);
}
