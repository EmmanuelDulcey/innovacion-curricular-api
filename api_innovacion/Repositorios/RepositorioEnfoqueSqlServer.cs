using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio SQL Server de `enfoque`: lecturas activas y borrado lógico.
/// </summary>
public class RepositorioEnfoqueSqlServer : IRepositorioEnfoque
{
    private readonly string _cadenaConexion;

    public RepositorioEnfoqueSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<Enfoque>> ObtenerTodosAsync(int limite)
    {
        const string sql = """
            SELECT TOP (@limite) id, nombre, descripcion, activo
              FROM enfoque
             WHERE activo = 1
             ORDER BY id;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<Enfoque>(sql, new { limite });
        return filas.AsList();
    }

    public async Task<Enfoque?> ObtenerPorIdAsync(int id)
    {
        const string sql = """
            SELECT id, nombre, descripcion, activo
              FROM enfoque
             WHERE id = @id AND activo = 1;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<Enfoque>(sql, new { id });
    }

    public async Task CrearAsync(Enfoque entidad)
    {
        const string sql = """
            INSERT INTO enfoque (id, nombre, descripcion, activo)
            VALUES (@id, @nombre, @descripcion, 1);
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.ExecuteAsync(sql, new
        {
            entidad.Id,
            entidad.Nombre,
            entidad.Descripcion
        });
    }

    public async Task<int> ActualizarAsync(int id, Dictionary<string, object> datos)
    {
        if (datos.Count == 0)
            return 0;

        var conjuntoSet = string.Join(", ", datos.Keys.Select(clave => $"{clave} = @{clave}"));
        var sql = $"""
            UPDATE enfoque
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
            UPDATE enfoque
               SET activo = 0
             WHERE id = @id AND activo = 1;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { id });
    }
}
