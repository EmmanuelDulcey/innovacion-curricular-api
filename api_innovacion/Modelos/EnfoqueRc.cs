namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla puente `enfoque_rc` (v2): pareja enfoque (tabla de la v1)
/// - registro_calificado (tabla de la v2). PK compuesta, sin atributos propios:
/// una pareja existe o no existe, por eso no hay edicion (tropiezo 9).
///
/// Los dos nombres legibles los resuelve el LEFT JOIN del procedimiento con los
/// alias enfoque_nombre y registro_calificado_nombre (para el segundo se calcula
/// "codigo - tipo_titulacion", porque registro_calificado no tiene columna
/// `nombre`).
/// </summary>
public class EnfoqueRc
{
    public required int Enfoque { get; set; }                 // clave 1 (PK)
    public required int RegistroCalificado { get; set; }      // clave 2 (PK)
    public string? EnfoqueNombre { get; set; }                // solo lectura
    public string? RegistroCalificadoNombre { get; set; }     // solo lectura
    public bool Activo { get; set; } = true;
}
