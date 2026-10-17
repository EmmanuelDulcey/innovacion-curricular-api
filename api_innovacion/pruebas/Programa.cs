using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Servicios;

// El modelo `Programa` (de la API) choca con la clase estatica `Programa` que
// contiene el Main de esta prueba de capas: se le da un alias solo aqui.
using ProgramaModelo = ApiInnovacion.Modelos.Programa;

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
/// Repositorio FALSO en memoria de `facultad` (v2). El flag `tieneHijos` simula
/// el bloqueo del maestro (programas activos): si esta activo, el borrado logico
/// devuelve 0 filas, y el servicio lo traduce a ConflictoExcepcion (409).
/// </summary>
public class RepositorioFacultadFalso : IRepositorioFacultad
{
    private readonly List<Facultad> _datos = new();
    private readonly bool _tieneHijos;

    public RepositorioFacultadFalso(bool tieneHijos = false)
    {
        _tieneHijos = tieneHijos;
    }

    public Task<List<Facultad>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<Facultad?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_datos.FirstOrDefault(d => d.Id == id && d.Activo));

    public Task<Facultad> CrearAsync(Facultad entidad)
    {
        _datos.Add(entidad);
        return Task.FromResult(entidad);
    }

    public Task<int> ActualizarAsync(Facultad entidad)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == entidad.Id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        registro.Nombre = entidad.Nombre;
        registro.Tipo = entidad.Tipo;
        registro.FechaFun = entidad.FechaFun;
        registro.Universidad = entidad.Universidad;
        return Task.FromResult(1);
    }

    public Task<int> EliminarAsync(int id)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        if (_tieneHijos) return Task.FromResult(0);
        registro.Activo = false;
        return Task.FromResult(1);
    }
}

/// <summary>
/// Repositorio FALSO en memoria de `pasantia` (v2, detalle de `programa`).
/// </summary>
public class RepositorioPasantiaFalso : IRepositorioPasantia
{
    private readonly List<Pasantia> _datos = new();

    public Task<List<Pasantia>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<Pasantia?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_datos.FirstOrDefault(d => d.Id == id && d.Activo));

    public Task<Pasantia> CrearAsync(Pasantia entidad)
    {
        _datos.Add(entidad);
        return Task.FromResult(entidad);
    }

    public Task<int> ActualizarAsync(Pasantia entidad)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == entidad.Id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        registro.Nombre = entidad.Nombre;
        registro.Pais = entidad.Pais;
        registro.Empresa = entidad.Empresa;
        registro.Descripcion = entidad.Descripcion;
        registro.Programa = entidad.Programa;
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

/// <summary>
/// Repositorio FALSO en memoria de `premio` (v2, detalle de `programa`).
/// </summary>
public class RepositorioPremioFalso : IRepositorioPremio
{
    private readonly List<Premio> _datos = new();

    public Task<List<Premio>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<Premio?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_datos.FirstOrDefault(d => d.Id == id && d.Activo));

    public Task<Premio> CrearAsync(Premio entidad)
    {
        _datos.Add(entidad);
        return Task.FromResult(entidad);
    }

    public Task<int> ActualizarAsync(Premio entidad)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == entidad.Id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        registro.Nombre = entidad.Nombre;
        registro.Descripcion = entidad.Descripcion;
        registro.Fecha = entidad.Fecha;
        registro.EntidadOtorgante = entidad.EntidadOtorgante;
        registro.Pais = entidad.Pais;
        registro.Programa = entidad.Programa;
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

/// <summary>
/// Repositorio FALSO en memoria de `programa` (v2, maestro central). El flag
/// `tieneHijos` simula el bloqueo del retiro por hijos activos (la regla §3.1,
/// que el procedimiento resuelve con el mapa de 5_data_model.md §4.2).
/// </summary>
public class RepositorioProgramaFalso : IRepositorioPrograma
{
    private readonly List<ProgramaModelo> _datos = new();
    private readonly bool _tieneHijos;

    public RepositorioProgramaFalso(bool tieneHijos = false)
    {
        _tieneHijos = tieneHijos;
    }

    public Task<List<ProgramaModelo>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<ProgramaModelo?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_datos.FirstOrDefault(d => d.Id == id && d.Activo));

    public Task<ProgramaModelo> CrearAsync(ProgramaModelo entidad)
    {
        _datos.Add(entidad);
        return Task.FromResult(entidad);
    }

    public Task<int> ActualizarAsync(ProgramaModelo entidad)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == entidad.Id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        registro.Nombre = entidad.Nombre;
        registro.Tipo = entidad.Tipo;
        registro.Nivel = entidad.Nivel;
        registro.FechaCreacion = entidad.FechaCreacion;
        registro.FechaCierre = entidad.FechaCierre;
        registro.NumeroCohortes = entidad.NumeroCohortes;
        registro.CantGraduados = entidad.CantGraduados;
        registro.FechaActualizacion = entidad.FechaActualizacion;
        registro.Ciudad = entidad.Ciudad;
        registro.Facultad = entidad.Facultad;
        return Task.FromResult(1);
    }

    public Task<int> EliminarAsync(int id)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        if (_tieneHijos) return Task.FromResult(0);
        registro.Activo = false;
        return Task.FromResult(1);
    }
}

