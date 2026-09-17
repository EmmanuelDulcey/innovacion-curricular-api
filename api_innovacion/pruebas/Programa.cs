using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

namespace PruebaCapas;

/// <summary>
/// Repositorio FALSO en memoria: implementa IRepositorioAreaConocimiento sin tocar
/// SQL Server. Permite validar el aislamiento de las capas (criterio 5 de 2_spec.md §5):
/// "una ejecucion con repositorio falso en memoria pasa todas las verificaciones sin SQL Server".
/// </summary>
public class RepositorioAreaConocimientoFalso : IRepositorioAreaConocimiento
{
    private readonly List<AreaConocimiento> _datos = new();

    public RepositorioAreaConocimientoFalso()
    {
        _datos.Add(new AreaConocimiento { Id = 1, GranArea = "Ingeniería", Area = "Ingeniería de Sistemas", Disciplina = "Computación", Activo = true });
    }

    public Task<List<AreaConocimiento>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<AreaConocimiento?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_datos.FirstOrDefault(d => d.Id == id && d.Activo));

    public Task CrearAsync(AreaConocimiento entidad)
    {
        _datos.Add(entidad);
        return Task.CompletedTask;
    }

    public Task<int> ActualizarAsync(int id, Dictionary<string, object> datos)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == id && d.Activo);
        if (registro is null)
            return Task.FromResult(0);

        if (datos.TryGetValue("gran_area", out var granArea)) registro.GranArea = (string)granArea;
        if (datos.TryGetValue("area", out var area)) registro.Area = (string)area;
        if (datos.TryGetValue("disciplina", out var disciplina)) registro.Disciplina = (string)disciplina;

        return Task.FromResult(1);
    }

    public Task<int> EliminarAsync(int id)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == id && d.Activo);
        if (registro is null)
            return Task.FromResult(0);

        registro.Activo = false;
        return Task.FromResult(1);
    }
}

public class RepositorioEnfoqueFalso : IRepositorioEnfoque
{
    private readonly List<Enfoque> _datos = new();

    public Task<List<Enfoque>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<Enfoque?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_datos.FirstOrDefault(d => d.Id == id && d.Activo));

    public Task CrearAsync(Enfoque entidad)
    {
        _datos.Add(entidad);
        return Task.CompletedTask;
    }

    public Task<int> ActualizarAsync(int id, Dictionary<string, object> datos)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        if (datos.TryGetValue("nombre", out var nombre)) registro.Nombre = (string)nombre;
        if (datos.TryGetValue("descripcion", out var descripcion)) registro.Descripcion = (string)descripcion;
        return Task.FromResult(1);
    }

    public Task<int> EliminarAsync(int id)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        registro.Activo = false;
        return Task.FromResult(1);
    }
}

public class RepositorioAspectoNormativoFalso : IRepositorioAspectoNormativo
{
    private readonly List<AspectoNormativo> _datos = new();

    public Task<List<AspectoNormativo>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<AspectoNormativo?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_datos.FirstOrDefault(d => d.Id == id && d.Activo));

    public Task CrearAsync(AspectoNormativo entidad)
    {
        _datos.Add(entidad);
        return Task.CompletedTask;
    }

    public Task<int> ActualizarAsync(int id, Dictionary<string, object> datos)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        if (datos.TryGetValue("tipo", out var tipo)) registro.Tipo = (string)tipo;
        if (datos.TryGetValue("descripcion", out var descripcion)) registro.Descripcion = (string)descripcion;
        if (datos.TryGetValue("fuente", out var fuente)) registro.Fuente = (string)fuente;
        return Task.FromResult(1);
    }

    public Task<int> EliminarAsync(int id)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        registro.Activo = false;
        return Task.FromResult(1);
    }
}

public class RepositorioPracticaEstrategiaFalso : IRepositorioPracticaEstrategia
{
    private readonly List<PracticaEstrategia> _datos = new();

