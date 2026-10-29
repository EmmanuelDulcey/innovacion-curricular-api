// =============================================================================
// QUÉ ES: repositorio SQL Server de alianza.
// PAPEL: invoca procedimientos almacenados con Dapper; no contiene SQL directo.
// =============================================================================
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

public class RepositorioAlianzaSqlServer : IRepositorioAlianza
{
    private readonly string _cadenaConexion;

    public RepositorioAlianzaSqlServer(string cadenaConexion) => _cadenaConexion = cadenaConexion;

    public async Task<List<Alianza>> ObtenerTodosAsync(int limite)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<Alianza>(
            "sp_listar_alianza", new { limite }, commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<Alianza?> ObtenerPorParejaAsync(int aliado, int departamento)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<Alianza>(
            "sp_consultar_alianza", new { aliado, departamento }, commandType: CommandType.StoredProcedure);
    }

    public async Task<Alianza> CrearAsync(Alianza entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("aliado", entidad.Aliado);
        parametros.Add("departamento", entidad.Departamento);
        parametros.Add("fecha_inicio", entidad.FechaInicio);
        parametros.Add("fecha_fin", entidad.FechaFin);
        parametros.Add("docente", entidad.Docente);
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<Alianza>(
            "sp_crear_alianza", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ActualizarAsync(Alianza entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("aliado", entidad.Aliado);
        parametros.Add("departamento", entidad.Departamento);
        parametros.Add("fecha_inicio", entidad.FechaInicio);
        parametros.Add("fecha_fin", entidad.FechaFin);
        parametros.Add("docente", entidad.Docente);
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_actualizar_alianza", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> RetirarAsync(int aliado, int departamento)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_eliminar_alianza", new { aliado, departamento }, commandType: CommandType.StoredProcedure);
    }
}
