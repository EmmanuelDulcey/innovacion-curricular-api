namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla `area_conocimiento` (v1, tabla sin FK).
/// La llave primaria es `id`, entero positivo generado por la secuencia de SQL Server.
/// Fuente: docs/spec_kit/versiones/v1_producto_sqlserver/5_data_model.md, Seccion 2.
/// </summary>
public class AreaConocimiento
{
    public int Id { get; set; }
    public required string GranArea { get; set; }
    public required string Area { get; set; }
    public required string Disciplina { get; set; }
    public bool Activo { get; set; } = true;
}
