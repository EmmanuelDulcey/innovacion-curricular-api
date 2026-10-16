using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de `premio` (v2) sobre SQL Server con Dapper. Todo el SQL vive
/// en los procedimientos almacenados (CommandType.StoredProcedure).
/// Fuente: docs/spec_kit/versiones/v2/3_plan.md §4.5.
/// </summary>
public class RepositorioPremioSqlServer : IRepositorioPremio
{
    private readonly string _cadenaConexion;

    public RepositorioPremioSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<Premio>> ObtenerTodosAsync(int limite)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<Premio>(
            "sp_listar_premio",
            new { limite },
            commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<Premio?> ObtenerPorIdAsync(int id)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<Premio>(
            "sp_consultar_premio",
            new { id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Premio> CrearAsync(Premio entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("nombre", entidad.Nombre);
        parametros.Add("descripcion", entidad.Descripcion);
        parametros.Add("fecha", entidad.Fecha);
        parametros.Add("entidad_otorgante", entidad.EntidadOtorgante);
        parametros.Add("pais", entidad.Pais);
        parametros.Add("programa", entidad.Programa);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<Premio>(
            "sp_crear_premio",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ActualizarAsync(Premio entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("id", entidad.Id);
        parametros.Add("nombre", entidad.Nombre);
        parametros.Add("descripcion", entidad.Descripcion);
        parametros.Add("fecha", entidad.Fecha);
        parametros.Add("entidad_otorgante", entidad.EntidadOtorgante);
        parametros.Add("pais", entidad.Pais);
        parametros.Add("programa", entidad.Programa);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_actualizar_premio",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> EliminarAsync(int id)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_eliminar_premio",
            new { id },
            commandType: CommandType.StoredProcedure);
    }
}
