using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de `activ_academica` (v2): solo procedimientos (cero SQL).
/// El parametro `disenio` se agrega tambien cuando el valor es null: Dapper
/// envia DBNull y el procedimiento lo guarda como NULL (la FK es opcional).
/// </summary>
public class RepositorioActivAcademicaSqlServer : IRepositorioActivAcademica
{
    private readonly string _cadenaConexion;

    public RepositorioActivAcademicaSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<ActivAcademica>> ObtenerTodosAsync(int limite)
    {
        var parametros = new DynamicParameters();
        parametros.Add("limite", limite);

        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<ActivAcademica>(
            "sp_listar_activ_academica", parametros, commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<ActivAcademica?> ObtenerPorIdAsync(int id)
    {
        var parametros = new DynamicParameters();
        parametros.Add("id", id);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<ActivAcademica>(
            "sp_consultar_activ_academica", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<ActivAcademica> CrearAsync(ActivAcademica entidad)
    {
        var parametros = new DynamicParameters();
        // Sin "id": lo genera el procedimiento con la secuencia de la base.
        parametros.Add("nombre", entidad.Nombre);
        parametros.Add("num_creditos", entidad.NumCreditos);
        parametros.Add("tipo", entidad.Tipo);
        parametros.Add("area_formacion", entidad.AreaFormacion);
        parametros.Add("h_acom", entidad.HAcom);
        parametros.Add("h_indep", entidad.HIndep);
        parametros.Add("idioma", entidad.Idioma);
        parametros.Add("espejo", entidad.Espejo);               // TINYINT 0/1
        parametros.Add("entidad_espejo", entidad.EntidadEspejo);
        parametros.Add("pais_espejo", entidad.PaisEspejo);
        parametros.Add("disenio", entidad.Disenio);             // puede ir NULL

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<ActivAcademica>(
            "sp_crear_activ_academica", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ActualizarAsync(ActivAcademica entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("id", entidad.Id);
        parametros.Add("nombre", entidad.Nombre);
        parametros.Add("num_creditos", entidad.NumCreditos);
        parametros.Add("tipo", entidad.Tipo);
        parametros.Add("area_formacion", entidad.AreaFormacion);
        parametros.Add("h_acom", entidad.HAcom);
        parametros.Add("h_indep", entidad.HIndep);
        parametros.Add("idioma", entidad.Idioma);
        parametros.Add("espejo", entidad.Espejo);
        parametros.Add("entidad_espejo", entidad.EntidadEspejo);
        parametros.Add("pais_espejo", entidad.PaisEspejo);
        parametros.Add("disenio", entidad.Disenio);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_actualizar_activ_academica", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> EliminarAsync(int id)
    {
        var parametros = new DynamicParameters();
        parametros.Add("id", id);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_eliminar_activ_academica", parametros, commandType: CommandType.StoredProcedure);
    }
}
