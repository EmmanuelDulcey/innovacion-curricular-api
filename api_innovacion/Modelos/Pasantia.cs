namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla `pasantia` (v2). Es un detalle de `programa`: se crea
/// siempre ligada a un programa existente. Incluye `programa_nombre`, resuelto
/// por el procedimiento (LEFT JOIN con programa) sólo para mostrar.
/// Fuente: docs/spec_kit/versiones/v2/5_data_model.md, Sección 2.
/// </summary>
public class Pasantia
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Pais { get; set; }
    public required string Empresa { get; set; }
    public required string Descripcion { get; set; }
    public required int Programa { get; set; }
    public string? ProgramaNombre { get; set; }
    public bool Activo { get; set; } = true;
}