    public Task<List<PracticaEstrategia>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<PracticaEstrategia?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_datos.FirstOrDefault(d => d.Id == id && d.Activo));

    public Task CrearAsync(PracticaEstrategia entidad)
    {
        _datos.Add(entidad);
        return Task.CompletedTask;
    }

    public Task<int> ActualizarAsync(int id, Dictionary<string, object> datos)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        if (datos.TryGetValue("tipo", out var tipo)) registro.Tipo = (string)tipo;
        if (datos.TryGetValue("nombre", out var nombre)) registro.Nombre = (string)nombre;
        if (datos.TryGetValue("descripcion", out var descripcion)) registro.Descripcion = (string)descripcion;
        return Task.FromResult(1);
    }

    public Task<int> EliminarAsync(int id)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        registro.Activo = false;
        return Task.FromResult(1);
    }
}

public class RepositorioCarInnovacionFalso : IRepositorioCarInnovacion
{
    private readonly List<CarInnovacion> _datos = new();

    public Task<List<CarInnovacion>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<CarInnovacion?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_datos.FirstOrDefault(d => d.Id == id && d.Activo));

    public Task CrearAsync(CarInnovacion entidad)
    {
        _datos.Add(entidad);
        return Task.CompletedTask;
    }

    public Task<int> ActualizarAsync(int id, Dictionary<string, object> datos)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        if (datos.TryGetValue("nombre", out var nombre)) registro.Nombre = (string)nombre;
        if (datos.TryGetValue("descripcion", out var descripcion)) registro.Descripcion = (string)descripcion;
        if (datos.TryGetValue("tipo", out var tipo)) registro.Tipo = (string)tipo;
        return Task.FromResult(1);
    }

    public Task<int> EliminarAsync(int id)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        registro.Activo = false;
        return Task.FromResult(1);
    }
}

public class RepositorioAliadoFalso : IRepositorioAliado
{
    private readonly List<Aliado> _datos = new();

    public Task<List<Aliado>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<Aliado?> ObtenerPorIdAsync(int nit)
        => Task.FromResult(_datos.FirstOrDefault(d => d.Nit == nit && d.Activo));

    public Task CrearAsync(Aliado entidad)
    {
        _datos.Add(entidad);
        return Task.CompletedTask;
    }

    public Task<int> ActualizarAsync(int nit, Dictionary<string, object> datos)
    {
        var registro = _datos.FirstOrDefault(d => d.Nit == nit && d.Activo);
        if (registro is null) return Task.FromResult(0);
        if (datos.TryGetValue("razon_social", out var razonSocial)) registro.RazonSocial = (string)razonSocial;
        if (datos.TryGetValue("nombre_contacto", out var nombreContacto)) registro.NombreContacto = (string)nombreContacto;
        if (datos.TryGetValue("correo", out var correo)) registro.Correo = (string)correo;
        if (datos.TryGetValue("telefono", out var telefono)) registro.Telefono = (string)telefono;
        if (datos.TryGetValue("ciudad", out var ciudad)) registro.Ciudad = (string)ciudad;
        return Task.FromResult(1);
    }

    public Task<int> EliminarAsync(int nit)
    {
        var registro = _datos.FirstOrDefault(d => d.Nit == nit && d.Activo);
        if (registro is null) return Task.FromResult(0);
        registro.Activo = false;
        return Task.FromResult(1);
    }
}

/// <summary>
/// Programa de consola (prueba de capas). Verificaciones:
///   1. Crear nuevo registro via servicio.
///   2. Listar: el registro aparece.
///   3. Obtener por id: devuelve el registro.
///   4. Reemplazo (PUT): cambia todos los campos.
///   5. Actualizacion parcial (PATCH): cambia solo el enviado.
///   6. Obtener id inexistente -> NoEncontradoExcepcion.
///   7. PATCH sin campos -> ArgumentException.
///   8. Eliminar (borrado logico) y segundo DELETE -> NoEncontradoExcepcion.
/// Termina con codigo 0 si todo pasa; 1 y mensaje en caso contrario.
/// </summary>
public static class Programa
{
    private static int _contador = 0;

    public static async Task<int> Main()
    {
        try
        {
            await ProbarAreaConocimiento();
            await ProbarEnfoque();
            await ProbarAspectoNormativo();
            await ProbarPracticaEstrategia();
            await ProbarCarInnovacion();
            await ProbarAliado();

            Console.WriteLine($"Prueba de capas SUPERADA: {_contador} verificaciones en verde.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Prueba de capas FALLADA: {ex.Message}");
            return 1;
        }
    }

