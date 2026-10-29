// =============================================================================
// QUÉ ES: traductor de errores conocidos de SQL Server para relaciones de v2.
// PAPEL: el servicio distingue conflictos de integridad de fallos inesperados.
// =============================================================================
using ApiInnovacion.Excepciones;
using Microsoft.Data.SqlClient;

namespace ApiInnovacion.Servicios;

internal static class TraductorSqlRelacion
{
    public static bool EsConflicto(SqlException ex) => ex.Number is 547 or 2601 or 2627 or 51000;

    // 51000 procede del procedimiento cuando el padre existe físicamente pero
    // está inactivo; una FK por sí sola no detecta el borrado lógico.
    public static ConflictoExcepcion Convertir(SqlException ex) => ex.Number switch
    {
        51000 => new ConflictoExcepcion(ex.Message),
        547 => new ConflictoExcepcion("Uno de los registros relacionados no existe."),
        2601 or 2627 => new ConflictoExcepcion("Esa relación ya está registrada."),
        _ => throw new ArgumentOutOfRangeException(nameof(ex), "No es un conflicto conocido.")
    };
}
