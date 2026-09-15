# Modelo de datos — Versión 1: la base completa (dada) y las 7 tablas sin FK

> **Versión 1** · La base de datos NO se diseña en esta versión: **viene
> dada** ([4_research.md](4_research.md), D4). Este documento describe lo
> que hay y lo único que esta versión puede tocar.

---

## 1. La base de datos `innovacion_curricular` (dada, completa)

El script **provisto** `db/innovacion_curricular.ss.sql` (dialecto SQL
Server) crea la base `innovacion_curricular` completa. El contenedor
`sqlserver-init` lo ejecuta automáticamente la primera vez.

**25 tablas** en dos módulos:

```
INNOVACIÓN CURRICULAR (22 tablas)                    USUARIOS (3 tablas)
─────────────────────────────────                    ────────────────────
Sin llave foránea (7, de esta versión):               usuario ──┐
  area_conocimiento · universidad                                ├── rol_usuario ── rol
  aspecto_normativo · practica_estrategia                        │
  enfoque · car_innovacion · aliado

Con llave foránea (15, de una versión posterior):
  facultad → universidad
  programa → facultad
  acreditacion, registro_calificado, pasantia, premio → programa
  activ_academica → programa
  docente_departamento, alianza → programa
  alianza → aliado
  + tablas puente: programa_ac, programa_pe, programa_ci,
    an_programa, enfoque_rc, aa_rc
```

No hay triggers ni procedimientos almacenados en este script: toda la
lógica de negocio, incluido el borrado lógico, vive en la API.

## 2. Lo único que esta versión puede nombrar: las 7 tablas sin llave foránea

Todas usan una llave primaria entera **sin `IDENTITY`**: el valor lo
asigna quien hace el `POST`.

### `area_conocimiento` — 218 registros base

| Columna | Tipo (SQL Server) | Regla |
|---|---|---|
| `id` | `INT` | **PK**, entero positivo, asignado por el cliente |
| `gran_area` | `VARCHAR(60)` | No nulo, no vacío, máx. 60 |
| `area` | `VARCHAR(60)` | No nulo, no vacío, máx. 60 |
| `disciplina` | `VARCHAR(60)` | No nulo, no vacío, máx. 60 |
| `activo` | `BIT` | `1` visible, `0` borrado lógico |

### `universidad` — 6 registros base

| Columna | Tipo | Regla |
|---|---|---|
| `id` | `INT` | **PK**, entero positivo |
| `nombre` | `VARCHAR(60)` | No nulo, no vacío, máx. 60 |
| `tipo` | `VARCHAR(45)` | No nulo, no vacío, máx. 45 |
| `ciudad` | `VARCHAR(45)` | No nulo, no vacío, máx. 45 |
| `activo` | `BIT` | idem |

### `aspecto_normativo`

| Columna | Tipo | Regla |
|---|---|---|
| `id` | `INT` | **PK**, entero positivo |
| `tipo` | `VARCHAR(45)` | No nulo, no vacío, máx. 45 |
| `descripcion` | `VARCHAR(45)` | No nulo, no vacío, máx. 45 |
| `fuente` | `VARCHAR(45)` | No nulo, no vacío, máx. 45 |
| `activo` | `BIT` | idem |

### `practica_estrategia`

| Columna | Tipo | Regla |
|---|---|---|
| `id` | `INT` | **PK**, entero positivo |
| `tipo` | `VARCHAR(45)` | No nulo, no vacío, máx. 45 |
| `nombre` | `VARCHAR(45)` | No nulo, no vacío, máx. 45 |
| `descripcion` | `VARCHAR(45)` | No nulo, no vacío, máx. 45 |
| `activo` | `BIT` | idem |

### `enfoque`

| Columna | Tipo | Regla |
|---|---|---|
| `id` | `INT` | **PK**, entero positivo |
| `nombre` | `VARCHAR(45)` | No nulo, no vacío, máx. 45 |
| `descripcion` | `VARCHAR(45)` | No nulo, no vacío, máx. 45 |
| `activo` | `BIT` | idem |

### `car_innovacion`

| Columna | Tipo | Regla |
|---|---|---|
| `id` | `INT` | **PK**, entero positivo |
| `nombre` | `VARCHAR(45)` | No nulo, no vacío, máx. 45 |
| `descripcion` | `VARCHAR(MAX)` | No nulo, no vacío |
| `tipo` | `VARCHAR(45)` | No nulo, no vacío, máx. 45 |
| `activo` | `BIT` | idem |

