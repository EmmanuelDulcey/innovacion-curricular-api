using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Contrato del servicio de negocio de `activ_academica` (v2). Maneja la unica
/// FK opcional (`disenio`): cuando no viene, la entidad la lleva en null y el
/// procedimiento guarda NULL (nunca cadena vacia, tropiezo 4).
/// </summary>
public interface IServicioActivAcademica
{
    Task<List<ActivAcademica>> ObtenerTodosAsync(int limite);
    Task<ActivAcademica> ObtenerPorIdAsync(int id);
    Task<ActivAcademica> CrearAsync(ActivAcademicaCrear peticion);
    Task<int> ReemplazarAsync(int id, ActivAcademicaReemplazo peticion);
    Task<int> ActualizarAsync(int id, ActivAcademicaActualizar peticion);
    Task<int> EliminarAsync(int id);
}
