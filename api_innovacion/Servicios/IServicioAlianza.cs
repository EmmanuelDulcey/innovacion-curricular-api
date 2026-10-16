// =============================================================================
// QUÉ ES: contrato de negocio de alianza.
// PAPEL: separa validaciones y reglas de la API HTTP y del repositorio.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

public interface IServicioAlianza
{
    Task<List<Alianza>> ObtenerTodosAsync(int limite);
    Task<Alianza> ObtenerPorParejaAsync(int aliado, int departamento);
    Task<Alianza> CrearAsync(AlianzaCrear peticion);
    Task<int> ReemplazarAsync(int aliado, int departamento, AlianzaReemplazo peticion);
    Task<int> ActualizarAsync(int aliado, int departamento, AlianzaActualizar peticion);
    Task<int> RetirarAsync(int aliado, int departamento);
}
