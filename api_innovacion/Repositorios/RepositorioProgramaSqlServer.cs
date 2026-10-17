using ApiInnovacion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiInnovacion.Repositorios;

/// <summary>
/// Repositorio de `programa` (v2) sobre SQL Server con Dapper. CERO SQL en C#:
/// solo invoca los procedimientos con CommandType.StoredProcedure.
///
/// Los parametros se arman SIEMPRE con DynamicParameters y el nombre snake_case
/// exacto del parametro del procedimiento (@fecha_creacion, @fecha_cierre...):
/// Dapper no empareja esos nombres con las propiedades PascalCase de un objeto
/// anonimo, y ese es el gotcha que mas rompe (4_research.md D2). Los parametros
/// van por nombre, no por posicion.
/// </summary>
public class RepositorioProgramaSqlServer : IRepositorioPrograma
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<List<Programa>> ObtenerTodosAsync(int limite)
    {
        var parametros = new DynamicParameters();
        parametros.Add("limite", limite);

        await using var conexion = new SqlConnection(_cadenaConexion);
        var filas = await conexion.QueryAsync<Programa>(
            "sp_listar_programa", parametros, commandType: CommandType.StoredProcedure);
        return filas.AsList();
    }

    public async Task<Programa?> ObtenerPorIdAsync(int id)
    {
        var parametros = new DynamicParameters();
        parametros.Add("id", id);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleOrDefaultAsync<Programa>(
            "sp_consultar_programa", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<Programa> CrearAsync(Programa entidad)
    {
        var parametros = new DynamicParameters();
        // Sin "id": lo genera el procedimiento con la secuencia de la base y lo
        // devuelve en la fila guardada.
        parametros.Add("nombre", entidad.Nombre);
        parametros.Add("tipo", entidad.Tipo);
        parametros.Add("nivel", entidad.Nivel);
        parametros.Add("fecha_creacion", entidad.FechaCreacion);
        parametros.Add("fecha_cierre", entidad.FechaCierre);   // puede ir NULL
        parametros.Add("numero_cohortes", entidad.NumeroCohortes);
        parametros.Add("cant_graduados", entidad.CantGraduados);
        parametros.Add("fecha_actualizacion", entidad.FechaActualizacion);
        parametros.Add("ciudad", entidad.Ciudad);
        parametros.Add("facultad", entidad.Facultad);
        // num_acreditaciones NO viaja: lo mantiene el disparador.

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<Programa>(
            "sp_crear_programa", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ActualizarAsync(Programa entidad)
    {
        var parametros = new DynamicParameters();
        parametros.Add("id", entidad.Id);
        parametros.Add("nombre", entidad.Nombre);
        parametros.Add("tipo", entidad.Tipo);
        parametros.Add("nivel", entidad.Nivel);
        parametros.Add("fecha_creacion", entidad.FechaCreacion);
        parametros.Add("fecha_cierre", entidad.FechaCierre);
        parametros.Add("numero_cohortes", entidad.NumeroCohortes);
        parametros.Add("cant_graduados", entidad.CantGraduados);
        parametros.Add("fecha_actualizacion", entidad.FechaActualizacion);
        parametros.Add("ciudad", entidad.Ciudad);
        parametros.Add("facultad", entidad.Facultad);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_actualizar_programa", parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task<int> EliminarAsync(int id)
    {
        var parametros = new DynamicParameters();
        parametros.Add("id", id);

        await using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QuerySingleAsync<int>(
            "sp_eliminar_programa", parametros, commandType: CommandType.StoredProcedure);
    }
}