/// <summary>
/// Repositorio FALSO en memoria de `acreditacion` (v2, detalle de programa).
/// </summary>
public class RepositorioAcreditacionFalso : IRepositorioAcreditacion
{
    private readonly List<Acreditacion> _datos = new();

    public Task<List<Acreditacion>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<Acreditacion?> ObtenerPorIdAsync(int resolucion)
        => Task.FromResult(_datos.FirstOrDefault(d => d.Resolucion == resolucion && d.Activo));

    public Task<Acreditacion> CrearAsync(Acreditacion entidad)
    {
        _datos.Add(entidad);
        return Task.FromResult(entidad);
    }

    public Task<int> ActualizarAsync(Acreditacion entidad)
    {
        var registro = _datos.FirstOrDefault(d => d.Resolucion == entidad.Resolucion && d.Activo);
        if (registro is null) return Task.FromResult(0);
        registro.Tipo = entidad.Tipo;
        registro.Calificacion = entidad.Calificacion;
        registro.FechaInicio = entidad.FechaInicio;
        registro.FechaFin = entidad.FechaFin;
        registro.Programa = entidad.Programa;
        return Task.FromResult(1);
    }

    public Task<int> EliminarAsync(int resolucion)
    {
        var registro = _datos.FirstOrDefault(d => d.Resolucion == resolucion && d.Activo);
        if (registro is null) return Task.FromResult(0);
        registro.Activo = false;
        return Task.FromResult(1);
    }
}

/// <summary>
/// Repositorio FALSO en memoria de `registro_calificado` (v2). `tieneHijos`
/// simula el bloqueo del retiro por sus puentes activas (enfoque_rc, aa_rc).
/// </summary>
public class RepositorioRegistroCalificadoFalso : IRepositorioRegistroCalificado
{
    private readonly List<RegistroCalificado> _datos = new();
    private readonly bool _tieneHijos;

    public RepositorioRegistroCalificadoFalso(bool tieneHijos = false)
    {
        _tieneHijos = tieneHijos;
    }

    public Task<List<RegistroCalificado>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<RegistroCalificado?> ObtenerPorIdAsync(int codigo)
        => Task.FromResult(_datos.FirstOrDefault(d => d.Codigo == codigo && d.Activo));

    public Task<RegistroCalificado> CrearAsync(RegistroCalificado entidad)
    {
        _datos.Add(entidad);
        return Task.FromResult(entidad);
    }

    public Task<int> ActualizarAsync(RegistroCalificado entidad)
    {
        var registro = _datos.FirstOrDefault(d => d.Codigo == entidad.Codigo && d.Activo);
        if (registro is null) return Task.FromResult(0);
        registro.CantCreditos = entidad.CantCreditos;
        registro.HoraAcom = entidad.HoraAcom;
        registro.HoraInd = entidad.HoraInd;
        registro.Metodologia = entidad.Metodologia;
        registro.FechaInicio = entidad.FechaInicio;
        registro.FechaFin = entidad.FechaFin;
        registro.DuracionAnios = entidad.DuracionAnios;
        registro.DuracionSemestres = entidad.DuracionSemestres;
        registro.TipoTitulacion = entidad.TipoTitulacion;
        registro.Programa = entidad.Programa;
        return Task.FromResult(1);
    }

    public Task<int> EliminarAsync(int codigo)
    {
        var registro = _datos.FirstOrDefault(d => d.Codigo == codigo && d.Activo);
        if (registro is null) return Task.FromResult(0);
        if (_tieneHijos) return Task.FromResult(0);
        registro.Activo = false;
        return Task.FromResult(1);
    }
}

/// <summary>
/// Repositorio FALSO en memoria de `activ_academica` (v2). `tieneHijos` simula
/// el bloqueo del retiro por parejas activas en aa_rc.
/// </summary>
public class RepositorioActivAcademicaFalso : IRepositorioActivAcademica
{
    private readonly List<ActivAcademica> _datos = new();
    private readonly bool _tieneHijos;

    public RepositorioActivAcademicaFalso(bool tieneHijos = false)
    {
        _tieneHijos = tieneHijos;
    }

