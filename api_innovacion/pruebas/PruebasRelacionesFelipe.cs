// =============================================================================
// QUÉ ES: pruebas de capa de las relaciones de Felipe, sin SQL Server.
// PAPEL: verifica asignar/retirar de una tabla puente y el CRUD de relaciones
//       con atributos, incluidas sus reglas de fecha y claves inmutables.
// =============================================================================
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace PruebaCapas;

public static class PruebasRelacionesFelipe
{
    public static async Task<int> EjecutarAsync()
    {
        var verificaciones = 0;

        // Las tablas puente puras tienen una pareja como identidad y solo se asignan/retiran.
        var puente = new ServicioProgramaAc(new RepositorioProgramaAcMemoria());
        var creada = await puente.AsignarAsync(new ProgramaAcCrear { Programa = 10, AreaConocimiento = 3 });
        Confirmar(creada.Programa == 10 && creada.AreaConocimiento == 3, "Felipe: asignación de puente", ref verificaciones);
        Confirmar((await puente.ObtenerTodosAsync(10)).Count == 1, "Felipe: listado activo de puente", ref verificaciones);
        Confirmar((await puente.ObtenerPorParejaAsync(10, 3)).Activo, "Felipe: consulta de puente", ref verificaciones);
        Confirmar(await puente.RetirarAsync(10, 3) == 1, "Felipe: retiro lógico de puente", ref verificaciones);
        verificaciones += await EsperarNoEncontrado(() => puente.ObtenerPorParejaAsync(10, 3), "Felipe: consulta de puente retirado");
        verificaciones += await EsperarNoEncontrado(() => puente.RetirarAsync(10, 3), "Felipe: segundo retiro de puente");

        verificaciones += await ProbarDocenteDepartamento();
        verificaciones += await ProbarAlianza();

        // La regla de fecha se valida antes de llamar a SQL Server.
        var docenteSinUso = new ServicioDocenteDepartamento(new RepositorioDocenteNoUsado());
        verificaciones += await EsperarFechaInvalida(() => docenteSinUso.CrearAsync(new DocenteDepartamentoCrear
        {
            Docente = 501, Departamento = 10, Dedicacion = "Completa", Modalidad = "Presencial",
            FechaIngreso = new DateTime(2026, 10, 15), FechaSalida = new DateTime(2026, 10, 14)
        }), "Felipe: fecha de salida inválida al crear");

        var alianzaSinUso = new ServicioAlianza(new RepositorioAlianzaNoUsado());
        verificaciones += await EsperarFechaInvalida(() => alianzaSinUso.CrearAsync(new AlianzaCrear
        {
            Aliado = 900123456, Departamento = 10,
            FechaInicio = new DateTime(2026, 10, 15), FechaFin = new DateTime(2026, 10, 14)
        }), "Felipe: fecha de fin inválida al crear");

        return verificaciones;
    }

