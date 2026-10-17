// =============================================================================
// QUÉ ES: repositorio SQL Server del catálogo de docentes.
// PAPEL: delega todas las operaciones a procedimientos almacenados.
// =============================================================================
using System.Data;
using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Repositorios;

public class RepositorioDocenteSqlServer : IRepositorioDocente
{
    private readonly string _cadenaConexion;

    public RepositorioDocenteSqlServer(string cadenaConexion) => _cadenaConexion = cadenaConexion;

    public async Task<List<Docente>> ObtenerTodosAsync(int limite)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<Docente>(
            "sp_listar_docente", new { limite }, commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<Docente?> ObtenerPorIdAsync(int id)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<Docente>(
            "sp_consultar_docente", new { id }, commandType: CommandType.StoredProcedure);
    }

    public async Task<Docente> CrearAsync(Docente entidad)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<Docente>(
            "sp_crear_docente", new { entidad.Documento, entidad.Nombre },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ActualizarAsync(Docente entidad)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_actualizar_docente", new { entidad.Id, entidad.Documento, entidad.Nombre },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> EliminarAsync(int id)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_eliminar_docente", new { id }, commandType: CommandType.StoredProcedure);
    }
}
