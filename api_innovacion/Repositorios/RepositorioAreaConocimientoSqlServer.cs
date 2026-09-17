using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de `area_conocimiento` sobre SQL Server con Dapper.
/// SQL escrito a mano y parametrizado (@parametro). Todas las lecturas filtran
/// activo = 1; la eliminacion es un UPDATE de borrado logico, nunca DELETE físico.
/// Fuente: docs/spec_kit/versiones/v1_producto_sqlserver/3_plan.md, Seccion 4.5.
/// </summary>
public class RepositorioAreaConocimientoSqlServer : IRepositorioAreaConocimiento
{
    private readonly string _cadenaConexion;

    public RepositorioAreaConocimientoSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<AreaConocimiento>> ObtenerTodosAsync(int limite)
    {
        const string sql = """
            SELECT TOP (@limite) id, gran_area, area, disciplina, activo
              FROM area_conocimiento
             WHERE activo = 1
             ORDER BY id;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<AreaConocimiento>(sql, new { limite });
        return filas.AsList();
    }

    public async Task<AreaConocimiento?> ObtenerPorIdAsync(int id)
    {
        const string sql = """
            SELECT id, gran_area, area, disciplina, activo
              FROM area_conocimiento
             WHERE id = @id AND activo = 1;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<AreaConocimiento>(sql, new { id });
    }

    public async Task CrearAsync(AreaConocimiento entidad)
    {
        const string sql = """
            INSERT INTO area_conocimiento (id, gran_area, area, disciplina, activo)
            VALUES (@id, @gran_area, @area, @disciplina, 1);
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.ExecuteAsync(sql, new
        {
            entidad.Id,
            gran_area = entidad.GranArea,
            entidad.Area,
            entidad.Disciplina
        });
    }

    public async Task<int> ActualizarAsync(int id, Dictionary<string, object> datos)
    {
        // PUT y PATCH llegan aqui: el diccionario ya contiene solo las columnas a
        // actualizar con sus claves en snake_case (las mapea el servicio).
        if (datos.Count == 0)
            return 0;

        var conjuntoSet = string.Join(", ", datos.Keys.Select(clave => $"{clave} = @{clave}"));
        var sql = $"""
            UPDATE area_conocimiento
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
            UPDATE area_conocimiento
               SET activo = 0
             WHERE id = @id AND activo = 1;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { id });
    }
}