### `aliado` — tabla usada para probar el CRUD completo

| Columna | Tipo | Regla |
|---|---|---|
| `nit` | `INT` | **PK** (aquí la llave se llama `nit`, no `id`), entero positivo |
| `razon_social` | `VARCHAR(60)` | No nulo, no vacío, máx. 60 |
| `nombre_contacto` | `VARCHAR(60)` | No nulo, no vacío, máx. 60 |
| `correo` | `VARCHAR(70)` | No nulo, no vacío, máx. 70, formato de correo (regla de la API) |
| `telefono` | `VARCHAR(45)` | No nulo, no vacío, máx. 45 |
| `ciudad` | `VARCHAR(45)` | No nulo, no vacío, máx. 45 |
| `activo` | `BIT` | idem |

En C#, cada fila viaja como su modelo entidad:

```csharp
public class AreaConocimiento
{
    public required int Id { get; set; }
    public required string GranArea { get; set; }
    public required string Area { get; set; }
    public required string Disciplina { get; set; }
    public bool Activo { get; set; } = true;
}

public class Aliado
{
    public required int Nit { get; set; }
    public required string RazonSocial { get; set; }
    public required string NombreContacto { get; set; }
    public required string Correo { get; set; }
    public required string Telefono { get; set; }
    public required string Ciudad { get; set; }
    public bool Activo { get; set; } = true;
}
```

Las otras cinco entidades siguen el mismo patrón: una propiedad por
columna, más `Activo`.

## 3. La columna `activo`

El script provisto no define columna de estado en estas siete tablas. Se
agrega con un script separado, ejecutado después del original:

```sql
-- db/01_activar_borrado_logico.sql
IF COL_LENGTH('area_conocimiento', 'activo') IS NULL
    ALTER TABLE area_conocimiento ADD activo BIT NOT NULL DEFAULT 1;

IF COL_LENGTH('universidad', 'activo') IS NULL
    ALTER TABLE universidad ADD activo BIT NOT NULL DEFAULT 1;

IF COL_LENGTH('aspecto_normativo', 'activo') IS NULL
    ALTER TABLE aspecto_normativo ADD activo BIT NOT NULL DEFAULT 1;

IF COL_LENGTH('practica_estrategia', 'activo') IS NULL
    ALTER TABLE practica_estrategia ADD activo BIT NOT NULL DEFAULT 1;

IF COL_LENGTH('enfoque', 'activo') IS NULL
    ALTER TABLE enfoque ADD activo BIT NOT NULL DEFAULT 1;

IF COL_LENGTH('car_innovacion', 'activo') IS NULL
    ALTER TABLE car_innovacion ADD activo BIT NOT NULL DEFAULT 1;

IF COL_LENGTH('aliado', 'activo') IS NULL
    ALTER TABLE aliado ADD activo BIT NOT NULL DEFAULT 1;
```

`IF COL_LENGTH(...) IS NULL` hace el script seguro de repetir.

## 4. Datos iniciales

`area_conocimiento` inicia con 218 registros y `universidad` con 6,
cargados por `db/02_datos_iniciales.sql` en el mismo paso de
inicialización. Las demás tablas (`aspecto_normativo`,
`practica_estrategia`, `enfoque`, `car_innovacion`, `aliado`) inician
vacías; `aliado` es la tabla sobre la que se prueba el ciclo completo de
creación, reemplazo, actualización parcial, lectura y borrado lógico.

## 5. Las dos murallas de validación

1. **La API** (las peticiones `Crear`/`Reemplazo`/`Actualizar` con
   anotaciones): forma, tipos y largos máximos → 422, antes de tocar la
   base.
2. **La base de datos** (llave primaria, `NOT NULL`): última línea de
   defensa — una llave duplicada la rechaza el motor aunque la API tuviera
   un error, y se reporta como 500 con el detalle del motor.

## 6. Reglas de esta versión

- El código solo puede nombrar las siete tablas sin llave foránea.
- El script provisto no se modifica salvo por la columna `activo` (§3) y
  la carga de datos iniciales (§4).
- El reinicio completo es de Docker, no de SQL:
  `docker compose down -v && docker compose up -d` borra el volumen y
  vuelve a correr los tres scripts desde cero.