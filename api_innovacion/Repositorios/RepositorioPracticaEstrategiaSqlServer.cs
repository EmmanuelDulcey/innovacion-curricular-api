using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

public class RepositorioPracticaEstrategiaSqlServer : IRepositorioPracticaEstrategia
{
    private readonly string _cadenaConexion;

    public RepositorioPracticaEstrategiaSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<PracticaEstrategia>> ObtenerTodosAsync(int limite)
    {
        const string sql = """
            SELECT TOP (@limite) id, tipo, nombre, descripcion, activo
              FROM practica_estrategia
             WHERE activo = 1
             ORDER BY id;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<PracticaEstrategia>(sql, new { limite });
        return filas.AsList();
    }

    public async Task<PracticaEstrategia?> ObtenerPorIdAsync(int id)
    {
        const string sql = """
            SELECT id, tipo, nombre, descripcion, activo
              FROM practica_estrategia
             WHERE id = @id AND activo = 1;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<PracticaEstrategia>(sql, new { id });
    }

    public async Task CrearAsync(PracticaEstrategia entidad)
    {
        const string sql = """
            INSERT INTO practica_estrategia (id, tipo, nombre, descripcion, activo)
            VALUES (@id, @tipo, @nombre, @descripcion, 1);
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.ExecuteAsync(sql, new
        {
            entidad.Id,
            entidad.Tipo,
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
            UPDATE practica_estrategia
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
            UPDATE practica_estrategia
               SET activo = 0
             WHERE id = @id AND activo = 1;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { id });
    }
}
