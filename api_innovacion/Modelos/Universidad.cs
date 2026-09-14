namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla `universidad` (v1, tabla sin FK).
/// La llave primaria es `id`, entero positivo asignado por el cliente (sin IDENTITY).
/// Fuente: docs/spec_kit/versiones/v1_producto_sqlserver/5_data_model.md, Seccion 2.
/// </summary>
public class Universidad
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Tipo { get; set; }
    public required string Ciudad { get; set; }
    public bool Activo { get; set; } = true;
}