    private static async Task ProbarAreaConocimiento()
    {
        IServicioAreaConocimiento servicio =
            new ServicioAreaConocimiento(new RepositorioAreaConocimientoFalso());

        // 1. Crear
        await servicio.CrearAsync(new AreaConocimientoCrear
        {
            Id = 2,
            GranArea = "Ingeniería",
            Area = "Ingeniería de Sistemas",
            Disciplina = "Desarrollo de Software"
        });
        Confirmar(true, "1. Crear registro");

        // 2. Listar
        var lista = await servicio.ObtenerTodosAsync(1000);
        Confirmar(lista.Count == 2, "2. Listar (2 registros)");

        // 3. Obtener por id
        var buscado = await servicio.ObtenerPorIdAsync(2);
        Confirmar(buscado.Disciplina == "Desarrollo de Software", "3. Obtener por id");

        // 4. Reemplazo (PUT)
        await servicio.ReemplazarAsync(2, new AreaConocimientoReemplazo
        {
            GranArea = "Tecnología",
            Area = "Ingeniería de Software",
            Disciplina = "Arquitectura"
        });
        buscado = await servicio.ObtenerPorIdAsync(2);
        Confirmar(buscado.GranArea == "Tecnología" && buscado.Disciplina == "Arquitectura", "4. Reemplazo completo (PUT)");

        // 5. Actualizacion parcial (PATCH)
        await servicio.ActualizarAsync(2, new AreaConocimientoActualizar { Disciplina = "Calidad de Software" });
        buscado = await servicio.ObtenerPorIdAsync(2);
        Confirmar(buscado.Disciplina == "Calidad de Software" && buscado.Area == "Ingeniería de Software", "5. Actualizacion parcial (PATCH)");

        // 6. Id inexistente
        await EsperarExcepcion(
            () => servicio.ObtenerPorIdAsync(999),
            typeof(NoEncontradoExcepcion),
            "6. Id inexistente lanza NoEncontradoExcepcion");

        // 7. PATCH sin campos
        await EsperarExcepcion(
            () => servicio.ActualizarAsync(2, new AreaConocimientoActualizar()),
            typeof(ArgumentException),
            "7. PATCH sin campos lanza ArgumentException");

        // 8. Eliminar (borrado logico) y segundo DELETE
        var filas = await servicio.EliminarAsync(2);
        Confirmar(filas == 1, "8a. Borrado logico (1 fila)");

        await EsperarExcepcion(
            () => servicio.EliminarAsync(2),
            typeof(NoEncontradoExcepcion),
            "8b. Segundo DELETE lanza NoEncontradoExcepcion");

    }

    private static async Task ProbarEnfoque()
    {
        IServicioEnfoque servicio = new ServicioEnfoque(new RepositorioEnfoqueFalso());
        await servicio.CrearAsync(new EnfoqueCrear { Id = 2, Nombre = "Activo", Descripcion = "Participación" });
        Confirmar((await servicio.ObtenerTodosAsync(1000)).Count == 1, "9. Enfoque creado y listado");
        var registro = await servicio.ObtenerPorIdAsync(2);
        Confirmar(registro.Nombre == "Activo", "10. Enfoque obtenido por id");
        await servicio.ReemplazarAsync(2, new EnfoqueReemplazo { Nombre = "Colaborativo", Descripcion = "Trabajo conjunto" });
        await servicio.ActualizarAsync(2, new EnfoqueActualizar { Descripcion = "Participación conjunta" });
        registro = await servicio.ObtenerPorIdAsync(2);
        Confirmar(registro.Nombre == "Colaborativo" && registro.Descripcion == "Participación conjunta", "11. Enfoque PUT y PATCH");
        await EsperarExcepcion(() => servicio.ActualizarAsync(2, new EnfoqueActualizar()), typeof(ArgumentException), "12. PATCH vacío de enfoque");
        Confirmar(await servicio.EliminarAsync(2) == 1, "13. Borrado lógico de enfoque");
        await EsperarExcepcion(() => servicio.ObtenerPorIdAsync(2), typeof(NoEncontradoExcepcion), "14. Enfoque inactivo devuelve 404");
    }