    private static async Task<int> ProbarDocenteDepartamento()
    {
        var pruebas = 0;
        var servicio = new ServicioDocenteDepartamento(new RepositorioDocenteDepartamentoMemoria());
        var fila = await servicio.CrearAsync(new DocenteDepartamentoCrear
        {
            Docente = 501, Departamento = 10, Dedicacion = "Completa", Modalidad = "Presencial",
            FechaIngreso = new DateTime(2026, 1, 1), FechaSalida = new DateTime(2026, 12, 31)
        });
        Confirmar(fila.Activo && fila.Dedicacion == "Completa", "Felipe: crear vinculación docente-programa", ref pruebas);

        Confirmar(await servicio.ReemplazarAsync(501, 10, new DocenteDepartamentoReemplazo
        {
            Dedicacion = "Media", Modalidad = "Hibrido", FechaIngreso = new DateTime(2026, 2, 1), FechaSalida = null
        }) == 1, "Felipe: PUT de vinculación", ref pruebas);
        var reemplazada = await servicio.ObtenerPorParejaAsync(501, 10);
        Confirmar(reemplazada.Docente == 501 && reemplazada.Departamento == 10 && reemplazada.Dedicacion == "Media"
            && reemplazada.Modalidad == "Hibrido" && reemplazada.FechaSalida is null,
            "Felipe: PUT conserva claves y reemplaza atributos", ref pruebas);

        Confirmar(await servicio.ActualizarAsync(501, 10, new DocenteDepartamentoActualizar { Modalidad = "Virtual-Remoto" }) == 1,
            "Felipe: PATCH parcial", ref pruebas);
        Confirmar((await servicio.ObtenerPorParejaAsync(501, 10)).Dedicacion == "Media"
            && (await servicio.ObtenerPorParejaAsync(501, 10)).Modalidad == "Virtual-Remoto",
            "Felipe: PATCH conserva campos ausentes", ref pruebas);
        Confirmar(await servicio.ActualizarAsync(501, 10, new DocenteDepartamentoActualizar { FechaSalida = null }) == 1,
            "Felipe: PATCH limpia fecha opcional", ref pruebas);
        pruebas += await EsperarArgumento(() => servicio.ActualizarAsync(501, 10, new DocenteDepartamentoActualizar()),
            "Felipe: PATCH vacío responde 400");
        pruebas += await EsperarFechaInvalida(() => servicio.ActualizarAsync(501, 10,
            new DocenteDepartamentoActualizar { FechaSalida = new DateTime(2026, 1, 31) }),
            "Felipe: PATCH valida coherencia de fechas");

        Confirmar(await servicio.RetirarAsync(501, 10) == 1, "Felipe: retirar vinculación", ref pruebas);
        pruebas += await EsperarNoEncontrado(() => servicio.ObtenerPorParejaAsync(501, 10), "Felipe: docente retirado no aparece");
        return pruebas;
    }

    private static async Task<int> ProbarAlianza()
    {
        var pruebas = 0;
        var servicio = new ServicioAlianza(new RepositorioAlianzaMemoria());
        var fila = await servicio.CrearAsync(new AlianzaCrear
        {
            Aliado = 900123456, Departamento = 10, FechaInicio = new DateTime(2026, 1, 1),
            FechaFin = new DateTime(2026, 12, 31), Docente = 501
        });
        Confirmar(fila.Activo && fila.Docente == 501, "Felipe: crear alianza", ref pruebas);

        Confirmar(await servicio.ReemplazarAsync(900123456, 10, new AlianzaReemplazo
        {
            FechaInicio = new DateTime(2026, 2, 1), FechaFin = null, Docente = null
        }) == 1, "Felipe: PUT de alianza", ref pruebas);
        var reemplazada = await servicio.ObtenerPorParejaAsync(900123456, 10);
        Confirmar(reemplazada.Aliado == 900123456 && reemplazada.Departamento == 10
            && reemplazada.FechaInicio == new DateTime(2026, 2, 1) && reemplazada.FechaFin is null
            && reemplazada.Docente is null,
            "Felipe: PUT conserva claves y reemplaza atributos", ref pruebas);

        Confirmar(await servicio.ActualizarAsync(900123456, 10, new AlianzaActualizar
        {
            Docente = 502, FechaFin = new DateTime(2026, 11, 30)
        }) == 1, "Felipe: PATCH parcial de alianza", ref pruebas);
        var parcial = await servicio.ObtenerPorParejaAsync(900123456, 10);
        Confirmar(parcial.FechaInicio == new DateTime(2026, 2, 1) && parcial.Docente == 502,
            "Felipe: PATCH conserva inicio y modifica docente", ref pruebas);
        Confirmar(await servicio.ActualizarAsync(900123456, 10, new AlianzaActualizar { Docente = null }) == 1,
            "Felipe: PATCH limpia docente opcional", ref pruebas);
        pruebas += await EsperarArgumento(() => servicio.ActualizarAsync(900123456, 10, new AlianzaActualizar()),
            "Felipe: PATCH vacío responde 400");
        pruebas += await EsperarFechaInvalida(() => servicio.ActualizarAsync(900123456, 10,
            new AlianzaActualizar { FechaFin = new DateTime(2026, 1, 31) }),
            "Felipe: PATCH valida coherencia de fechas");

        Confirmar(await servicio.RetirarAsync(900123456, 10) == 1, "Felipe: retirar alianza", ref pruebas);
        pruebas += await EsperarNoEncontrado(() => servicio.ObtenerPorParejaAsync(900123456, 10), "Felipe: alianza retirada no aparece");
        return pruebas;
    }

