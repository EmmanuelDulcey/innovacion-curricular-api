using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

/// <summary>
/// Petición del POST /api/login. Las validaciones de forma se resuelven con
/// DataAnnotations (422) y las de credenciales en el servicio (401).
/// </summary>
public class LoginPeticion
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(100, ErrorMessage = "El usuario no puede superar los 100 caracteres.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(200, ErrorMessage = "La contraseña no puede superar los 200 caracteres.")]
    public string? Password { get; set; }
}
