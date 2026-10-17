// =============================================================================
// QUÉ ES: repositorio de opciones de programa.
// PAPEL: invoca exclusivamente el procedimiento de lectura con Dapper.
// =============================================================================
using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

public class RepositorioProgramaOpcionesSqlServer : IRepositorioProgramaOpciones
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaOpcionesSqlServer(string cadenaConexion) => _cadenaConexion = cadenaConexion;

    public async Task<List<ProgramaOpcion>> ObtenerTodosAsync(int limite)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<ProgramaOpcion>(
            "sp_listar_opciones_programa", new { limite }, commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }
}