    private static void Confirmar(bool condicion, string nombre, ref int contador)
    {
        if (!condicion) throw new InvalidOperationException($"Falló {nombre}.");
        contador++;
        Console.WriteLine($"  [OK] {nombre}");
    }

    private static async Task<int> EsperarNoEncontrado(Func<Task> accion, string nombre)
    {
        try { await accion(); }
        catch (NoEncontradoExcepcion)
        {
            Console.WriteLine($"  [OK] {nombre}");
            return 1;
        }
        throw new InvalidOperationException($"Falló {nombre}: faltó el 404.");
    }

    private static async Task<int> EsperarFechaInvalida(Func<Task> accion, string nombre)
    {
        try { await accion(); }
        catch (ArgumentException)
        {
            Console.WriteLine($"  [OK] {nombre}");
            return 1;
        }
        throw new InvalidOperationException($"Falló {nombre}: faltó el 400.");
    }

    private static async Task<int> EsperarArgumento(Func<Task> accion, string nombre)
    {
        try { await accion(); }
        catch (ArgumentException)
        {
            Console.WriteLine($"  [OK] {nombre}");
            return 1;
        }
        throw new InvalidOperationException($"Falló {nombre}: faltó el 400.");
    }
}

internal sealed class RepositorioProgramaAcMemoria : IRepositorioProgramaAc
{
    private ProgramaAc? _fila;
    public Task<List<ProgramaAc>> ObtenerTodosAsync(int limite) =>
        Task.FromResult(_fila is { Activo: true } ? new List<ProgramaAc> { _fila } : new List<ProgramaAc>());
    public Task<ProgramaAc?> ObtenerPorParejaAsync(int programa, int area_conocimiento) =>
        Task.FromResult(_fila is { Activo: true } && _fila.Programa == programa && _fila.AreaConocimiento == area_conocimiento ? _fila : null);
    public Task<ProgramaAc> AsignarAsync(ProgramaAc entidad)
    {
        _fila = entidad;
        return Task.FromResult(entidad);
    }
    public Task<int> RetirarAsync(int programa, int area_conocimiento)
    {
        if (_fila is not { Activo: true } || _fila.Programa != programa || _fila.AreaConocimiento != area_conocimiento)
            return Task.FromResult(0);
        _fila.Activo = false;
        return Task.FromResult(1);
    }
}

