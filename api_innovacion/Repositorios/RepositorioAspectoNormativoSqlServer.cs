using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

public class RepositorioAspectoNormativoSqlServer : IRepositorioAspectoNormativo
{
    private readonly string _cadenaConexion;

    public RepositorioAspectoNormativoSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<AspectoNormativo>> ObtenerTodosAsync(int limite)
    {
        const string sql = """
            SELECT TOP (@limite) id, tipo, descripcion, fuente, activo
              FROM aspecto_normativo
             WHERE activo = 1
             ORDER BY id;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<AspectoNormativo>(sql, new { limite });
        return filas.AsList();
    }

    public async Task<AspectoNormativo?> ObtenerPorIdAsync(int id)
    {
        const string sql = """
            SELECT id, tipo, descripcion, fuente, activo
              FROM aspecto_normativo
             WHERE id = @id AND activo = 1;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<AspectoNormativo>(sql, new { id });
    }

    public async Task CrearAsync(AspectoNormativo entidad)
    {
        const string sql = """
            INSERT INTO aspecto_normativo (id, tipo, descripcion, fuente, activo)
            VALUES (@id, @tipo, @descripcion, @fuente, 1);
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.ExecuteAsync(sql, new
        {
            entidad.Id,
            entidad.Tipo,
            entidad.Descripcion,
            entidad.Fuente
        });
    }

    public async Task<int> ActualizarAsync(int id, Dictionary<string, object> datos)
    {
        if (datos.Count == 0)
            return 0;

        var conjuntoSet = string.Join(", ", datos.Keys.Select(clave => $"{clave} = @{clave}"));
        var sql = $"""
            UPDATE aspecto_normativo
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
            UPDATE aspecto_normativo
               SET activo = 0
             WHERE id = @id AND activo = 1;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { id });
    }
}
