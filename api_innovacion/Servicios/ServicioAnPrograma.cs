// =============================================================================
// QUÉ ES: servicio de la relación an_programa.
// PAPEL: aplica reglas de negocio y convierte conflictos SQL conocidos en 409.
// =============================================================================
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

public class ServicioAnPrograma : IServicioAnPrograma
{
    private readonly IRepositorioAnPrograma _repositorio;

    public ServicioAnPrograma(IRepositorioAnPrograma repositorio) => _repositorio = repositorio;

    public Task<List<AnPrograma>> ObtenerTodosAsync(int limite) => _repositorio.ObtenerTodosAsync(limite);

    public async Task<AnPrograma> ObtenerPorParejaAsync(int aspecto_normativo, int programa)
    {
        var fila = await _repositorio.ObtenerPorParejaAsync(aspecto_normativo, programa);
        return fila ?? throw new NoEncontradoExcepcion("No existe una relación activa con esas claves.");
    }

    public async Task<AnPrograma> AsignarAsync(AnProgramaCrear peticion)
    {
        var entidad = new AnPrograma
        {
            AspectoNormativo = peticion.AspectoNormativo!.Value,
            Programa = peticion.Programa!.Value,
        };

        try
        {
            return await _repositorio.AsignarAsync(entidad);
        }
        catch (SqlException ex) when (TraductorSqlRelacion.EsConflicto(ex))
        {
            throw TraductorSqlRelacion.Convertir(ex);
        }
    }

    public async Task<int> RetirarAsync(int aspecto_normativo, int programa)
    {
        var filas = await _repositorio.RetirarAsync(aspecto_normativo, programa);
        if (filas == 0)
            throw new NoEncontradoExcepcion("No existe una relación activa con esas claves.");
        return filas;
    }
}
