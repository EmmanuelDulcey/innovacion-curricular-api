using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de la lista negra de tokens sobre SQL Server. Todo el SQL vive
/// en `sp_revocar_token` y `sp_token_revocado` (db/10_revocacion_tokens_v3.sql);
/// aquí no hay SQL embebido.
/// </summary>
public class RepositorioTokenRevocadoSqlServer : IRepositorioTokenRevocado
{
    private readonly string _cadenaConexion;

    public RepositorioTokenRevocadoSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task RevocarAsync(string jti)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.ExecuteAsync(
            "sp_revocar_token",
            new { jti },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> EstaRevocadoAsync(string jti)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<bool>(
            "sp_token_revocado",
            new { jti },
            commandType: CommandType.StoredProcedure);
    }
}
