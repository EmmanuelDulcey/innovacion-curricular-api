using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Contrato de acceso a datos de `aliado`. El servicio depende de esta
/// interfaz, no de la clase concreta (1_constitution.md, Articulo 3).
///
/// La plantilla es la misma que la de las otras seis tablas
/// (3_plan.md, Seccion 4.1: "en `aliado` la llave primaria es `nit` en vez de
/// `id`; el resto de la plantilla es identico"): los nombres de los metodos se
/// conservan y lo unico que cambia es el nombre del parametro y de la columna
/// de la llave, que aqui es `nit`.
/// </summary>
public interface IRepositorioAliado
{
    Task<List<Aliado>> ObtenerTodosAsync(int limite);
    Task<Aliado?> ObtenerPorIdAsync(int nit);
    Task CrearAsync(Aliado entidad);
    Task<int> ActualizarAsync(int nit, Dictionary<string, object> datos); // PUT y PATCH
    Task<int> EliminarAsync(int nit);                                     // UPDATE activo = 0
}
