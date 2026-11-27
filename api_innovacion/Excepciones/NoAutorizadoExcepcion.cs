namespace ApiInnovacion.Excepciones;

/// <summary>
/// Credenciales inválidas o usuario inactivo. El controlador la traduce a 401
/// con un mensaje genérico para no revelar si el usuario existe.
/// </summary>
public class NoAutorizadoExcepcion : Exception
{
    public NoAutorizadoExcepcion(string mensaje) : base(mensaje)
    {
    }
}
