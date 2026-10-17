// =============================================================================
// QUÉ ES: repositorio SQL Server de docente_departamento.
// PAPEL: invoca procedimientos almacenados con Dapper; no contiene SQL directo.
// =============================================================================
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

public class RepositorioDocenteDepartamentoSqlServer : IRepositorioDocenteDepartamento
{
    private readonly string _cadenaConexion;

    public RepositorioDocenteDepartamentoSqlServer(string cadenaConexion) => _cadenaConexion = cadenaConexion;

    public async Task<List<DocenteDepartamento>> ObtenerTodosAsync(int limite)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<DocenteDepartamento>(
            "sp_listar_docente_departamento", new { limite }, commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<DocenteDepartamento?> ObtenerPorParejaAsync(int docente, int departamento)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<DocenteDepartamento>(
            "sp_consultar_docente_departamento", new { docente, departamento }, commandType: CommandType.StoredProcedure);
    }

    public async Task<DocenteDepartamento> CrearAsync(DocenteDepartamento entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("docente", entidad.Docente);
        parametros.Add("departamento", entidad.Departamento);
        parametros.Add("dedicacion", entidad.Dedicacion);
        parametros.Add("modalidad", entidad.Modalidad);
        parametros.Add("fecha_ingreso", entidad.FechaIngreso);
        parametros.Add("fecha_salida", entidad.FechaSalida);
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<DocenteDepartamento>(
            "sp_crear_docente_departamento", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ActualizarAsync(DocenteDepartamento entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("docente", entidad.Docente);
        parametros.Add("departamento", entidad.Departamento);
        parametros.Add("dedicacion", entidad.Dedicacion);
        parametros.Add("modalidad", entidad.Modalidad);
        parametros.Add("fecha_ingreso", entidad.FechaIngreso);
        parametros.Add("fecha_salida", entidad.FechaSalida);
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_actualizar_docente_departamento", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> RetirarAsync(int docente, int departamento)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_eliminar_docente_departamento", new { docente, departamento }, commandType: CommandType.StoredProcedure);
    }
}
