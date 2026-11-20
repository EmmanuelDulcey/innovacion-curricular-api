using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;

namespace ApiInnovacion.Servicios;

/// <summary>
/// Caso de uso de inicio de sesión: valida credenciales y emite la sesión JWT.
/// No conoce HTTP; el controlador decide los códigos de estado.
/// </summary>
public interface IServicioAutenticacion
{
    Task<SesionUsuario> AutenticarAsync(LoginPeticion peticion);
}
