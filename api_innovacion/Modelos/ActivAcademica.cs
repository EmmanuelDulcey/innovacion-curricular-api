namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla `activ_academica` (v2): detalle de `programa` a traves de
/// la columna `disenio`, que es la UNICA clave foranea OPCIONAL del bloque
/// (tropiezo 4): llega como null, nunca como cadena vacia, y se guarda NULL.
///
/// `espejo` es TINYINT 0/1 en la base (5_data_model.md §3.5): en C# viaja como
/// byte para no cambiar el tipo del script. `disenio_nombre` lo resuelve el
/// LEFT JOIN del procedimiento (solo lectura).
///
/// Es padre de la puente aa_rc; no se retira con parejas activas.
/// </summary>
public class ActivAcademica
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public required int NumCreditos { get; set; }
    public required string Tipo { get; set; }
    public required string AreaFormacion { get; set; }
    public required int HAcom { get; set; }
    public required int HIndep { get; set; }
    public required string Idioma { get; set; }
    public required byte Espejo { get; set; }        // TINYINT 0/1
    public required string EntidadEspejo { get; set; }
    public required string PaisEspejo { get; set; }
    public int? Disenio { get; set; }                // FK OPCIONAL -> null
    public string? DisenioNombre { get; set; }       // alias del JOIN (solo lectura)
    public bool Activo { get; set; } = true;
}
