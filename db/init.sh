#!/usr/bin/env bash
# =============================================================================
# init.sh
# Inicializa la base de datos `innovacion_curricular` ejecutando, en orden,
# los tres scripts provistos para la v1. Se ejecuta UNA sola vez por volumen,
# desde el contenedor `sqlserver-init` (docker-compose).
#
# Orden (fuente: 3_plan.md, Seccion 4.7):
#   1. innovacion_curricular.ss.sql     -> crea las 25 tablas de la base
#   2. 01_activar_borrado_logico.sql    -> agrega la columna `activo` (BIT)
#   3. 02_datos_iniciales.sql           -> carga 218 areas y 6 universidades
#
# La primera ejecución crea el esquema y carga los datos base. Las ejecuciones
# posteriores comprueban el esquema y no vuelven a sembrar datos: así no se
# borran cambios realizados por la API.
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
    echo "[init] Base ya inicializada; se conservan sus datos."
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

echo "[init] Inicializacion completada."
