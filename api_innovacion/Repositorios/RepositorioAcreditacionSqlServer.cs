using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de `acreditacion` (v2): solo invoca los procedimientos de la
/// tabla (cero SQL en C#). Aqui el nombre del parametro de la PK es
/// `resolucion`, no `id`: el contrato §0.2 fija esa ruta y el procedimiento
/// declara ese parametro.
/// </summary>
public class RepositorioAcreditacionSqlServer : IRepositorioAcreditacion
{
    private readonly string _cadenaConexion;

    public RepositorioAcreditacionSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<Acreditacion>> ObtenerTodosAsync(int limite)
    {
        var parametros = new DynamicParameters();
        parametros.Add("limite", limite);

        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<Acreditacion>(
            "sp_listar_acreditacion", parametros, commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<Acreditacion?> ObtenerPorIdAsync(int resolucion)
    {
        var parametros = new DynamicParameters();
        parametros.Add("resolucion", resolucion);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<Acreditacion>(
            "sp_consultar_acreditacion", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<Acreditacion> CrearAsync(Acreditacion entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("resolucion", entidad.Resolucion);
        parametros.Add("tipo", entidad.Tipo);
        parametros.Add("calificacion", entidad.Calificacion);
        parametros.Add("fecha_inicio", entidad.FechaInicio);
        parametros.Add("fecha_fin", entidad.FechaFin);
        parametros.Add("programa", entidad.Programa);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<Acreditacion>(
            "sp_crear_acreditacion", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ActualizarAsync(Acreditacion entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("resolucion", entidad.Resolucion);
        parametros.Add("tipo", entidad.Tipo);
        parametros.Add("calificacion", entidad.Calificacion);
        parametros.Add("fecha_inicio", entidad.FechaInicio);
        parametros.Add("fecha_fin", entidad.FechaFin);
        parametros.Add("programa", entidad.Programa);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_actualizar_acreditacion", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> EliminarAsync(int resolucion)
    {
        var parametros = new DynamicParameters();
        parametros.Add("resolucion", resolucion);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_eliminar_acreditacion", parametros, commandType: CommandType.StoredProcedure);
    }
}
