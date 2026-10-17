using ApiInnovacion.Modelos;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Contrato de datos de `activ_academica` (v2). La entidad lleva `Disenio`
/// como `int?`: la FK opcional viaja null cuando no se eligio programa.
/// `EliminarAsync` devuelve 0 cuando hay parejas activas en aa_rc.
/// </summary>
public interface IRepositorioActivAcademica
{
    Task<List<ActivAcademica>> ObtenerTodosAsync(int limite);
    Task<ActivAcademica?> ObtenerPorIdAsync(int id);
    Task<ActivAcademica> CrearAsync(ActivAcademica entidad);
    Task<int> ActualizarAsync(ActivAcademica entidad);
    Task<int> EliminarAsync(int id);
}
