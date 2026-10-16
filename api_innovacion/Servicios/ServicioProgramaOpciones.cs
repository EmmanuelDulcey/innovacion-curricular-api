// =============================================================================
// QUÉ ES: servicio de lectura de programas activos.
// PAPEL: ofrece una fuente estable de opciones sin modificar el módulo programa.
// =============================================================================
using ApiInnovacion.Modelos;
using ApiInnovacion.Repositorios;

namespace ApiInnovacion.Servicios;

public class ServicioProgramaOpciones : IServicioProgramaOpciones
{
    private readonly IRepositorioProgramaOpciones _repositorio;

    public ServicioProgramaOpciones(IRepositorioProgramaOpciones repositorio) => _repositorio = repositorio;

    public Task<List<ProgramaOpcion>> ObtenerTodosAsync(int limite) => _repositorio.ObtenerTodosAsync(limite);
}