    public Task<List<ActivAcademica>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<ActivAcademica?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_datos.FirstOrDefault(d => d.Id == id && d.Activo));

    public Task<ActivAcademica> CrearAsync(ActivAcademica entidad)
    {
        _datos.Add(entidad);
        return Task.FromResult(entidad);
    }

    public Task<int> ActualizarAsync(ActivAcademica entidad)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == entidad.Id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        registro.Nombre = entidad.Nombre;
        registro.NumCreditos = entidad.NumCreditos;
        registro.Tipo = entidad.Tipo;
        registro.AreaFormacion = entidad.AreaFormacion;
        registro.HAcom = entidad.HAcom;
        registro.HIndep = entidad.HIndep;
        registro.Idioma = entidad.Idioma;
        registro.Espejo = entidad.Espejo;
        registro.EntidadEspejo = entidad.EntidadEspejo;
        registro.PaisEspejo = entidad.PaisEspejo;
        registro.Disenio = entidad.Disenio;   // puede ser null (FK opcional)
        return Task.FromResult(1);
    }

    public Task<int> EliminarAsync(int id)
    {
        var registro = _datos.FirstOrDefault(d => d.Id == id && d.Activo);
        if (registro is null) return Task.FromResult(0);
        if (_tieneHijos) return Task.FromResult(0);
        registro.Activo = false;
        return Task.FromResult(1);
    }
}

/// <summary>
/// Repositorio FALSO en memoria de la puente `enfoque_rc` (v2). La pareja
/// repetida se rechaza como lo haria la PK compuesta del motor: el servicio
/// real traduce el error 2627 a ConflictoExcepcion (409); aqui el doble lanza
/// esa misma traduccion porque no hay SQL Server en la prueba de capas.
/// </summary>
public class RepositorioEnfoqueRcFalso : IRepositorioEnfoqueRc
{
    private readonly List<EnfoqueRc> _datos = new();

    public Task<List<EnfoqueRc>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<EnfoqueRc?> ObtenerPorIdAsync(int enfoque, int registroCalificado)
        => Task.FromResult(_datos.FirstOrDefault(d =>
            d.Enfoque == enfoque && d.RegistroCalificado == registroCalificado && d.Activo));

    public Task<EnfoqueRc> AsignarAsync(EnfoqueRc entidad)
    {
        // La PK compuesta no distingue activo: una pareja existe o no existe.
        if (_datos.Any(d => d.Enfoque == entidad.Enfoque && d.RegistroCalificado == entidad.RegistroCalificado))
            throw new ConflictoExcepcion("Esa pareja ya está registrada.");

        _datos.Add(entidad);
        return Task.FromResult(entidad);
    }

    public Task<int> RetirarAsync(int enfoque, int registroCalificado)
    {
        var registro = _datos.FirstOrDefault(d =>
            d.Enfoque == enfoque && d.RegistroCalificado == registroCalificado && d.Activo);
        if (registro is null) return Task.FromResult(0);
        registro.Activo = false;
        return Task.FromResult(1);
    }
}

/// <summary>
/// Repositorio FALSO en memoria de la puente `aa_rc` (v2): pareja + componente
/// y semestre; la pareja repetida se rechaza igual que en enfoque_rc.
/// </summary>
public class RepositorioAaRcFalso : IRepositorioAaRc
{
    private readonly List<AaRc> _datos = new();

    public Task<List<AaRc>> ObtenerTodosAsync(int limite)
        => Task.FromResult(_datos.Where(d => d.Activo).Take(limite).ToList());

    public Task<AaRc?> ObtenerPorIdAsync(int activAcademicasIdcurso, int registroCalificadoCodigo)
        => Task.FromResult(_datos.FirstOrDefault(d =>
            d.ActivAcademicasIdcurso == activAcademicasIdcurso
            && d.RegistroCalificadoCodigo == registroCalificadoCodigo
            && d.Activo));

    public Task<AaRc> AsignarAsync(AaRc entidad)
    {
        if (_datos.Any(d =>
                d.ActivAcademicasIdcurso == entidad.ActivAcademicasIdcurso
                && d.RegistroCalificadoCodigo == entidad.RegistroCalificadoCodigo))
            throw new ConflictoExcepcion("Esa pareja ya está registrada.");

        _datos.Add(entidad);
        return Task.FromResult(entidad);
    }

