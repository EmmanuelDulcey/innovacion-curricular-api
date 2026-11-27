namespace ApiInnovacion.Servicios;

/// <summary>
/// Contrato de hashing de contraseñas. La implementación usa bcrypt; el resto
/// de la aplicación nunca manipula contraseñas en texto plano más allá del
/// inicio de sesión.
/// </summary>
public interface IServicioHashContrasena
{
    string Hashear(string contrasena);
    bool Verificar(string contrasena, string hash);
}
