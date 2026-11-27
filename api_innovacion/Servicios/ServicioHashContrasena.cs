using BCrypt.Net;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Hash de contraseñas con bcrypt (work factor 12). Cada hash incluye su propia
/// sal aleatoria, por lo que dos contraseñas iguales producen hashes distintos.
/// `Verificar` devuelve false ante un hash malformado en lugar de romper la API.
/// </summary>
public class ServicioHashContrasena : IServicioHashContrasena
{
    private const int FactorTrabajo = 12;

    public string Hashear(string contrasena)
    {
        return BCrypt.Net.BCrypt.HashPassword(contrasena, FactorTrabajo);
    }

    public bool Verificar(string contrasena, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(contrasena, hash);
        }
        catch (SaltParseException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
