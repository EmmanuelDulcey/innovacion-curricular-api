using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de `universidad` sobre SQL Server con Dapper.
/// SQL escrito a mano y parametrizado (@parametro). Todas las lecturas filtran
/// activo = 1; la eliminacion es un UPDATE de borrado logico, nunca DELETE físico.
/// Fuente: docs/spec_kit/versiones/v1_producto_sqlserver/3_plan.md, Seccion 4.5.
/// </summary>
public class RepositorioUniversidadSqlServer : IRepositorioUniversidad
{
    private readonly string _cadenaConexion;

    public RepositorioUniversidadSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<Universidad>> ObtenerTodosAsync(int limite)
    {
        const string sql = """
            SELECT TOP (@limite) id, nombre, tipo, ciudad, activo
              FROM universidad
             WHERE activo = 1
             ORDER BY id;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<Universidad>(sql, new { limite });
        return filas.AsList();
    }

    public async Task<Universidad?> ObtenerPorIdAsync(int id)
    {
        const string sql = """
            SELECT id, nombre, tipo, ciudad, activo
              FROM universidad
             WHERE id = @id AND activo = 1;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<Universidad>(sql, new { id });
    }

    public async Task CrearAsync(Universidad entidad)
    {
        const string sql = """
            INSERT INTO universidad (id, nombre, tipo, ciudad, activo)
            VALUES (@id, @nombre, @tipo, @ciudad, 1);
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.ExecuteAsync(sql, new
        {
            entidad.Id,
            entidad.Nombre,
            entidad.Tipo,
            entidad.Ciudad
        });
    }

    public async Task<int> ActualizarAsync(int id, Dictionary<string, object> datos)
    {
        if (datos.Count == 0)
            return 0;

        var conjuntoSet = string.Join(", ", datos.Keys.Select(clave => $"{clave} = @{clave}"));
        var sql = $"""
            UPDATE universidad
               SET {conjuntoSet}
             WHERE id = @id AND activo = 1;
            """;

        var parametros = new DynamicParameters();
        foreach (var par in datos)
            parametros.Add(par.Key, par.Value);
        parametros.Add("id", id);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarAsync(int id)
    {
        const string sql = """
            UPDATE universidad
               SET activo = 0
             WHERE id = @id AND activo = 1;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { id });
    }
}