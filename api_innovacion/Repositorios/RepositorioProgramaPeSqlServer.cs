// =============================================================================
// QUÉ ES: repositorio SQL Server de programa_pe.
// PAPEL: invoca procedimientos almacenados con Dapper; no contiene SQL directo.
// =============================================================================
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

public class RepositorioProgramaPeSqlServer : IRepositorioProgramaPe
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaPeSqlServer(string cadenaConexion) => _cadenaConexion = cadenaConexion;

    public async Task<List<ProgramaPe>> ObtenerTodosAsync(int limite)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<ProgramaPe>(
            "sp_listar_programa_pe", new { limite }, commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<ProgramaPe?> ObtenerPorParejaAsync(int programa, int practica_estrategia)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<ProgramaPe>(
            "sp_consultar_programa_pe", new { programa, practica_estrategia }, commandType: CommandType.StoredProcedure);
    }

    public async Task<ProgramaPe> AsignarAsync(ProgramaPe entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("programa", entidad.Programa);
        parametros.Add("practica_estrategia", entidad.PracticaEstrategia);
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<ProgramaPe>(
            "sp_asignar_programa_pe", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> RetirarAsync(int programa, int practica_estrategia)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_retirar_programa_pe", new { programa, practica_estrategia }, commandType: CommandType.StoredProcedure);
    }
}
