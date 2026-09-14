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

            Console.WriteLine($"Prueba de capas SUPERADA: {_contador} verificaciones en verde.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Prueba de capas FALLADA: {ex.Message}");
            return 1;
        }
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