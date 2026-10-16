// =============================================================================
// QUÉ ES: repositorio SQL Server de an_programa.
// PAPEL: invoca procedimientos almacenados con Dapper; no contiene SQL directo.
// =============================================================================
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

public class RepositorioAnProgramaSqlServer : IRepositorioAnPrograma
{
    private readonly string _cadenaConexion;

    public RepositorioAnProgramaSqlServer(string cadenaConexion) => _cadenaConexion = cadenaConexion;

    public async Task<List<AnPrograma>> ObtenerTodosAsync(int limite)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<AnPrograma>(
            "sp_listar_an_programa", new { limite }, commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<AnPrograma?> ObtenerPorParejaAsync(int aspecto_normativo, int programa)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<AnPrograma>(
            "sp_consultar_an_programa", new { aspecto_normativo, programa }, commandType: CommandType.StoredProcedure);
    }

    public async Task<AnPrograma> AsignarAsync(AnPrograma entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("aspecto_normativo", entidad.AspectoNormativo);
        parametros.Add("programa", entidad.Programa);
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<AnPrograma>(
            "sp_asignar_an_programa", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> RetirarAsync(int aspecto_normativo, int programa)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_retirar_an_programa", new { aspecto_normativo, programa }, commandType: CommandType.StoredProcedure);
    }
}
