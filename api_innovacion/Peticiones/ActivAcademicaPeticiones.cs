using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// Peticiones del CRUD de `activ_academica` (v2): Crear, Reemplazo y
/// Actualizar. Es la tabla de la UNICA FK opcional (`disenio` -> programa,
/// tropiezo 4): cuando no se elige programa, el campo llega null (o se omite)
/// y se guarda NULL; nunca se convierte la cadena vacia en un valor.
///
/// `espejo` es TINYINT 0/1 en la base: aqui se valida en ese rango exacto.
/// `h_acom` / `h_indep` / `num_creditos` son conteos: enteros no negativos.
/// </summary>
public class ActivAcademicaCrear
{
    // El id interno no se recibe: lo genera SQL Server (6_contracts.md §4).

    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo nombre no puede superar los 45 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo num_creditos es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El campo num_creditos no puede ser negativo.")]
    public int? NumCreditos { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(20, ErrorMessage = "El campo tipo no puede superar los 20 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo area_formacion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo area_formacion no puede superar los 45 caracteres.")]
    public string? AreaFormacion { get; set; }

    [Required(ErrorMessage = "El campo h_acom es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El campo h_acom no puede ser negativo.")]
    public int? HAcom { get; set; }

    [Required(ErrorMessage = "El campo h_indep es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El campo h_indep no puede ser negativo.")]
    public int? HIndep { get; set; }

    [Required(ErrorMessage = "El campo idioma es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo idioma no puede superar los 45 caracteres.")]
    public string? Idioma { get; set; }

    [Required(ErrorMessage = "El campo espejo es obligatorio.")]
    [Range(0, 1, ErrorMessage = "El campo espejo debe ser 0 o 1.")]
    public byte? Espejo { get; set; }

    [Required(ErrorMessage = "El campo entidad_espejo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo entidad_espejo no puede superar los 45 caracteres.")]
    public string? EntidadEspejo { get; set; }

    [Required(ErrorMessage = "El campo pais_espejo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo pais_espejo no puede superar los 45 caracteres.")]
    public string? PaisEspejo { get; set; }

    // FK OPCIONAL: sin [Required]; si viene con valor debe ser un entero positivo.
    [Range(1, int.MaxValue, ErrorMessage = "El campo disenio debe ser un entero positivo.")]
    public int? Disenio { get; set; }
}

/// <summary>
/// Peticion del PUT /api/activ_academica/{id}: reemplazo completo; la PK va en
/// la URL. `disenio` sigue siendo opcional (null = «(ninguna)»).
/// Fuente: 6_contracts.md §5 y §9.4.
/// </summary>
public class ActivAcademicaReemplazo
{
    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo nombre no puede superar los 45 caracteres.")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El campo num_creditos es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El campo num_creditos no puede ser negativo.")]
    public int? NumCreditos { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [StringLength(20, ErrorMessage = "El campo tipo no puede superar los 20 caracteres.")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El campo area_formacion es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo area_formacion no puede superar los 45 caracteres.")]
    public string? AreaFormacion { get; set; }

    [Required(ErrorMessage = "El campo h_acom es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El campo h_acom no puede ser negativo.")]
    public int? HAcom { get; set; }

    [Required(ErrorMessage = "El campo h_indep es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El campo h_indep no puede ser negativo.")]
    public int? HIndep { get; set; }

    [Required(ErrorMessage = "El campo idioma es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo idioma no puede superar los 45 caracteres.")]
    public string? Idioma { get; set; }

    [Required(ErrorMessage = "El campo espejo es obligatorio.")]
    [Range(0, 1, ErrorMessage = "El campo espejo debe ser 0 o 1.")]
    public byte? Espejo { get; set; }

    [Required(ErrorMessage = "El campo entidad_espejo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo entidad_espejo no puede superar los 45 caracteres.")]
    public string? EntidadEspejo { get; set; }

    [Required(ErrorMessage = "El campo pais_espejo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo pais_espejo no puede superar los 45 caracteres.")]
    public string? PaisEspejo { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo disenio debe ser un entero positivo.")]
    public int? Disenio { get; set; }
}

/// <summary>
/// Peticion del PATCH /api/activ_academica/{id}: todo opcional; body sin campos
/// -> 400 (regla de negocio del servicio). Fuente: 6_contracts.md §6.
/// </summary>
public class ActivAcademicaActualizar
{
    [StringLength(45, ErrorMessage = "El campo nombre no puede superar los 45 caracteres.")]
    public string? Nombre { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El campo num_creditos no puede ser negativo.")]
    public int? NumCreditos { get; set; }

    [StringLength(20, ErrorMessage = "El campo tipo no puede superar los 20 caracteres.")]
    public string? Tipo { get; set; }

    [StringLength(45, ErrorMessage = "El campo area_formacion no puede superar los 45 caracteres.")]
    public string? AreaFormacion { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El campo h_acom no puede ser negativo.")]
    public int? HAcom { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El campo h_indep no puede ser negativo.")]
    public int? HIndep { get; set; }

    [StringLength(45, ErrorMessage = "El campo idioma no puede superar los 45 caracteres.")]
    public string? Idioma { get; set; }

    [Range(0, 1, ErrorMessage = "El campo espejo debe ser 0 o 1.")]
    public byte? Espejo { get; set; }

    [StringLength(45, ErrorMessage = "El campo entidad_espejo no puede superar los 45 caracteres.")]
    public string? EntidadEspejo { get; set; }

    [StringLength(45, ErrorMessage = "El campo pais_espejo no puede superar los 45 caracteres.")]
    public string? PaisEspejo { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El campo disenio debe ser un entero positivo.")]
    public int? Disenio { get; set; }
}
