// =============================================================================
// PruebasAutenticacion.cs
// Pruebas de capa de la v3, sin SQL Server: validan el login con hash bcrypt,
// los rechazos genéricos, la emisión del JWT y la regla de roles de escritura.
// =============================================================================
using System.IdentityModel.Tokens.Jwt;
using ApiInnovacion.Configuracion;
using ApiInnovacion.Excepciones;
using ApiInnovacion.Modelos;
using ApiInnovacion.Peticiones;
using ApiInnovacion.Repositorios;
using ApiInnovacion.Seguridad;
using ApiInnovacion.Servicios;
using Microsoft.Extensions.Options;

namespace PruebaCapas;

public static class PruebasAutenticacion
{
    public static async Task<int> EjecutarAsync()
    {
        var pruebas = 0;
        var hash = new ServicioHashContrasena();
        var repositorio = new RepositorioUsuarioFalso(hash);
        var servicio = new ServicioAutenticacion(repositorio, hash, new TokenFalso());

        // Hash: sal aleatoria, verificación correcta e incorrecta.
        var hashAdmin = hash.Hashear("Demo123!");
        var hashAdmin2 = hash.Hashear("Demo123!");
        Confirmar(hashAdmin.StartsWith("$2"), "Auth: el hash usa formato bcrypt", ref pruebas);
        Confirmar(hashAdmin != hashAdmin2, "Auth: dos hashes de la misma clave difieren (sal)", ref pruebas);
        Confirmar(hash.Verificar("Demo123!", hashAdmin), "Auth: la contraseña correcta verifica", ref pruebas);
        Confirmar(!hash.Verificar("incorrecta", hashAdmin), "Auth: la contraseña incorrecta no verifica", ref pruebas);
        Confirmar(!hash.Verificar("Demo123!", "hash-invalido"), "Auth: un hash malformado no rompe la API", ref pruebas);

        // Login correcto: token, usuario y roles.
        var sesion = await servicio.AutenticarAsync(new LoginPeticion
        {
            Username = "admin",
            Password = "Demo123!",
        });
        Confirmar(sesion.Token == "token-falso", "Auth: el login emite el token", ref pruebas);
        Confirmar(sesion.Usuario.Username == "admin" && sesion.Usuario.Id == 1,
            "Auth: el login devuelve el usuario autenticado", ref pruebas);
        Confirmar(sesion.Roles.Count == 1 && sesion.Roles[0] == PoliticaRoles.Administrador,
            "Auth: el login devuelve los roles", ref pruebas);

        // Usuario con espacios alrededor: se normaliza el username.
        var sesionEspacios = await servicio.AutenticarAsync(new LoginPeticion
        {
            Username = "  admin  ",
            Password = "Demo123!",
        });
        Confirmar(sesionEspacios.Usuario.Username == "admin", "Auth: el username se recorta", ref pruebas);

        // Rechazos genéricos: contraseña, usuario inexistente e inactivo.
        pruebas += await EsperarNoAutorizado(
            () => servicio.AutenticarAsync(new LoginPeticion { Username = "admin", Password = "mala" }),
            "Auth: contraseña incorrecta -> 401");
        pruebas += await EsperarNoAutorizado(
            () => servicio.AutenticarAsync(new LoginPeticion { Username = "nadie", Password = "Demo123!" }),
            "Auth: usuario inexistente -> 401");
        pruebas += await EsperarNoAutorizado(
            () => servicio.AutenticarAsync(new LoginPeticion { Username = "inactivo", Password = "Demo123!" }),
            "Auth: usuario inactivo -> 401");

        // Forma: credenciales vacías y contraseña fuera del límite de bcrypt.
        pruebas += await EsperarArgumento(
            () => servicio.AutenticarAsync(new LoginPeticion { Username = "", Password = "" }),
            "Auth: credenciales vacías -> 400");
        pruebas += await EsperarArgumento(
            () => servicio.AutenticarAsync(new LoginPeticion
            {
                Username = "admin",
                Password = new string('x', 73),
            }),
            "Auth: contraseña de más de 72 bytes -> 400");

        // Usuario sin rol: puede autenticarse, pero no escribir.
        var sinRol = await servicio.AutenticarAsync(new LoginPeticion
        {
            Username = "sinrol",
            Password = "Demo123!",
        });
        Confirmar(sinRol.Roles.Count == 0, "Auth: usuario sin rol se autentica", ref pruebas);
        Confirmar(!PoliticaRoles.PuedeEscribir(sinRol.Roles),
            "Auth: usuario sin rol no puede escribir", ref pruebas);

        // Política de roles: escritura solo para los tres roles operativos.
        Confirmar(PoliticaRoles.PuedeEscribir([PoliticaRoles.Administrador]), "Roles: admin escribe", ref pruebas);
        Confirmar(PoliticaRoles.PuedeEscribir([PoliticaRoles.Coordinador]), "Roles: coordinación escribe", ref pruebas);
        Confirmar(PoliticaRoles.PuedeEscribir([PoliticaRoles.Secretaria]), "Roles: secretaría escribe", ref pruebas);
        Confirmar(!PoliticaRoles.PuedeEscribir([PoliticaRoles.Docente]), "Roles: docente es solo lectura", ref pruebas);
        Confirmar(!PoliticaRoles.PuedeEscribir([PoliticaRoles.Consulta]), "Roles: consulta es solo lectura", ref pruebas);
        Confirmar(!PoliticaRoles.PuedeEscribir([]), "Roles: sin rol es solo lectura", ref pruebas);

        // Emisor real de JWT: token de tres partes, claims y expiración futura.
        var opciones = Options.Create(new JwtOpciones
        {
            Secret = "secreto-de-prueba-con-mas-de-32-caracteres-1234567890",
            Issuer = "InnovacionCurricular",
            Audience = "InnovacionCurricularFrontend",
            ExpirationMinutes = 60,
        });
        var tokenReal = new ServicioTokenJwt(opciones).Generar(sesion.Usuario, sesion.Roles);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokenReal.Token);
        Confirmar(tokenReal.Token.Split('.').Length == 3, "JWT: el token tiene tres partes", ref pruebas);
        Confirmar(tokenReal.Expira > DateTime.UtcNow, "JWT: expira en el futuro", ref pruebas);
        Confirmar(jwt.Claims.Any(claim => claim.Type == "role" && claim.Value == PoliticaRoles.Administrador),
            "JWT: incluye el rol del usuario", ref pruebas);
        Confirmar(jwt.Claims.Any(claim => claim.Type == "nameid" && claim.Value == "1"),
            "JWT: incluye el identificador del usuario", ref pruebas);

