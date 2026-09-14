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
# Los tres pasos son idempotentes: repetirlos no falla ni duplica datos.
# =============================================================================
set -e

echo "[init] Esperando a que SQL Server este listo..."
for i in {1..60}; do
  if /opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" > /dev/null 2>&1; then
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
  /opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U sa -P "$MSSQL_SA_PASSWORD" -C -i "$1"
}

run_script /scripts/innovacion_curricular.ss.sql
run_script /scripts/01_activar_borrado_logico.sql
run_script /scripts/02_datos_iniciales.sql

echo "[init] Inicializacion completada."