    public Task<int> RetirarAsync(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        var registro = _datos.FirstOrDefault(d =>
            d.ActivAcademicasIdcurso == activAcademicasIdcurso
            && d.RegistroCalificadoCodigo == registroCalificadoCodigo
            && d.Activo);
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
            await ProbarFacultad();
            await ProbarPasantia();
            await ProbarPremio();
            await ProbarPrograma();
            await ProbarAcreditacion();
            await ProbarRegistroCalificado();
            await ProbarActivAcademica();
            await ProbarEnfoqueRc();
            await ProbarAaRc();

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

    private static async Task ProbarFacultad()
    {
        // `facultad` (v2) es un maestro de `programa`: chequeo del ciclo CRUD sin
        // base de datos y de la regla §3.1 (no se retira con hijos activos).
        IServicioFacultad servicio = new ServicioFacultad(new RepositorioFacultadFalso());

        var creado = await servicio.CrearAsync(new FacultadCrear
        {
            Id = 2,
            Nombre = "Facultad de Ingeniería",
            Tipo = "Pública",
            FechaFun = new DateTime(1960, 3, 15),
            Universidad = 1
        });
        Confirmar(creado.Id == 2 && creado.Nombre == "Facultad de Ingeniería", "40. Facultad creada (devuelve la fila)");

        Confirmar((await servicio.ObtenerTodosAsync(1000)).Count == 1, "41. Facultad listada");

        var registro = await servicio.ObtenerPorIdAsync(2);
        Confirmar(registro.Universidad == 1, "42. Facultad obtenida por id");

        await servicio.ReemplazarAsync(2, new FacultadReemplazo
        {
            Nombre = "Facultad de Ciencias",
            Tipo = "Privada",
            FechaFun = new DateTime(1975, 8, 1),
            Universidad = 2
        });
        registro = await servicio.ObtenerPorIdAsync(2);
        Confirmar(registro.Nombre == "Facultad de Ciencias" && registro.Universidad == 2, "43. Facultad reemplazada (PUT)");

        await servicio.ActualizarAsync(2, new FacultadActualizar { Tipo = "Mixta" });
        registro = await servicio.ObtenerPorIdAsync(2);
        Confirmar(registro.Tipo == "Mixta" && registro.Nombre == "Facultad de Ciencias", "44. Facultad actualizada parcialmente (PATCH)");

        await EsperarExcepcion(
            () => servicio.ObtenerPorIdAsync(999),
            typeof(NoEncontradoExcepcion),
            "45. Facultad inexistente devuelve 404");

        await EsperarExcepcion(
            () => servicio.ActualizarAsync(2, new FacultadActualizar()),
            typeof(ArgumentException),
            "46. PATCH vacío de facultad lanza ArgumentException");

        Confirmar(await servicio.EliminarAsync(2) == 1, "47. Borrado lógico de facultad");

        await EsperarExcepcion(
            () => servicio.EliminarAsync(2),
            typeof(NoEncontradoExcepcion),
            "48. Segundo DELETE de facultad devuelve 404");

        // Regla §3.1: el maestro con hijos activos no se retira -> ConflictoExcepcion (409).
        IServicioFacultad servicioBloqueado = new ServicioFacultad(new RepositorioFacultadFalso(tieneHijos: true));
        await servicioBloqueado.CrearAsync(new FacultadCrear
        {
            Id = 3,
            Nombre = "Facultad con programas",
            Tipo = "Pública",
            FechaFun = new DateTime(1990, 1, 1),
            Universidad = 1
        });
        await EsperarExcepcion(
            () => servicioBloqueado.EliminarAsync(3),
            typeof(ConflictoExcepcion),
            "49. Facultad con hijos activos lanza ConflictoExcepcion (409)");
    }

    private static async Task ProbarPasantia()
    {
        // `pasantia` (v2) es un detalle de `programa`: ciclo CRUD completo.
        IServicioPasantia servicio = new ServicioPasantia(new RepositorioPasantiaFalso());

        var creado = await servicio.CrearAsync(new PasantiaCrear
        {
            Id = 2,
            Nombre = "Pasantía internacional",
            Pais = "Chile",
            Empresa = "ACME S.A.",
            Descripcion = "Práctica profesional en el exterior",
            Programa = 10
        });
        Confirmar(creado.Id == 2 && creado.Programa == 10, "50. Pasantía creada (devuelve la fila)");

        Confirmar((await servicio.ObtenerTodosAsync(1000)).Count == 1, "51. Pasantía listada");

        Confirmar((await servicio.ObtenerPorIdAsync(2)).Pais == "Chile", "52. Pasantía obtenida por id");

        await servicio.ReemplazarAsync(2, new PasantiaReemplazo
        {
            Nombre = "Pasantía nacional",
            Pais = "Colombia",
            Empresa = "Ecopetrol",
            Descripcion = "Práctica en el país",
            Programa = 11
        });
        var registro = await servicio.ObtenerPorIdAsync(2);
        Confirmar(registro.Pais == "Colombia" && registro.Programa == 11, "53. Pasantía reemplazada (PUT)");

        await servicio.ActualizarAsync(2, new PasantiaActualizar { Empresa = "Ecopetrol S.A." });
        registro = await servicio.ObtenerPorIdAsync(2);
        Confirmar(registro.Empresa == "Ecopetrol S.A." && registro.Pais == "Colombia", "54. Pasantía actualizada parcialmente (PATCH)");

        await EsperarExcepcion(
            () => servicio.ObtenerPorIdAsync(999),
            typeof(NoEncontradoExcepcion),
            "55. Pasantía inexistente devuelve 404");

        await EsperarExcepcion(
            () => servicio.ActualizarAsync(2, new PasantiaActualizar()),
            typeof(ArgumentException),
            "56. PATCH vacío de pasantía lanza ArgumentException");

        Confirmar(await servicio.EliminarAsync(2) == 1, "57. Borrado lógico de pasantía");

        await EsperarExcepcion(
            () => servicio.ObtenerPorIdAsync(2),
            typeof(NoEncontradoExcepcion),
            "58. Pasantía inactiva devuelve 404");
    }

    private static async Task ProbarPremio()
    {
        // `premio` (v2) es un detalle de `programa`: ciclo CRUD completo.
        IServicioPremio servicio = new ServicioPremio(new RepositorioPremioFalso());

        var creado = await servicio.CrearAsync(new PremioCrear
        {
            Id = 2,
            Nombre = "Premio a la innovación",
            Descripcion = "Reconocimiento nacional",
            Fecha = new DateTime(2020, 11, 20),
            EntidadOtorgante = "MEN",
            Pais = "Colombia",
            Programa = 10
        });
        Confirmar(creado.Id == 2 && creado.EntidadOtorgante == "MEN", "59. Premio creado (devuelve la fila)");

        Confirmar((await servicio.ObtenerTodosAsync(1000)).Count == 1, "60. Premio listado");

        Confirmar((await servicio.ObtenerPorIdAsync(2)).Pais == "Colombia", "61. Premio obtenido por id");

        await servicio.ReemplazarAsync(2, new PremioReemplazo
        {
            Nombre = "Premio a la calidad",
            Descripcion = "Reconocimiento internacional",
            Fecha = new DateTime(2021, 5, 10),
            EntidadOtorgante = "UNESCO",
            Pais = "Francia",
            Programa = 11
        });
        var registro = await servicio.ObtenerPorIdAsync(2);
        Confirmar(registro.Nombre == "Premio a la calidad" && registro.EntidadOtorgante == "UNESCO", "62. Premio reemplazado (PUT)");

        await servicio.ActualizarAsync(2, new PremioActualizar { Pais = "Suiza" });
        registro = await servicio.ObtenerPorIdAsync(2);
        Confirmar(registro.Pais == "Suiza" && registro.Nombre == "Premio a la calidad", "63. Premio actualizado parcialmente (PATCH)");

        await EsperarExcepcion(
            () => servicio.ObtenerPorIdAsync(999),
            typeof(NoEncontradoExcepcion),
            "64. Premio inexistente devuelve 404");

        await EsperarExcepcion(
            () => servicio.ActualizarAsync(2, new PremioActualizar()),
            typeof(ArgumentException),
            "65. PATCH vacío de premio lanza ArgumentException");

        Confirmar(await servicio.EliminarAsync(2) == 1, "66. Borrado lógico de premio");

        await EsperarExcepcion(
            () => servicio.ObtenerPorIdAsync(2),
            typeof(NoEncontradoExcepcion),
            "67. Premio inactivo devuelve 404");
    }

    private static async Task ProbarPrograma()
    {
        // `programa` (v2) es el maestro central del bloque de Valentina: ciclo
        // CRUD sin base de datos y regla §3.1 (no se retira con hijos activos).
        IServicioPrograma servicio = new ServicioPrograma(new RepositorioProgramaFalso());

        var creado = await servicio.CrearAsync(new ProgramaCrear
        {
            Id = 2,
            Nombre = "Ingeniería de Sistemas",
            Tipo = "Profesional",
            Nivel = "Pregrado",
            FechaCreacion = "2001-02-01",
            FechaCierre = null,                       // opcional: viaja null
            NumeroCohortes = "24",
            CantGraduados = "320",
            FechaActualizacion = "2025-01-15",
            Ciudad = "Medellín",
            Facultad = 1
        });
        Confirmar(creado.Id == 2 && creado.Facultad == 1, "68. Programa creado (devuelve la fila)");

        Confirmar((await servicio.ObtenerTodosAsync(1000)).Count == 1, "69. Programa listado");

        var registro = await servicio.ObtenerPorIdAsync(2);
        Confirmar(registro.Nivel == "Pregrado" && registro.FechaCierre is null, "70. Programa obtenido por id (con fecha de cierre nula)");

        await servicio.ReemplazarAsync(2, new ProgramaReemplazo
        {
            Nombre = "Ingeniería de Software",
            Tipo = "Profesional",
            Nivel = "Pregrado",
            FechaCreacion = "2001-02-01",
            FechaCierre = "2020-12-31",
            NumeroCohortes = "24",
            CantGraduados = "330",
            FechaActualizacion = "2025-06-30",
            Ciudad = "Medellín",
            Facultad = 2
        });
        registro = await servicio.ObtenerPorIdAsync(2);
        Confirmar(registro.Nombre == "Ingeniería de Software" && registro.Facultad == 2, "71. Programa reemplazado (PUT)");

        await servicio.ActualizarAsync(2, new ProgramaActualizar { Nivel = "Posgrado" });
        registro = await servicio.ObtenerPorIdAsync(2);
        Confirmar(registro.Nivel == "Posgrado" && registro.Nombre == "Ingeniería de Software", "72. Programa actualizado parcialmente (PATCH)");

        await EsperarExcepcion(
            () => servicio.ObtenerPorIdAsync(999),
            typeof(NoEncontradoExcepcion),
            "73. Programa inexistente devuelve 404");

        await EsperarExcepcion(
            () => servicio.ActualizarAsync(2, new ProgramaActualizar()),
            typeof(ArgumentException),
            "74. PATCH vacío de programa lanza ArgumentException");

        Confirmar(await servicio.EliminarAsync(2) == 1, "75. Borrado lógico de programa");

        await EsperarExcepcion(
            () => servicio.EliminarAsync(2),
            typeof(NoEncontradoExcepcion),
            "76. Segundo DELETE de programa devuelve 404");

        // Regla §3.1: el maestro con hijos activos no se retira -> ConflictoExcepcion (409).
        IServicioPrograma servicioBloqueado = new ServicioPrograma(new RepositorioProgramaFalso(tieneHijos: true));
        await servicioBloqueado.CrearAsync(new ProgramaCrear
        {
            Id = 3,
            Nombre = "Programa con detalle",
            Tipo = "Profesional",
            Nivel = "Pregrado",
            FechaCreacion = "2010-01-01",
            NumeroCohortes = "10",
            CantGraduados = "50",
            FechaActualizacion = "2025-01-01",
            Ciudad = "Medellín",
            Facultad = 1
        });
        await EsperarExcepcion(
            () => servicioBloqueado.EliminarAsync(3),
            typeof(ConflictoExcepcion),
            "77. Programa con hijos activos lanza ConflictoExcepcion (409)");
    }

    private static async Task ProbarAcreditacion()
    {
        // `acreditacion` (v2) es detalle de `programa` con PK `resolucion`.
        IServicioAcreditacion servicio = new ServicioAcreditacion(new RepositorioAcreditacionFalso());

        var creado = await servicio.CrearAsync(new AcreditacionCrear
        {
            Resolucion = 400,
            Tipo = "Nacional",
            Calificacion = "Alta calidad",
            FechaInicio = "2020-01-01",
            FechaFin = "2024-01-01",
            Programa = 2
        });
        Confirmar(creado.Resolucion == 400 && creado.Programa == 2, "78. Acreditación creada (devuelve la fila)");

        Confirmar((await servicio.ObtenerTodosAsync(1000)).Count == 1, "79. Acreditación listada");

        Confirmar((await servicio.ObtenerPorIdAsync(400)).Calificacion == "Alta calidad", "80. Acreditación obtenida por resolución");

        await servicio.ReemplazarAsync(400, new AcreditacionReemplazo
        {
            Tipo = "Internacional",
            Calificacion = "Excelente",
            FechaInicio = "2021-01-01",
            FechaFin = "2025-01-01",
            Programa = 3
        });
        var registro = await servicio.ObtenerPorIdAsync(400);
        Confirmar(registro.Tipo == "Internacional" && registro.Programa == 3, "81. Acreditación reemplazada (PUT)");

        await servicio.ActualizarAsync(400, new AcreditacionActualizar { Calificacion = "Superior" });
        registro = await servicio.ObtenerPorIdAsync(400);
        Confirmar(registro.Calificacion == "Superior" && registro.Tipo == "Internacional", "82. Acreditación actualizada parcialmente (PATCH)");

        await EsperarExcepcion(
            () => servicio.ObtenerPorIdAsync(999),
            typeof(NoEncontradoExcepcion),
            "83. Acreditación inexistente devuelve 404");

        await EsperarExcepcion(
            () => servicio.ActualizarAsync(400, new AcreditacionActualizar()),
            typeof(ArgumentException),
            "84. PATCH vacío de acreditación lanza ArgumentException");

        Confirmar(await servicio.EliminarAsync(400) == 1, "85. Borrado lógico de acreditación (el disparador baja el contador)");

        await EsperarExcepcion(
            () => servicio.ObtenerPorIdAsync(400),
            typeof(NoEncontradoExcepcion),
            "86. Acreditación inactiva devuelve 404");
    }

    private static async Task ProbarRegistroCalificado()
    {
        // `registro_calificado` (v2): PK `codigo` y fechas DATE (DateTime).
        IServicioRegistroCalificado servicio =
            new ServicioRegistroCalificado(new RepositorioRegistroCalificadoFalso());

        var creado = await servicio.CrearAsync(new RegistroCalificadoCrear
        {
            Codigo = 500,
            CantCreditos = "160",
            HoraAcom = "20",
            HoraInd = "30",
            Metodologia = "Presencial",
            FechaInicio = new DateTime(2020, 1, 15),
            FechaFin = new DateTime(2024, 1, 15),
            DuracionAnios = "4",
            DuracionSemestres = "8",
            TipoTitulacion = "Profesional",
            Programa = 2
        });
        Confirmar(creado.Codigo == 500 && creado.FechaInicio == new DateTime(2020, 1, 15), "87. Registro calificado creado (devuelve la fila)");

        Confirmar((await servicio.ObtenerTodosAsync(1000)).Count == 1, "88. Registro calificado listado");

        Confirmar((await servicio.ObtenerPorIdAsync(500)).TipoTitulacion == "Profesional", "89. Registro calificado obtenido por código");

        await servicio.ReemplazarAsync(500, new RegistroCalificadoReemplazo
        {
            CantCreditos = "170",
            HoraAcom = "22",
            HoraInd = "28",
            Metodologia = "Híbrida",
            FechaInicio = new DateTime(2021, 1, 15),
            FechaFin = new DateTime(2025, 1, 15),
            DuracionAnios = "4",
            DuracionSemestres = "8",
            TipoTitulacion = "Profesional",
            Programa = 3
        });
        var registro = await servicio.ObtenerPorIdAsync(500);
        Confirmar(registro.Metodologia == "Híbrida" && registro.Programa == 3, "90. Registro calificado reemplazado (PUT)");

        await servicio.ActualizarAsync(500, new RegistroCalificadoActualizar { DuracionAnios = "5" });
        registro = await servicio.ObtenerPorIdAsync(500);
        Confirmar(registro.DuracionAnios == "5" && registro.Metodologia == "Híbrida", "91. Registro calificado actualizado parcialmente (PATCH)");

        await EsperarExcepcion(
            () => servicio.ObtenerPorIdAsync(999),
            typeof(NoEncontradoExcepcion),
            "92. Registro calificado inexistente devuelve 404");

        await EsperarExcepcion(
            () => servicio.ActualizarAsync(500, new RegistroCalificadoActualizar()),
            typeof(ArgumentException),
            "93. PATCH vacío de registro calificado lanza ArgumentException");

        Confirmar(await servicio.EliminarAsync(500) == 1, "94. Borrado lógico de registro calificado");

        await EsperarExcepcion(
            () => servicio.EliminarAsync(500),
            typeof(NoEncontradoExcepcion),
            "95. Segundo DELETE de registro calificado devuelve 404");

        // Padre con puentes activas (enfoque_rc / aa_rc) -> 409.
        IServicioRegistroCalificado servicioBloqueado =
            new ServicioRegistroCalificado(new RepositorioRegistroCalificadoFalso(tieneHijos: true));
        await servicioBloqueado.CrearAsync(new RegistroCalificadoCrear
        {
            Codigo = 501,
            CantCreditos = "160",
            HoraAcom = "20",
            HoraInd = "30",
            Metodologia = "Presencial",
            FechaInicio = new DateTime(2020, 1, 15),
            FechaFin = new DateTime(2024, 1, 15),
            DuracionAnios = "4",
            DuracionSemestres = "8",
            TipoTitulacion = "Profesional",
            Programa = 2
        });
        await EsperarExcepcion(
            () => servicioBloqueado.EliminarAsync(501),
            typeof(ConflictoExcepcion),
            "96. Registro calificado con parejas activas lanza ConflictoExcepcion (409)");
    }

    private static async Task ProbarActivAcademica()
    {
        // `activ_academica` (v2): la ÚNICA FK opcional es `disenio` (puede ser
        // null); `espejo` es TINYINT 0/1 (byte en C#).
        IServicioActivAcademica servicio =
            new ServicioActivAcademica(new RepositorioActivAcademicaFalso());

        var creado = await servicio.CrearAsync(new ActivAcademicaCrear
        {
            Id = 600,
            Nombre = "Cátedra de innovación",
            NumCreditos = 3,
            Tipo = "Curso",
            AreaFormacion = "Sociohumanística",
            HAcom = 4,
            HIndep = 5,
            Idioma = "Español",
            Espejo = 0,
            EntidadEspejo = "N/A",
            PaisEspejo = "N/A",
            Disenio = null                      // «(ninguna)»: FK opcional en null
        });
        Confirmar(creado.Id == 600 && creado.Disenio is null, "97. Actividad académica creada con diseño nulo (FK opcional)");

        Confirmar((await servicio.ObtenerTodosAsync(1000)).Count == 1, "98. Actividad académica listada");

        var registro = await servicio.ObtenerPorIdAsync(600);
        Confirmar(registro.Espejo == 0 && registro.Nombre == "Cátedra de innovación", "99. Actividad académica obtenida por id");

        await servicio.ReemplazarAsync(600, new ActivAcademicaReemplazo
        {
            Nombre = "Cátedra de innovación abierta",
            NumCreditos = 3,
            Tipo = "Curso",
            AreaFormacion = "Sociohumanística",
            HAcom = 4,
            HIndep = 6,
            Idioma = "Español",
            Espejo = 1,
            EntidadEspejo = "Universidad de Antioquia",
            PaisEspejo = "Colombia",
            Disenio = 2                          // ahora sí referencia un programa
        });
        registro = await servicio.ObtenerPorIdAsync(600);
        Confirmar(registro.Espejo == 1 && registro.Disenio == 2, "100. Actividad académica reemplazada (PUT)");

        await servicio.ActualizarAsync(600, new ActivAcademicaActualizar { HAcom = 5 });
        registro = await servicio.ObtenerPorIdAsync(600);
        Confirmar(registro.HAcom == 5 && registro.Nombre == "Cátedra de innovación abierta", "101. Actividad académica actualizada parcialmente (PATCH)");

        await EsperarExcepcion(
            () => servicio.ObtenerPorIdAsync(999),
            typeof(NoEncontradoExcepcion),
            "102. Actividad académica inexistente devuelve 404");

        await EsperarExcepcion(
            () => servicio.ActualizarAsync(600, new ActivAcademicaActualizar()),
            typeof(ArgumentException),
            "103. PATCH vacío de actividad académica lanza ArgumentException");

        Confirmar(await servicio.EliminarAsync(600) == 1, "104. Borrado lógico de actividad académica");

        await EsperarExcepcion(
            () => servicio.EliminarAsync(600),
            typeof(NoEncontradoExcepcion),
            "105. Segundo DELETE de actividad académica devuelve 404");

        // Padre con parejas activas en aa_rc -> 409.
        IServicioActivAcademica servicioBloqueado =
            new ServicioActivAcademica(new RepositorioActivAcademicaFalso(tieneHijos: true));
        await servicioBloqueado.CrearAsync(new ActivAcademicaCrear
        {
            Id = 601,
            Nombre = "Actividad con parejas",
            NumCreditos = 2,
            Tipo = "Curso",
            AreaFormacion = "Disciplinar",
            HAcom = 2,
            HIndep = 2,
            Idioma = "Español",
            Espejo = 0,
            EntidadEspejo = "N/A",
            PaisEspejo = "N/A",
            Disenio = null
        });
        await EsperarExcepcion(
            () => servicioBloqueado.EliminarAsync(601),
            typeof(ConflictoExcepcion),
            "106. Actividad académica con parejas activas lanza ConflictoExcepcion (409)");
    }

    private static async Task ProbarEnfoqueRc()
    {
        // Puente `enfoque_rc` (v2): se asigna y se retira por pareja, sin
        // edición. La pareja repetida la rechaza la PK compuesta: el doble
        // lanza la misma ConflictoExcepcion que el servicio produce contra la
        // base al traducir el error 2627.
        IServicioEnfoqueRc servicio = new ServicioEnfoqueRc(new RepositorioEnfoqueRcFalso());

        var asignada = await servicio.AsignarAsync(new EnfoqueRcPareja { Enfoque = 2, RegistroCalificado = 500 });
        Confirmar(asignada.Enfoque == 2 && asignada.RegistroCalificado == 500, "107. Enfoque del registro asignado (devuelve la pareja)");

        Confirmar((await servicio.ObtenerTodosAsync(1000)).Count == 1, "108. Enfoque del registro listado");

        Confirmar((await servicio.ObtenerPorIdAsync(2, 500)).Enfoque == 2, "109. Enfoque del registro consultado por pareja");

        await EsperarExcepcion(
            () => servicio.AsignarAsync(new EnfoqueRcPareja { Enfoque = 2, RegistroCalificado = 500 }),
            typeof(ConflictoExcepcion),
            "110. Pareja repetida lanza ConflictoExcepcion (409)");

        Confirmar(await servicio.RetirarAsync(2, 500) == 1, "111. Pareja retirada (1 fila)");

        await EsperarExcepcion(
            () => servicio.RetirarAsync(2, 500),
            typeof(NoEncontradoExcepcion),
            "112. Retirar pareja inexistente lanza NoEncontradoExcepcion (404)");
    }

    private static async Task ProbarAaRc()
    {
        // Puente `aa_rc` (v2): pareja + `componente` y `semestre` (atributos
        // que viajan en la asignación). Igual patrón que enfoque_rc.
        IServicioAaRc servicio = new ServicioAaRc(new RepositorioAaRcFalso());

        var asignada = await servicio.AsignarAsync(new AaRcPareja
        {
            ActivAcademicasIdcurso = 600,
            RegistroCalificadoCodigo = 500,
            Componente = "Investigación",
            Semestre = "2025-1"
        });
        Confirmar(
            asignada.ActivAcademicasIdcurso == 600 && asignada.Componente == "Investigación",
            "113. Actividad del registro asignada con componente y semestre");

        Confirmar((await servicio.ObtenerTodosAsync(1000)).Count == 1, "114. Actividad del registro listada");

        Confirmar((await servicio.ObtenerPorIdAsync(600, 500)).Semestre == "2025-1", "115. Actividad del registro consultada por pareja");

        await EsperarExcepcion(
            () => servicio.AsignarAsync(new AaRcPareja
            {
                ActivAcademicasIdcurso = 600,
                RegistroCalificadoCodigo = 500,
                Componente = "Investigación",
                Semestre = "2025-1"
            }),
            typeof(ConflictoExcepcion),
            "116. Pareja repetida en aa_rc lanza ConflictoExcepcion (409)");

        Confirmar(await servicio.RetirarAsync(600, 500) == 1, "117. Pareja de aa_rc retirada (1 fila)");

        await EsperarExcepcion(
            () => servicio.RetirarAsync(600, 500),
            typeof(NoEncontradoExcepcion),
            "118. Retirar pareja inexistente de aa_rc lanza NoEncontradoExcepcion (404)");
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
