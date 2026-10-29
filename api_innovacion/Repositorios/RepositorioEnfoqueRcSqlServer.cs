using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de la puente `enfoque_rc` (v2): solo procedimientos (cero SQL).
/// Los dos parametros de la PK se pasan por nombre (`enfoque`,
/// `registro_calificado`), sin depender del orden (gotcha de Dapper, D2).
/// </summary>
public class RepositorioEnfoqueRcSqlServer : IRepositorioEnfoqueRc
{
    private readonly string _cadenaConexion;

    public RepositorioEnfoqueRcSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<EnfoqueRc>> ObtenerTodosAsync(int limite)
    {
        var parametros = new DynamicParameters();
        parametros.Add("limite", limite);

        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<EnfoqueRc>(
            "sp_listar_enfoque_rc", parametros, commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<EnfoqueRc?> ObtenerPorIdAsync(int enfoque, int registroCalificado)
    {
        var parametros = new DynamicParameters();
        parametros.Add("enfoque", enfoque);
        parametros.Add("registro_calificado", registroCalificado);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<EnfoqueRc>(
            "sp_consultar_enfoque_rc", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<EnfoqueRc> AsignarAsync(EnfoqueRc entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("enfoque", entidad.Enfoque);
        parametros.Add("registro_calificado", entidad.RegistroCalificado);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<EnfoqueRc>(
            "sp_asignar_enfoque_rc", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> RetirarAsync(int enfoque, int registroCalificado)
    {
        var parametros = new DynamicParameters();
        parametros.Add("enfoque", enfoque);
        parametros.Add("registro_calificado", registroCalificado);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_retirar_enfoque_rc", parametros, commandType: CommandType.StoredProcedure);
    }
}
