using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de `car_innovacion` sobre SQL Server con Dapper.
/// SQL escrito a mano y parametrizado (@parametro). Todas las lecturas filtran
/// activo = 1; la eliminacion es un UPDATE de borrado logico, nunca un DELETE
/// fisico (1_constitution.md, Articulo 3; 3_plan.md, Seccion 4.5).
/// </summary>
public class RepositorioCarInnovacionSqlServer : IRepositorioCarInnovacion
{
    private readonly string _cadenaConexion;

    public RepositorioCarInnovacionSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<CarInnovacion>> ObtenerTodosAsync(int limite)
    {
        const string sql = """
            SELECT TOP (@limite) id, nombre, descripcion, tipo, activo
              FROM car_innovacion
             WHERE activo = 1
             ORDER BY id;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<CarInnovacion>(sql, new { limite });
        return filas.AsList();
    }

    public async Task<CarInnovacion?> ObtenerPorIdAsync(int id)
    {
        const string sql = """
            SELECT id, nombre, descripcion, tipo, activo
              FROM car_innovacion
             WHERE id = @id AND activo = 1;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<CarInnovacion>(sql, new { id });
    }

    public async Task CrearAsync(CarInnovacion entidad)
    {
        const string sql = """
            INSERT INTO car_innovacion (id, nombre, descripcion, tipo, activo)
            VALUES (@id, @nombre, @descripcion, @tipo, 1);
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.ExecuteAsync(sql, new
        {
            entidad.Id,
            entidad.Nombre,
            entidad.Descripcion,
            entidad.Tipo
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
            UPDATE car_innovacion
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
            UPDATE car_innovacion
               SET activo = 0
             WHERE id = @id AND activo = 1;
            """;

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteAsync(sql, new { id });
    }
}
