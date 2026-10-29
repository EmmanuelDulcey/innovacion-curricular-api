// =============================================================================
// QUÉ ES: servicio de la relación alianza.
// PAPEL: aplica reglas de negocio y convierte conflictos SQL conocidos en 409.
// =============================================================================
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

public class ServicioAlianza : IServicioAlianza
{
    private readonly IRepositorioAlianza _repositorio;

    public ServicioAlianza(IRepositorioAlianza repositorio) => _repositorio = repositorio;

    public Task<List<Alianza>> ObtenerTodosAsync(int limite) => _repositorio.ObtenerTodosAsync(limite);

    public async Task<Alianza> ObtenerPorParejaAsync(int aliado, int departamento)
    {
        var fila = await _repositorio.ObtenerPorParejaAsync(aliado, departamento);
        return fila ?? throw new NoEncontradoExcepcion("No existe una relación activa con esas claves.");
    }

    public async Task<Alianza> CrearAsync(AlianzaCrear peticion)
    {
        // La fecha final es opcional, pero nunca puede preceder a la inicial.
        ValidarFechas(peticion.FechaInicio, peticion.FechaFin);

        var entidad = new Alianza
        {
            Aliado = peticion.Aliado!.Value,
            Departamento = peticion.Departamento!.Value,
            FechaInicio = peticion.FechaInicio!.Value,
            FechaFin = peticion.FechaFin,
            Docente = peticion.Docente,
        };

        try
        {
            return await _repositorio.CrearAsync(entidad);
        }
        catch (SqlException ex) when (TraductorSqlRelacion.EsConflicto(ex))
        {
            throw TraductorSqlRelacion.Convertir(ex);
        }
    }

    public async Task<int> ReemplazarAsync(int aliado, int departamento, AlianzaReemplazo peticion)
    {
        if (peticion.FechaInicio is null)
            throw new ArgumentException("La fecha inicial es obligatoria.");

        var entidad = new Alianza
        {
            Aliado = aliado,
            Departamento = departamento,
            FechaInicio = peticion.FechaInicio.Value,
            FechaFin = peticion.FechaFin,
            Docente = peticion.Docente
        };
        ValidarFechas(entidad.FechaInicio, entidad.FechaFin);

        return await ActualizarPersistenciaAsync(entidad);
    }

    public async Task<int> ActualizarAsync(int aliado, int departamento, AlianzaActualizar peticion)
    {
        if (!peticion.TieneCamposEnviados)
            throw new ArgumentException("No se envió ningún campo para actualizar.");

        var actual = await ObtenerPorParejaAsync(aliado, departamento);
        var entidad = new Alianza
        {
            Aliado = actual.Aliado,
            Departamento = actual.Departamento,
            FechaInicio = peticion.FechaInicioEnviada ? peticion.FechaInicio!.Value : actual.FechaInicio,
            FechaFin = peticion.FechaFinEnviada ? peticion.FechaFin : actual.FechaFin,
            Docente = peticion.DocenteEnviado ? peticion.Docente : actual.Docente
        };
        ValidarFechas(entidad.FechaInicio, entidad.FechaFin);

        return await ActualizarPersistenciaAsync(entidad);
    }

    public async Task<int> RetirarAsync(int aliado, int departamento)
    {
        var filas = await _repositorio.RetirarAsync(aliado, departamento);
        if (filas == 0)
            throw new NoEncontradoExcepcion("No existe una relación activa con esas claves.");
        return filas;
    }

    // El PATCH compone la fila completa antes del procedimiento, que conserva
    // parámetros fijos y deja las reglas de negocio fuera del repositorio.
    private async Task<int> ActualizarPersistenciaAsync(Alianza entidad)
    {
        try
        {
            var filas = await _repositorio.ActualizarAsync(entidad);
            if (filas == 0)
                throw new NoEncontradoExcepcion("No existe una relación activa con esas claves.");
            return filas;
        }
        catch (SqlException ex) when (TraductorSqlRelacion.EsConflicto(ex))
        {
            throw TraductorSqlRelacion.Convertir(ex);
        }
    }

    private static void ValidarFechas(DateTime? fechaInicio, DateTime? fechaFin)
    {
        if (fechaInicio is null)
            throw new ArgumentException("La fecha inicial es obligatoria.");
        if (fechaFin is not null && fechaFin < fechaInicio)
            throw new ArgumentException("La fecha final no puede ser anterior a la fecha inicial.");
    }
}
