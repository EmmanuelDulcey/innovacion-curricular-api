namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla `car_innovacion` (v1, tabla sin FK).
/// Una propiedad tipada por columna mas `Activo`.
/// Fuente: docs/spec_kit/versiones/v1_producto_sqlserver/5_data_model.md, Seccion 2.
/// </summary>
public class CarInnovacion
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }

    // En la base es VARCHAR(MAX): no tiene limite practico de longitud.
    public required string Descripcion { get; set; }

    public required string Tipo { get; set; }
    public bool Activo { get; set; } = true;
}
