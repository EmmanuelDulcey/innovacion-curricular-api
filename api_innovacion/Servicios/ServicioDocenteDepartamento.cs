// =============================================================================
// QUÉ ES: servicio de la relación docente_departamento.
// PAPEL: aplica reglas de negocio y convierte conflictos SQL conocidos en 409.
// =============================================================================
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

public class ServicioDocenteDepartamento : IServicioDocenteDepartamento
{
    private readonly IRepositorioDocenteDepartamento _repositorio;

    public ServicioDocenteDepartamento(IRepositorioDocenteDepartamento repositorio) => _repositorio = repositorio;

    public Task<List<DocenteDepartamento>> ObtenerTodosAsync(int limite) => _repositorio.ObtenerTodosAsync(limite);

    public async Task<DocenteDepartamento> ObtenerPorParejaAsync(int docente, int departamento)
    {
        var fila = await _repositorio.ObtenerPorParejaAsync(docente, departamento);
        return fila ?? throw new NoEncontradoExcepcion("No existe una relación activa con esas claves.");
    }

    public async Task<DocenteDepartamento> CrearAsync(DocenteDepartamentoCrear peticion)
    {
        // La fecha final es opcional, pero nunca puede preceder a la inicial.
        ValidarFechas(peticion.FechaIngreso, peticion.FechaSalida);

        var entidad = new DocenteDepartamento
        {
            Docente = peticion.Docente!.Value,
            Departamento = peticion.Departamento!.Value,
            Dedicacion = peticion.Dedicacion!,
            Modalidad = peticion.Modalidad!,
            FechaIngreso = peticion.FechaIngreso!.Value,
            FechaSalida = peticion.FechaSalida,
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

    public async Task<int> ReemplazarAsync(int docente, int departamento, DocenteDepartamentoReemplazo peticion)
    {
        if (string.IsNullOrWhiteSpace(peticion.Dedicacion) || string.IsNullOrWhiteSpace(peticion.Modalidad) || peticion.FechaIngreso is null)
            throw new ArgumentException("Dedicación, modalidad y fecha de ingreso son obligatorias.");

        var entidad = new DocenteDepartamento
        {
            Docente = docente,
            Departamento = departamento,
            Dedicacion = peticion.Dedicacion,
            Modalidad = peticion.Modalidad,
            FechaIngreso = peticion.FechaIngreso.Value,
            FechaSalida = peticion.FechaSalida
        };
        ValidarFechas(entidad.FechaIngreso, entidad.FechaSalida);

        return await ActualizarPersistenciaAsync(entidad);
    }

    public async Task<int> ActualizarAsync(int docente, int departamento, DocenteDepartamentoActualizar peticion)
    {
        if (!peticion.TieneCamposEnviados)
            throw new ArgumentException("No se envió ningún campo para actualizar.");

        var actual = await ObtenerPorParejaAsync(docente, departamento);
        var entidad = new DocenteDepartamento
        {
            Docente = actual.Docente,
            Departamento = actual.Departamento,
            Dedicacion = peticion.DedicacionEnviada ? peticion.Dedicacion! : actual.Dedicacion,
            Modalidad = peticion.ModalidadEnviada ? peticion.Modalidad! : actual.Modalidad,
            FechaIngreso = peticion.FechaIngresoEnviada ? peticion.FechaIngreso!.Value : actual.FechaIngreso,
            FechaSalida = peticion.FechaSalidaEnviada ? peticion.FechaSalida : actual.FechaSalida
        };

        if (string.IsNullOrWhiteSpace(entidad.Dedicacion) || string.IsNullOrWhiteSpace(entidad.Modalidad))
            throw new ArgumentException("Dedicación y modalidad no pueden quedar vacías.");
        ValidarFechas(entidad.FechaIngreso, entidad.FechaSalida);

        return await ActualizarPersistenciaAsync(entidad);
    }

    public async Task<int> RetirarAsync(int docente, int departamento)
    {
        var filas = await _repositorio.RetirarAsync(docente, departamento);
        if (filas == 0)
            throw new NoEncontradoExcepcion("No existe una relación activa con esas claves.");
        return filas;
    }

    // El PATCH conserva los valores ausentes; así las fechas y los campos
    // obligatorios se validan sobre el estado final que se guardará.
    private async Task<int> ActualizarPersistenciaAsync(DocenteDepartamento entidad)
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

    private static void ValidarFechas(DateTime? fechaIngreso, DateTime? fechaSalida)
    {
        if (fechaIngreso is null)
            throw new ArgumentException("La fecha de ingreso es obligatoria.");
        if (fechaSalida is not null && fechaSalida < fechaIngreso)
            throw new ArgumentException("La fecha de salida no puede ser anterior a la fecha de ingreso.");
    }
}
