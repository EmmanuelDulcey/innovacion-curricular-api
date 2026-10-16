namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla `facultad` (v2). Es un maestro de `programa`: no se retira
/// mientras tenga programas activos. Incluye `universidad_nombre`, resuelto por
/// el procedimiento (LEFT JOIN con universidad) sólo para mostrar.
/// Fuente: docs/spec_kit/versiones/v2/5_data_model.md, Sección 2.
/// </summary>
public class Facultad
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Tipo { get; set; }
    public required DateTime FechaFun { get; set; }
    public required int Universidad { get; set; }
    public string? UniversidadNombre { get; set; }
    public bool Activo { get; set; } = true;
}