    private static async Task ProbarAspectoNormativo()
    {
        IServicioAspectoNormativo servicio = new ServicioAspectoNormativo(new RepositorioAspectoNormativoFalso());
        await servicio.CrearAsync(new AspectoNormativoCrear { Id = 3, Tipo = "Ley", Descripcion = "Marco", Fuente = "MEN" });
        Confirmar((await servicio.ObtenerTodosAsync(1000)).Count == 1, "15. Aspecto normativo creado y listado");
        var registro = await servicio.ObtenerPorIdAsync(3);
        Confirmar(registro.Fuente == "MEN", "16. Aspecto normativo obtenido por id");
        await servicio.ReemplazarAsync(3, new AspectoNormativoReemplazo { Tipo = "Decreto", Descripcion = "Marco actualizado", Fuente = "Consejo" });
        await servicio.ActualizarAsync(3, new AspectoNormativoActualizar { Fuente = "Comité" });
        registro = await servicio.ObtenerPorIdAsync(3);
        Confirmar(registro.Tipo == "Decreto" && registro.Fuente == "Comité", "17. Aspecto normativo PUT y PATCH");
        await EsperarExcepcion(() => servicio.ActualizarAsync(3, new AspectoNormativoActualizar()), typeof(ArgumentException), "18. PATCH vacío de aspecto normativo");
        Confirmar(await servicio.EliminarAsync(3) == 1, "19. Borrado lógico de aspecto normativo");
        await EsperarExcepcion(() => servicio.EliminarAsync(3), typeof(NoEncontradoExcepcion), "20. Segundo DELETE de aspecto normativo");
    }

    private static async Task ProbarPracticaEstrategia()
    {
        IServicioPracticaEstrategia servicio = new ServicioPracticaEstrategia(new RepositorioPracticaEstrategiaFalso());
        await servicio.CrearAsync(new PracticaEstrategiaCrear { Id = 4, Tipo = "Práctica", Nombre = "Proyectos", Descripcion = "Reto" });
        Confirmar((await servicio.ObtenerTodosAsync(1000)).Count == 1, "21. Práctica creada y listada");
        var registro = await servicio.ObtenerPorIdAsync(4);
        Confirmar(registro.Nombre == "Proyectos", "22. Práctica obtenida por id");
        await servicio.ReemplazarAsync(4, new PracticaEstrategiaReemplazo { Tipo = "Estrategia", Nombre = "Colaboración", Descripcion = "Trabajo conjunto" });
        await servicio.ActualizarAsync(4, new PracticaEstrategiaActualizar { Nombre = "Colaboración guiada" });
        registro = await servicio.ObtenerPorIdAsync(4);
        Confirmar(registro.Tipo == "Estrategia" && registro.Nombre == "Colaboración guiada", "23. Práctica PUT y PATCH");
        await EsperarExcepcion(() => servicio.ActualizarAsync(4, new PracticaEstrategiaActualizar()), typeof(ArgumentException), "24. PATCH vacío de práctica");
        Confirmar(await servicio.EliminarAsync(4) == 1, "25. Borrado lógico de práctica");
        await EsperarExcepcion(() => servicio.ObtenerPorIdAsync(4), typeof(NoEncontradoExcepcion), "26. Práctica inactiva devuelve 404");
    }

