using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Contrato de acceso a datos de `car_innovacion`. El servicio depende de esta
/// interfaz, no de la clase concreta: por eso no sabe que detras hay SQL Server
/// (1_constitution.md, Articulo 3). Los 5 metodos son asincronos
/// (3_plan.md, Seccion 4.1).
/// </summary>
public interface IRepositorioCarInnovacion
{
    Task<List<CarInnovacion>> ObtenerTodosAsync(int limite);
    Task<CarInnovacion?> ObtenerPorIdAsync(int id);
    Task CrearAsync(CarInnovacion entidad);
    Task<int> ActualizarAsync(int id, Dictionary<string, object> datos); // PUT y PATCH
    Task<int> EliminarAsync(int id);                                     // UPDATE activo = 0
}
