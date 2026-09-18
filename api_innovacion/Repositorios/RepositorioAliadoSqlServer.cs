using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de `aliado` sobre SQL Server con Dapper.
/// Misma plantilla que las otras seis tablas (3_plan.md, Seccion 4.5), con la
/// unica diferencia de que la llave primaria es la columna `nit`.
/// SQL escrito a mano y parametrizado (@parametro). Todas las lecturas filtran
/// activo = 1; la eliminacion es un UPDATE de borrado logico, nunca un DELETE
/// fisico.
/// </summary>
public class RepositorioAliadoSqlServer : IRepositorioAliado
{
    private readonly string _cadenaConexion;

    public RepositorioAliadoSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<Aliado>> ObtenerTodosAsync(int limite)
    {
        const string sql = """
            SELECT TOP (@limite) nit, razon_social, nombre_contacto, correo, telefono, ciudad, activo
              FROM aliado
             WHERE activo = 1
             ORDER BY nit;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<Aliado>(sql, new { limite });
        return filas.AsList();
    }

    public async Task<Aliado?> ObtenerPorIdAsync(int nit)
    {
        const string sql = """
            SELECT nit, razon_social, nombre_contacto, correo, telefono, ciudad, activo
              FROM aliado
             WHERE nit = @nit AND activo = 1;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<Aliado>(sql, new { nit });
    }

    public async Task CrearAsync(Aliado entidad)
    {
        const string sql = """
            INSERT INTO aliado (nit, razon_social, nombre_contacto, correo, telefono, ciudad, activo)
            VALUES (@nit, @razon_social, @nombre_contacto, @correo, @telefono, @ciudad, 1);
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.ExecuteAsync(sql, new
        {
            entidad.Nit,
            razon_social = entidad.RazonSocial,
            nombre_contacto = entidad.NombreContacto,
            entidad.Correo,
            entidad.Telefono,
            entidad.Ciudad
        });
    }

    public async Task<int> ActualizarAsync(int nit, Dictionary<string, object> datos)
    {
        // PUT y PATCH llegan aqui: el diccionario ya contiene solo las columnas a
        // actualizar con sus claves en snake_case (las mapea el servicio).
        if (datos.Count == 0)
            return 0;

        var conjuntoSet = string.Join(", ", datos.Keys.Select(clave => $"{clave} = @{clave}"));
        var sql = $"""
            UPDATE aliado
               SET {conjuntoSet}
             WHERE nit = @nit AND activo = 1;
            """;

        var parametros = new DynamicParameters();
        foreach (var par in datos)
            parametros.Add(par.Key, par.Value);
        parametros.Add("nit", nit);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarAsync(int nit)
    {
        const string sql = """
            UPDATE aliado
               SET activo = 0
             WHERE nit = @nit AND activo = 1;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { nit });
    }
}
