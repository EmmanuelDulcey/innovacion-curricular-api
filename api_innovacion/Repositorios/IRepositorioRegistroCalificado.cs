using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Contrato de datos de `registro_calificado` (v2). Su PK se llama `codigo`.
/// `EliminarAsync` devuelve 0 cuando la tabla tiene puentes activas
/// (enfoque_rc o aa_rc): el servicio lo traduce a 409.
/// </summary>
public interface IRepositorioRegistroCalificado
{
    Task<List<RegistroCalificado>> ObtenerTodosAsync(int limite);
    Task<RegistroCalificado?> ObtenerPorIdAsync(int codigo);
    Task<RegistroCalificado> CrearAsync(RegistroCalificado entidad);
    Task<int> ActualizarAsync(RegistroCalificado entidad);
    Task<int> EliminarAsync(int codigo);
}