// Los dobles en memoria prueban las reglas de negocio sin depender del motor SQL.
internal sealed class RepositorioDocenteDepartamentoMemoria : IRepositorioDocenteDepartamento
{
    private readonly List<DocenteDepartamento> _filas = [];
    public Task<List<DocenteDepartamento>> ObtenerTodosAsync(int limite) =>
        Task.FromResult(_filas.Where(fila => fila.Activo).Take(limite).ToList());
    public Task<DocenteDepartamento?> ObtenerPorParejaAsync(int docente, int departamento) =>
        Task.FromResult(_filas.SingleOrDefault(fila => fila.Activo && fila.Docente == docente && fila.Departamento == departamento));
    public Task<DocenteDepartamento> CrearAsync(DocenteDepartamento entidad)
    {
        _filas.Add(entidad);
        return Task.FromResult(entidad);
    }
    public Task<int> ActualizarAsync(DocenteDepartamento entidad)
    {
        var fila = _filas.SingleOrDefault(actual => actual.Activo && actual.Docente == entidad.Docente && actual.Departamento == entidad.Departamento);
        if (fila is null) return Task.FromResult(0);
        fila.Dedicacion = entidad.Dedicacion;
        fila.Modalidad = entidad.Modalidad;
        fila.FechaIngreso = entidad.FechaIngreso;
        fila.FechaSalida = entidad.FechaSalida;
        return Task.FromResult(1);
    }
    public Task<int> RetirarAsync(int docente, int departamento)
    {
        var fila = _filas.SingleOrDefault(actual => actual.Activo && actual.Docente == docente && actual.Departamento == departamento);
        if (fila is null) return Task.FromResult(0);
        fila.Activo = false;
        return Task.FromResult(1);
    }
}

internal sealed class RepositorioAlianzaMemoria : IRepositorioAlianza
{
    private readonly List<Alianza> _filas = [];
    public Task<List<Alianza>> ObtenerTodosAsync(int limite) =>
        Task.FromResult(_filas.Where(fila => fila.Activo).Take(limite).ToList());
    public Task<Alianza?> ObtenerPorParejaAsync(int aliado, int departamento) =>
        Task.FromResult(_filas.SingleOrDefault(fila => fila.Activo && fila.Aliado == aliado && fila.Departamento == departamento));
    public Task<Alianza> CrearAsync(Alianza entidad)
    {
        _filas.Add(entidad);
        return Task.FromResult(entidad);
    }
    public Task<int> ActualizarAsync(Alianza entidad)
    {
        var fila = _filas.SingleOrDefault(actual => actual.Activo && actual.Aliado == entidad.Aliado && actual.Departamento == entidad.Departamento);
        if (fila is null) return Task.FromResult(0);
        fila.FechaInicio = entidad.FechaInicio;
        fila.FechaFin = entidad.FechaFin;
        fila.Docente = entidad.Docente;
        return Task.FromResult(1);
    }
    public Task<int> RetirarAsync(int aliado, int departamento)
    {
        var fila = _filas.SingleOrDefault(actual => actual.Activo && actual.Aliado == aliado && actual.Departamento == departamento);
        if (fila is null) return Task.FromResult(0);
        fila.Activo = false;
        return Task.FromResult(1);
    }
}

// Estos repositorios fallan intencionalmente si se invocan: una fecha inválida
// debe ser rechazada antes de tocar persistencia.
internal sealed class RepositorioDocenteNoUsado : IRepositorioDocenteDepartamento
{
    public Task<List<DocenteDepartamento>> ObtenerTodosAsync(int limite) => throw new InvalidOperationException();
    public Task<DocenteDepartamento?> ObtenerPorParejaAsync(int docente, int departamento) => throw new InvalidOperationException();
    public Task<DocenteDepartamento> CrearAsync(DocenteDepartamento entidad) => throw new InvalidOperationException();
    public Task<int> ActualizarAsync(DocenteDepartamento entidad) => throw new InvalidOperationException();
    public Task<int> RetirarAsync(int docente, int departamento) => throw new InvalidOperationException();
}

internal sealed class RepositorioAlianzaNoUsado : IRepositorioAlianza
{
    public Task<List<Alianza>> ObtenerTodosAsync(int limite) => throw new InvalidOperationException();
    public Task<Alianza?> ObtenerPorParejaAsync(int aliado, int departamento) => throw new InvalidOperationException();
    public Task<Alianza> CrearAsync(Alianza entidad) => throw new InvalidOperationException();
    public Task<int> ActualizarAsync(Alianza entidad) => throw new InvalidOperationException();
    public Task<int> RetirarAsync(int aliado, int departamento) => throw new InvalidOperationException();
}
