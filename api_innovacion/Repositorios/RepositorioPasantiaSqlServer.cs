using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de `pasantia` (v2) sobre SQL Server con Dapper. Todo el SQL vive
/// en los procedimientos almacenados (CommandType.StoredProcedure).
/// Fuente: docs/spec_kit/versiones/v2/3_plan.md §4.5.
/// </summary>
public class RepositorioPasantiaSqlServer : IRepositorioPasantia
{
    private readonly string _cadenaConexion;

    public RepositorioPasantiaSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<Pasantia>> ObtenerTodosAsync(int limite)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<Pasantia>(
            "sp_listar_pasantia",
            new { limite },
            commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<Pasantia?> ObtenerPorIdAsync(int id)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<Pasantia>(
            "sp_consultar_pasantia",
            new { id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Pasantia> CrearAsync(Pasantia entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("nombre", entidad.Nombre);
        parametros.Add("pais", entidad.Pais);
        parametros.Add("empresa", entidad.Empresa);
        parametros.Add("descripcion", entidad.Descripcion);
        parametros.Add("programa", entidad.Programa);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<Pasantia>(
            "sp_crear_pasantia",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ActualizarAsync(Pasantia entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("id", entidad.Id);
        parametros.Add("nombre", entidad.Nombre);
        parametros.Add("pais", entidad.Pais);
        parametros.Add("empresa", entidad.Empresa);
        parametros.Add("descripcion", entidad.Descripcion);
        parametros.Add("programa", entidad.Programa);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_actualizar_pasantia",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> EliminarAsync(int id)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_eliminar_pasantia",
            new { id },
            commandType: CommandType.StoredProcedure);
    }
}