    private static async Task ProbarCarInnovacion()
    {
        IServicioCarInnovacion servicio = new ServicioCarInnovacion(new RepositorioCarInnovacionFalso());
        await servicio.CrearAsync(new CarInnovacionCrear { Id = 1, Nombre = "Innovación pedagógica", Descripcion = "Descripción", Tipo = "Pedagógica" });
        Confirmar((await servicio.ObtenerTodosAsync(1000)).Count == 1, "27. Característica creada y listada");
        var registro = await servicio.ObtenerPorIdAsync(1);
        Confirmar(registro.Tipo == "Pedagógica", "28. Característica obtenida por id");
        await servicio.ReemplazarAsync(1, new CarInnovacionReemplazo { Nombre = "Innovación curricular", Descripcion = "Descripción completa", Tipo = "Curricular" });
        await servicio.ActualizarAsync(1, new CarInnovacionActualizar { Tipo = "Tecnológica" });
        registro = await servicio.ObtenerPorIdAsync(1);
        Confirmar(registro.Nombre == "Innovación curricular" && registro.Tipo == "Tecnológica", "29. Característica PUT y PATCH");
        await EsperarExcepcion(() => servicio.ActualizarAsync(1, new CarInnovacionActualizar()), typeof(ArgumentException), "30. PATCH vacío de característica");
        Confirmar(await servicio.EliminarAsync(1) == 1, "31. Borrado lógico de característica");
        await EsperarExcepcion(() => servicio.EliminarAsync(1), typeof(NoEncontradoExcepcion), "32. Segundo DELETE de característica");
    }

    private static async Task ProbarAliado()
    {
        // `aliado` es la tabla del ciclo CRUD completo del criterio 3 (2_spec.md §5).
        // Aqui se comprueba sin base de datos; el mismo ciclo contra SQL Server se
        // verifica con curl segun 7_quickstart.md §8.
        IServicioAliado servicio = new ServicioAliado(new RepositorioAliadoFalso());
        await servicio.CrearAsync(new AliadoCrear
        {
            Nit = 900123456,
            RazonSocial = "Empresa Ejemplo",
            NombreContacto = "Juan Pérez",
            Correo = "contacto@empresa.com",
            Telefono = "3001234567",
            Ciudad = "Medellín"
        });
        Confirmar((await servicio.ObtenerTodosAsync(1000)).Count == 1, "33. Aliado creado y listado");
        var registro = await servicio.ObtenerPorIdAsync(900123456);
        Confirmar(registro.RazonSocial == "Empresa Ejemplo" && registro.Correo == "contacto@empresa.com", "34. Aliado obtenido por nit");
        await servicio.ReemplazarAsync(900123456, new AliadoReemplazo
        {
            RazonSocial = "Empresa Actualizada",
            NombreContacto = "Carlos Pérez",
            Correo = "carlos@empresa.com",
            Telefono = "3009876543",
            Ciudad = "Medellín"
        });
        registro = await servicio.ObtenerPorIdAsync(900123456);
        Confirmar(registro.RazonSocial == "Empresa Actualizada" && registro.NombreContacto == "Carlos Pérez", "35. Aliado reemplazado (PUT)");
        await servicio.ActualizarAsync(900123456, new AliadoActualizar { Telefono = "3001112233" });
        registro = await servicio.ObtenerPorIdAsync(900123456);
        Confirmar(registro.Telefono == "3001112233" && registro.Correo == "carlos@empresa.com", "36. Aliado actualizado parcialmente (PATCH)");
        await EsperarExcepcion(() => servicio.ActualizarAsync(900123456, new AliadoActualizar()), typeof(ArgumentException), "37. PATCH vacío de aliado");
        Confirmar(await servicio.EliminarAsync(900123456) == 1, "38. Borrado lógico de aliado");
        await EsperarExcepcion(() => servicio.ObtenerPorIdAsync(900123456), typeof(NoEncontradoExcepcion), "39. Aliado inactivo devuelve 404");
    }

    private static void Confirmar(bool condicion, string nombre)
    {
        if (!condicion)
            throw new InvalidOperationException($"Verificacion fallida: {nombre}");

        _contador++;
        Console.WriteLine($"  [OK] {nombre}");
    }

    private static async Task EsperarExcepcion(Func<Task> accion, Type tipoEsperado, string nombre)
    {
        try
        {
            await accion();
            throw new InvalidOperationException($"Verificacion fallida: {nombre} (no lanzo excepcion)");
        }
        catch (Exception ex) when (tipoEsperado.IsInstanceOfType(ex))
        {
            _contador++;
            Console.WriteLine($"  [OK] {nombre} -> {ex.GetType().Name}: {ex.Message}");
        }
    }
}