        return pruebas;
    }

    private static void Confirmar(bool condicion, string nombre, ref int contador)
    {
        if (!condicion)
            throw new InvalidOperationException($"Verificacion fallida: {nombre}");

        contador++;
        Console.WriteLine($"  [OK] {nombre}");
    }

    private static async Task<int> EsperarNoAutorizado(Func<Task> accion, string nombre)
    {
        try
        {
            await accion();
            throw new InvalidOperationException($"Verificacion fallida: {nombre} (no lanzo excepcion)");
        }
        catch (NoAutorizadoExcepcion)
        {
            Console.WriteLine($"  [OK] {nombre}");
            return 1;
        }
    }

    private static async Task<int> EsperarArgumento(Func<Task> accion, string nombre)
    {
        try
        {
            await accion();
            throw new InvalidOperationException($"Verificacion fallida: {nombre} (no lanzo excepcion)");
        }
        catch (ArgumentException)
        {
            Console.WriteLine($"  [OK] {nombre}");
            return 1;
        }
    }

    private sealed class TokenFalso : ITokenJwt
    {
        public TokenGenerado Generar(Usuario usuario, IReadOnlyCollection<string> roles)
        {
            return new TokenGenerado("token-falso", DateTime.UtcNow.AddHours(1));
        }
    }

    private sealed class RepositorioUsuarioFalso : IRepositorioUsuario
    {
        private readonly Dictionary<string, Usuario> _usuarios;
        private readonly Dictionary<int, List<string>> _roles;

        public RepositorioUsuarioFalso(IServicioHashContrasena hash)
        {
            var clave = hash.Hashear("Demo123!");
            _usuarios = new Dictionary<string, Usuario>(StringComparer.OrdinalIgnoreCase)
            {
                ["admin"] = new Usuario
                {
                    Id = 1,
                    Username = "admin",
                    Password = clave,
                    Email = "admin@innovacion.edu.co",
                    NombreCompleto = "Administrador del sistema",
                    Activo = true,
                },
                ["inactivo"] = new Usuario
                {
                    Id = 2,
                    Username = "inactivo",
                    Password = clave,
                    Email = "inactivo@innovacion.edu.co",
                    NombreCompleto = "Usuario inactivo",
                    Activo = false,
                },
                ["sinrol"] = new Usuario
                {
                    Id = 3,
                    Username = "sinrol",
                    Password = clave,
                    Email = "sinrol@innovacion.edu.co",
                    NombreCompleto = "Usuario sin rol",
                    Activo = true,
                },
            };
            _roles = new Dictionary<int, List<string>>
            {
                [1] = [PoliticaRoles.Administrador],
            };
        }

        public Task<Usuario?> ObtenerPorUsernameAsync(string username)
        {
            return Task.FromResult(_usuarios.GetValueOrDefault(username));
        }

        public Task<List<string>> ObtenerRolesAsync(int usuarioId)
        {
            var roles = _roles.TryGetValue(usuarioId, out var encontrados)
                ? encontrados
                : new List<string>();
            return Task.FromResult(roles);
        }
    }
}
