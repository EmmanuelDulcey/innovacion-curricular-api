namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla `programa` (v2): el maestro central del bloque de
/// Valentina. Cuelga de `facultad` (FK obligatoria) y es padre de las
/// acreditaciones, registros calificados, actividades academicas, pasantias,
/// premios y puentes; por eso no se retira mientras tenga hijos activos.
///
/// Detalles del tipo (5_data_model.md §3.2): las fechas y los contadores son
/// TEXTO (VARCHAR(45)) en esta tabla, salvo `num_acreditaciones`, que es un INT
/// de SOLO LECTURA: lo mantiene el disparador trg_acreditacion_contador y la API
/// jamas lo escribe (por eso no aparece en las peticiones).
/// `facultad_nombre` lo resuelve el LEFT JOIN del procedimiento (solo lectura).
/// </summary>
public class Programa
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Tipo { get; set; }
    public required string Nivel { get; set; }
    public required string FechaCreacion { get; set; }
    public string? FechaCierre { get; set; }        // unica columna opcional
    public required string NumeroCohortes { get; set; }
    public required string CantGraduados { get; set; }
    public required string FechaActualizacion { get; set; }
    public required string Ciudad { get; set; }
    public required int Facultad { get; set; }      // clave que se envia
    public string? FacultadNombre { get; set; }     // alias del JOIN (solo lectura)
    public int NumAcreditaciones { get; set; }      // estado derivado (disparador)
    public bool Activo { get; set; } = true;
}
