// =============================================================================
// QUÉ ES: repositorio SQL Server de programa_ac.
// PAPEL: invoca procedimientos almacenados con Dapper; no contiene SQL directo.
// =============================================================================
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

public class RepositorioProgramaAcSqlServer : IRepositorioProgramaAc
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaAcSqlServer(string cadenaConexion) => _cadenaConexion = cadenaConexion;

    public async Task<List<ProgramaAc>> ObtenerTodosAsync(int limite)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<ProgramaAc>(
            "sp_listar_programa_ac", new { limite }, commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<ProgramaAc?> ObtenerPorParejaAsync(int programa, int area_conocimiento)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<ProgramaAc>(
            "sp_consultar_programa_ac", new { programa, area_conocimiento }, commandType: CommandType.StoredProcedure);
    }

    public async Task<ProgramaAc> AsignarAsync(ProgramaAc entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("programa", entidad.Programa);
        parametros.Add("area_conocimiento", entidad.AreaConocimiento);
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<ProgramaAc>(
            "sp_asignar_programa_ac", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> RetirarAsync(int programa, int area_conocimiento)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_retirar_programa_ac", new { programa, area_conocimiento }, commandType: CommandType.StoredProcedure);
    }
}
