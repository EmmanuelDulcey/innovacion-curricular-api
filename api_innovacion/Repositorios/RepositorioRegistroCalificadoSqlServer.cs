using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de `registro_calificado` (v2): solo procedimientos (cero SQL).
/// Las fechas DATE viajan como DateTime en ambos sentidos; SqlClient y el
/// parametro DATE del procedimiento se encargan de la conversion.
/// </summary>
public class RepositorioRegistroCalificadoSqlServer : IRepositorioRegistroCalificado
{
    private readonly string _cadenaConexion;

    public RepositorioRegistroCalificadoSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<RegistroCalificado>> ObtenerTodosAsync(int limite)
    {
        var parametros = new DynamicParameters();
        parametros.Add("limite", limite);

        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<RegistroCalificado>(
            "sp_listar_registro_calificado", parametros, commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<RegistroCalificado?> ObtenerPorIdAsync(int codigo)
    {
        var parametros = new DynamicParameters();
        parametros.Add("codigo", codigo);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<RegistroCalificado>(
            "sp_consultar_registro_calificado", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<RegistroCalificado> CrearAsync(RegistroCalificado entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("codigo", entidad.Codigo);
        parametros.Add("cant_creditos", entidad.CantCreditos);
        parametros.Add("hora_acom", entidad.HoraAcom);
        parametros.Add("hora_ind", entidad.HoraInd);
        parametros.Add("metodologia", entidad.Metodologia);
        parametros.Add("fecha_inicio", entidad.FechaInicio);    // DATE
        parametros.Add("fecha_fin", entidad.FechaFin);          // DATE
        parametros.Add("duracion_anios", entidad.DuracionAnios);
        parametros.Add("duracion_semestres", entidad.DuracionSemestres);
        parametros.Add("tipo_titulacion", entidad.TipoTitulacion);
        parametros.Add("programa", entidad.Programa);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<RegistroCalificado>(
            "sp_crear_registro_calificado", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ActualizarAsync(RegistroCalificado entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("codigo", entidad.Codigo);
        parametros.Add("cant_creditos", entidad.CantCreditos);
        parametros.Add("hora_acom", entidad.HoraAcom);
        parametros.Add("hora_ind", entidad.HoraInd);
        parametros.Add("metodologia", entidad.Metodologia);
        parametros.Add("fecha_inicio", entidad.FechaInicio);
        parametros.Add("fecha_fin", entidad.FechaFin);
        parametros.Add("duracion_anios", entidad.DuracionAnios);
        parametros.Add("duracion_semestres", entidad.DuracionSemestres);
        parametros.Add("tipo_titulacion", entidad.TipoTitulacion);
        parametros.Add("programa", entidad.Programa);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_actualizar_registro_calificado", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> EliminarAsync(int codigo)
    {
        var parametros = new DynamicParameters();
        parametros.Add("codigo", codigo);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_eliminar_registro_calificado", parametros, commandType: CommandType.StoredProcedure);
    }
}
