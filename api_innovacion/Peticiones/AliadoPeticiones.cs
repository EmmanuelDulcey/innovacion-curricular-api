using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

// Tres peticiones por tabla (3_plan.md, Seccion 4.2): una por verbo.
//   AliadoCrear      -> POST : todo obligatorio, incluida la llave primaria.
//   AliadoReemplazo  -> PUT  : todo obligatorio, sin la llave (va en la URL).
//   AliadoActualizar -> PATCH: todo opcional.
//
// Dos particularidades de `aliado` (issue #3):
//   1. La llave primaria es `nit` y no `id`, asi que el [Range] se aplica
//      sobre Nit (6_contracts.md, Seccion 8.7).
//   2. `correo` exige formato de correo valido ([EmailAddress]), ademas de
//      ser obligatorio y de maximo 70 caracteres (6_contracts.md, Seccion 4.7).
// Las anotaciones producen el 422 automatico antes de tocar SQL Server.

/// <summary>Peticion del POST /api/aliado (6_contracts.md, Seccion 4.7).</summary>
public class AliadoCrear
{
    [Required(ErrorMessage = "El campo nit es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo nit debe ser un entero positivo.")]
    public int? Nit { get; set; }

    [Required(ErrorMessage = "El campo razon_social es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo razon_social no puede superar los 60 caracteres.")]
    public string? RazonSocial { get; set; }

    [Required(ErrorMessage = "El campo nombre_contacto es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo nombre_contacto no puede superar los 60 caracteres.")]
    public string? NombreContacto { get; set; }

    [Required(ErrorMessage = "El campo correo es obligatorio.")]
    [StringLength(70, ErrorMessage = "El campo correo no puede superar los 70 caracteres.")]
    [EmailAddress(ErrorMessage = "El campo correo debe tener un formato válido.")]
    public string? Correo { get; set; }

    [Required(ErrorMessage = "El campo telefono es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo telefono no puede superar los 45 caracteres.")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El campo ciudad es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo ciudad no puede superar los 45 caracteres.")]
    public string? Ciudad { get; set; }
}

/// <summary>Peticion del PUT /api/aliado/{nit} (6_contracts.md, Seccion 5.7).</summary>
public class AliadoReemplazo
{
    [Required(ErrorMessage = "El campo razon_social es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo razon_social no puede superar los 60 caracteres.")]
    public string? RazonSocial { get; set; }

    [Required(ErrorMessage = "El campo nombre_contacto es obligatorio.")]
    [StringLength(60, ErrorMessage = "El campo nombre_contacto no puede superar los 60 caracteres.")]
    public string? NombreContacto { get; set; }

    [Required(ErrorMessage = "El campo correo es obligatorio.")]
    [StringLength(70, ErrorMessage = "El campo correo no puede superar los 70 caracteres.")]
    [EmailAddress(ErrorMessage = "El campo correo debe tener un formato válido.")]
    public string? Correo { get; set; }

    [Required(ErrorMessage = "El campo telefono es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo telefono no puede superar los 45 caracteres.")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El campo ciudad es obligatorio.")]
    [StringLength(45, ErrorMessage = "El campo ciudad no puede superar los 45 caracteres.")]
    public string? Ciudad { get; set; }
}

/// <summary>Peticion del PATCH /api/aliado/{nit} (6_contracts.md, Seccion 6.7).</summary>
public class AliadoActualizar
{
    [StringLength(60, ErrorMessage = "El campo razon_social no puede superar los 60 caracteres.")]
    public string? RazonSocial { get; set; }

    [StringLength(60, ErrorMessage = "El campo nombre_contacto no puede superar los 60 caracteres.")]
    public string? NombreContacto { get; set; }

    // [EmailAddress] devuelve true cuando el valor es null (campo omitido), asi
    // que solo valida el formato cuando el PATCH si envia el correo.
    [StringLength(70, ErrorMessage = "El campo correo no puede superar los 70 caracteres.")]
    [EmailAddress(ErrorMessage = "El campo correo debe tener un formato válido.")]
    public string? Correo { get; set; }

    [StringLength(45, ErrorMessage = "El campo telefono no puede superar los 45 caracteres.")]
    public string? Telefono { get; set; }

    [StringLength(45, ErrorMessage = "El campo ciudad no puede superar los 45 caracteres.")]
    public string? Ciudad { get; set; }
}
