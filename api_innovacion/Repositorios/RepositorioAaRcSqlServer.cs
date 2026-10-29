using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de la puente `aa_rc` (v2): solo procedimientos (cero SQL).
/// Aqui el gotcha D2 muerde de verdad: los parametros
/// `@activ_academicas_idcurso` y `@registro_calificado_codigo` solo se pueden
/// armar con DynamicParameters (un objeto anonimo no admite nombres
/// snake_case); si se escribieran mal, llegarian NULL y el INSERT fallaria.
/// </summary>
public class RepositorioAaRcSqlServer : IRepositorioAaRc
{
    private readonly string _cadenaConexion;

    public RepositorioAaRcSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<AaRc>> ObtenerTodosAsync(int limite)
    {
        var parametros = new DynamicParameters();
        parametros.Add("limite", limite);

        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<AaRc>(
            "sp_listar_aa_rc", parametros, commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<AaRc?> ObtenerPorIdAsync(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        var parametros = new DynamicParameters();
        parametros.Add("activ_academicas_idcurso", activAcademicasIdcurso);
        parametros.Add("registro_calificado_codigo", registroCalificadoCodigo);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<AaRc>(
            "sp_consultar_aa_rc", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<AaRc> AsignarAsync(AaRc entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("activ_academicas_idcurso", entidad.ActivAcademicasIdcurso);
        parametros.Add("registro_calificado_codigo", entidad.RegistroCalificadoCodigo);
        parametros.Add("componente", entidad.Componente);
        parametros.Add("semestre", entidad.Semestre);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<AaRc>(
            "sp_asignar_aa_rc", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> RetirarAsync(int activAcademicasIdcurso, int registroCalificadoCodigo)
    {
        var parametros = new DynamicParameters();
        parametros.Add("activ_academicas_idcurso", activAcademicasIdcurso);
        parametros.Add("registro_calificado_codigo", registroCalificadoCodigo);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_retirar_aa_rc", parametros, commandType: CommandType.StoredProcedure);
    }
}
