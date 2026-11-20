using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de usuarios sobre SQL Server. Todo el SQL vive en los
/// procedimientos `sp_consultar_usuario_por_username` y
/// `sp_listar_roles_usuario` (db/09_seguridad_v3.sql); aquí no hay SQL
/// embebido. Dapper mapea snake_case a PascalCase por la configuración global
/// de Program.cs.
/// </summary>
public class RepositorioUsuarioSqlServer : IRepositorioUsuario
{
    private readonly string _cadenaConexion;

    public RepositorioUsuarioSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<Usuario?> ObtenerPorUsernameAsync(string username)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<Usuario>(
            "sp_consultar_usuario_por_username",
            new { username },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<List<string>> ObtenerRolesAsync(int usuarioId)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        var parametros = new DynamicParameters();
        parametros.Add("usuario_id", usuarioId);

        var roles = await conexion.QueryAsync<string>(
            "sp_listar_roles_usuario",
            parametros,
            commandType: CommandType.StoredProcedure);

        return roles.AsList();
    }
}
