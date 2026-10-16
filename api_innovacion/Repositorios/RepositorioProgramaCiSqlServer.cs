// =============================================================================
// QUÉ ES: repositorio SQL Server de programa_ci.
// PAPEL: invoca procedimientos almacenados con Dapper; no contiene SQL directo.
// =============================================================================
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

public class RepositorioProgramaCiSqlServer : IRepositorioProgramaCi
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaCiSqlServer(string cadenaConexion) => _cadenaConexion = cadenaConexion;

    public async Task<List<ProgramaCi>> ObtenerTodosAsync(int limite)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<ProgramaCi>(
            "sp_listar_programa_ci", new { limite }, commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<ProgramaCi?> ObtenerPorParejaAsync(int programa, int car_innovacion)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<ProgramaCi>(
            "sp_consultar_programa_ci", new { programa, car_innovacion }, commandType: CommandType.StoredProcedure);
    }

    public async Task<ProgramaCi> AsignarAsync(ProgramaCi entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("programa", entidad.Programa);
        parametros.Add("car_innovacion", entidad.CarInnovacion);
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<ProgramaCi>(
            "sp_asignar_programa_ci", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> RetirarAsync(int programa, int car_innovacion)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_retirar_programa_ci", new { programa, car_innovacion }, commandType: CommandType.StoredProcedure);
    }
}
