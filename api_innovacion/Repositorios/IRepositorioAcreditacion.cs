using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Contrato de datos de `acreditacion` (v2). Su PK se llama `resolucion`, asi
/// que los metodos la reciben con ese nombre (la ruta tambien la usa).
/// </summary>
public interface IRepositorioAcreditacion
{
    Task<List<Acreditacion>> ObtenerTodosAsync(int limite);
    Task<Acreditacion?> ObtenerPorIdAsync(int resolucion);
    Task<Acreditacion> CrearAsync(Acreditacion entidad);
    Task<int> ActualizarAsync(Acreditacion entidad);
    Task<int> EliminarAsync(int resolucion);
}
