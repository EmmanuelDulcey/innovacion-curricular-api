namespace ApiInnovacion.Modelos;

/// <summary>
/// Entidad de la tabla `aliado` (v1, tabla sin FK).
///
/// Particularidad de esta tabla: su llave primaria se llama `nit`, no `id`
/// (5_data_model.md, Seccion 2). Por eso la propiedad es `Nit` y la columna
/// de la base es `nit`; la serializacion snake_case la expone como "nit"
/// (6_contracts.md, Seccion 0.4). El resto del patron es identico al de las
/// otras seis tablas.
/// </summary>
public class Aliado
{
    public required int Nit { get; set; }
    public required string RazonSocial { get; set; }
    public required string NombreContacto { get; set; }
    public required string Correo { get; set; }
    public required string Telefono { get; set; }
    public required string Ciudad { get; set; }
    public bool Activo { get; set; } = true;
}
