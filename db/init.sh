#!/usr/bin/env bash
# =============================================================================
# init.sh
# Inicializa la base de datos `innovacion_curricular`. Se ejecuta desde el
# contenedor `sqlserver-init` (docker-compose).
#
# Orden (fuente: 3_plan.md, Secciones 4.7 y 11):
#   1. innovacion_curricular.ss.sql          -> crea las 25 tablas de la base
#   2. 01_activar_borrado_logico.sql         -> v1: `activo` en las 7 tablas sin FK
#   3. 02_datos_iniciales.sql                -> v1: carga 218 areas y 6 universidades
#   4. 03_activar_borrado_logico_v2.sql      -> v2: `activo` en las 15 tablas con FK
#   5. 04_procedimientos_v2_*.sql            -> v2: procedimientos + disparador
#
# Los pasos 1-3 son la base de la v1: sólo corren cuando la base NO existe, para
# no volver a sembrar (no se borran cambios hechos por la API). Los pasos 4-5 son
# idempotentes y NO destructivos, así que se aplican SIEMPRE: migran una base de
# la v1 a la v2 y son no-ops cuando ya están aplicados.
# =============================================================================
set -e

SQLSERVER_HOST="${SQLSERVER_HOST:-sqlserver}"
SQLCMD=(/opt/mssql-tools18/bin/sqlcmd -S "$SQLSERVER_HOST" -U sa -P "$MSSQL_SA_PASSWORD" -C -b)

echo "[init] Esperando a que SQL Server este listo..."
for i in {1..60}; do
  if "${SQLCMD[@]}" -d master -Q "SELECT 1" > /dev/null 2>&1; then
    echo "[init] SQL Server listo (intento $i)."
    break
  fi
  if [ "$i" -eq 60 ]; then
    echo "[init] ERROR: SQL Server no respondio a tiempo."
    exit 1
  fi
  sleep 2
done

run_script() {
  echo "[init] Ejecutando: $1"
  "${SQLCMD[@]}" -i "$1"
}

estado="$(${SQLCMD[@]} -d master -h -1 -W -Q "SET NOCOUNT ON;
IF DB_ID(N'innovacion_curricular') IS NULL
    SELECT 'missing';
ELSE IF
    (SELECT COUNT(*) FROM innovacion_curricular.sys.tables WHERE is_ms_shipped = 0) = 25
    AND (SELECT COUNT(*)
         FROM innovacion_curricular.sys.columns
         WHERE name = 'activo'
           AND object_id IN (
             OBJECT_ID(N'innovacion_curricular.dbo.area_conocimiento'),
             OBJECT_ID(N'innovacion_curricular.dbo.universidad'),
             OBJECT_ID(N'innovacion_curricular.dbo.aspecto_normativo'),
             OBJECT_ID(N'innovacion_curricular.dbo.practica_estrategia'),
             OBJECT_ID(N'innovacion_curricular.dbo.enfoque'),
             OBJECT_ID(N'innovacion_curricular.dbo.car_innovacion'),
             OBJECT_ID(N'innovacion_curricular.dbo.aliado')
           )) = 7
    SELECT 'ready';
ELSE
    SELECT 'partial';" | tr -d '[:space:]')"

case "$estado" in
  missing)
    echo "[init] Base inexistente: creando esquema y datos iniciales."
    run_script /scripts/innovacion_curricular.ss.sql
    run_script /scripts/01_activar_borrado_logico.sql
    run_script /scripts/02_datos_iniciales.sql
    ;;
  ready)
    echo "[init] Base v1 ya inicializada; se conservan sus datos."
    ;;
  partial)
    echo "[init] ERROR: la base existe pero su esquema esta incompleto."
    echo "[init] No se ejecutan scripts destructivos; revisa la base manualmente."
    exit 1
    ;;
  *)
    echo "[init] ERROR: no se pudo determinar el estado de la base ($estado)."
    exit 1
    ;;
esac

# -----------------------------------------------------------------------------
# v2: scripts idempotentes y no destructivos. Se aplican SIEMPRE para migrar
# una base de la v1 y para dejar lista una recien creada.
#   * 03_activar_borrado_logico_v2.sql -> `activo` en las 15 tablas + contador
#   * 04_procedimientos_v2_*.sql       -> procedimientos + disparador
# -----------------------------------------------------------------------------
echo "[init] Aplicando los scripts de la v2 (idempotentes)."
run_script /scripts/03_activar_borrado_logico_v2.sql

shopt -s nullglob
for script in /scripts/04_*.sql; do
  run_script "$script"
done

echo "[init] Inicializacion completada."
