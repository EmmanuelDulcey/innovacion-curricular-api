using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de `facultad` (v2) sobre SQL Server con Dapper. Todo el SQL vive
/// en los procedimientos almacenados (CommandType.StoredProcedure); aqui no hay
/// SQL embebido. Los parametros se arman con los nombres snake_case exactos que
/// espera el procedimiento.
/// Fuente: docs/spec_kit/versiones/v2/3_plan.md §4.5.
/// </summary>
public class RepositorioFacultadSqlServer : IRepositorioFacultad
{
    private readonly string _cadenaConexion;

    public RepositorioFacultadSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<Facultad>> ObtenerTodosAsync(int limite)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<Facultad>(
            "sp_listar_facultad",
            new { limite },
            commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<Facultad?> ObtenerPorIdAsync(int id)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<Facultad>(
            "sp_consultar_facultad",
            new { id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Facultad> CrearAsync(Facultad entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("nombre", entidad.Nombre);
        parametros.Add("tipo", entidad.Tipo);
        parametros.Add("fecha_fun", entidad.FechaFun);
        parametros.Add("universidad", entidad.Universidad);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<Facultad>(
            "sp_crear_facultad",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ActualizarAsync(Facultad entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("id", entidad.Id);
        parametros.Add("nombre", entidad.Nombre);
        parametros.Add("tipo", entidad.Tipo);
        parametros.Add("fecha_fun", entidad.FechaFun);
        parametros.Add("universidad", entidad.Universidad);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_actualizar_facultad",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> EliminarAsync(int id)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_eliminar_facultad",
            new { id },
            commandType: CommandType.StoredProcedure);
    }
}
