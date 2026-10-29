# Modelo de datos — Versión 2: las 15 tablas con clave foránea

> **Versión 2** · La base parte del esquema de la v1 y suma los cambios aprobados
> en [4_research.md](4_research.md), D6 y D15: `activo` en las 15 tablas con FK,
> columna del disparador, IDs internos automáticos y catálogo local de docentes.

---

## 1. Dónde encaja la v2 en la base completa

`innovacion_curricular` tiene **26 tablas** en dos módulos. La v1 opera las **7
sin FK**; la v2 opera las **15 con FK** y el catálogo auxiliar `docente` del
módulo de Innovación Curricular. Las **3 de usuarios** (`usuario`, `rol`,
`rol_usuario`) son de la v3.

```
INNOVACIÓN CURRICULAR (23)                          USUARIOS (3, v3)
──────────────────────────                          ────────────────
Sin FK (7, v1):                                     usuario · rol · rol_usuario
  area_conocimiento · universidad
  aspecto_normativo · practica_estrategia
  enfoque · car_innovacion · aliado

Con FK (15, esta versión):
  facultad        → universidad
  programa        → facultad
  acreditacion    → programa            registro_calificado → programa
  activ_academica → programa (disenio, opcional)
  pasantia        → programa            premio              → programa
  docente_departamento → programa (departamento, PK)
  alianza         → aliado (nit) y → programa (departamento, PK)
  programa_ac     → programa + area_conocimiento       (puente)
  programa_pe     → programa + practica_estrategia     (puente)
  programa_ci     → programa + car_innovacion          (puente)
  an_programa     → aspecto_normativo + programa       (puente)
  enfoque_rc      → enfoque + registro_calificado      (puente)
  aa_rc           → activ_academica + registro_calificado (puente)
```

## 2. Convenciones comunes

- Las PK internas `id` las genera SQL Server de forma incremental (secuencia
  por defecto para las tablas heredadas y `IDENTITY` para `docente`). El documento
  de una persona y las claves naturales no se sustituyen por IDs internos.
- Tablas con **PK simple**: ruta `/api/{tabla}/{id}`.
- Tablas con **PK compuesta**: la ruta lleva las dos claves. Las relaciones con
  atributos `docente_departamento` y `alianza` permiten CRUD de esos atributos
  sin alterar la pareja; las cuatro puentes puras de Felipe sólo se asignan y
  retiran.
- **Todas** las columnas de texto son `NOT NULL` en el script, salvo las
  marcadas **opcional** abajo.
- La v2 agrega **`activo BIT NOT NULL DEFAULT 1`** a las 15 tablas (§3).
- Los **nombres de columna son la fuente de verdad**: el procedimiento los
  devuelve tal cual (o con alias `<fk>_nombre` para los nombres de FK).

---

## 3. Las 15 tablas

### 3.1 `facultad` — responsable: Emmanuel · maestro de `programa`

| Columna | Tipo | Regla | Notas |
|---|---|---|---|
| `id` | `INT` | **PK**, positiva | Generada automáticamente por la secuencia de SQL Server |
| `nombre` | `VARCHAR(60)` | Obligatorio, máx. 60 | |
| `tipo` | `VARCHAR(45)` | Obligatorio, máx. 45 | Texto libre |
| `fecha_fun` | `DATE` | Obligatorio | **`DateTime`** en C# |
| `universidad` | `INT` | **FK → `universidad(id)`**, obligatoria | **Desplegable** |
| `activo` | `BIT` | Agregada por la v2 | Borrado lógico |

### 3.2 `programa` — responsable: Valentina · **maestro central**

| Columna | Tipo | Regla | Notas |
|---|---|---|---|
| `id` | `INT` | **PK**, positiva | Generada automáticamente por la secuencia de SQL Server |
| `nombre` | `VARCHAR(60)` | Obligatorio, máx. 60 | |
| `tipo` | `VARCHAR(45)` | Obligatorio | |
| `nivel` | `VARCHAR(45)` | Obligatorio | |
| `fecha_creacion` | `VARCHAR(45)` | Obligatorio | Es **texto** en el script |
| `fecha_cierre` | `VARCHAR(45)` | **Opcional** (`NULL`) | Es **texto** |
| `numero_cohortes` | `VARCHAR(45)` | Obligatorio | Es **texto** |
| `cant_graduados` | `VARCHAR(45)` | Obligatorio | Es **texto** |
| `fecha_actualizacion` | `VARCHAR(45)` | Obligatorio | Es **texto** |
| `ciudad` | `VARCHAR(45)` | Obligatorio | |
| `facultad` | `INT` | **FK → `facultad(id)`**, obligatoria | **Desplegable** |
| `num_acreditaciones` | `INT` | **Agregada por la v2** (§4) | Sólo la escribe el **disparador** |
| `activo` | `BIT` | Agregada por la v2 | Borrado lógico |

### 3.3 `acreditacion` — Valentina · detalle de `programa`

| Columna | Tipo | Regla |
|---|---|---|
| `resolucion` | `INT` | **PK** (se llama `resolucion`, no `id`) |
| `tipo` | `VARCHAR(45)` | Obligatorio |
| `calificacion` | `VARCHAR(45)` | Obligatorio |
| `fecha_inicio` | `VARCHAR(45)` | Obligatorio (texto) |
| `fecha_fin` | `VARCHAR(45)` | Obligatorio (texto) |
| `programa` | `INT` | **FK → `programa(id)`**, obligatoria (desplegable) |
| `activo` | `BIT` | Agregada por la v2 |

> Es la tabla que dispara el **contador** (§4).

### 3.4 `registro_calificado` — Valentina · detalle de `programa`

| Columna | Tipo | Regla |
|---|---|---|
| `codigo` | `INT` | **PK** (se llama `codigo`) |
| `cant_creditos` | `VARCHAR(45)` | Obligatorio |
| `hora_acom` | `VARCHAR(45)` | Obligatorio |
| `hora_ind` | `VARCHAR(45)` | Obligatorio |
| `metodologia` | `VARCHAR(45)` | Obligatorio |
| `fecha_inicio` | `DATE` | Obligatorio (**`DateTime`**) |
| `fecha_fin` | `DATE` | Obligatorio (**`DateTime`**) |
| `duracion_anios` | `VARCHAR(45)` | Obligatorio |
| `duracion_semestres` | `VARCHAR(45)` | Obligatorio |
| `tipo_titulacion` | `VARCHAR(45)` | Obligatorio |
| `programa` | `INT` | **FK → `programa(id)`**, obligatoria (desplegable) |
| `activo` | `BIT` | Agregada por la v2 |

### 3.5 `activ_academica` — Valentina · detalle de `programa`

| Columna | Tipo | Regla |
|---|---|---|
| `id` | `INT` | **PK** | Generada automáticamente por la secuencia de SQL Server |
| `nombre` | `VARCHAR(45)` | Obligatorio |
| `num_creditos` | `INT` | Obligatorio |
| `tipo` | `VARCHAR(20)` | Obligatorio |
| `area_formacion` | `VARCHAR(45)` | Obligatorio |
| `h_acom` | `INT` | Obligatorio |
| `h_indep` | `INT` | Obligatorio |
| `idioma` | `VARCHAR(45)` | Obligatorio |
| `espejo` | `TINYINT` | Obligatorio (0/1) |
| `entidad_espejo` | `VARCHAR(45)` | Obligatorio |
| `pais_espejo` | `VARCHAR(45)` | Obligatorio |
| `disenio` | `INT` | **FK → `programa(id)`**, **OPCIONAL** → «(ninguna)» → `null` |
| `activo` | `BIT` | Agregada por la v2 |

> `disenio` es la única FK **opcional** de las tablas de Emmanuel/Valentina; es
> el caso de laboratorio del **tropiezo 4**.

### 3.6 `pasantia` — responsable: Emmanuel · detalle de `programa`

| Columna | Tipo | Regla |
|---|---|---|
| `id` | `INT` | **PK** | Generada automáticamente por la secuencia de SQL Server |
| `nombre` | `VARCHAR(45)` | Obligatorio |
| `pais` | `VARCHAR(45)` | Obligatorio |
| `empresa` | `VARCHAR(45)` | Obligatorio |
| `descripcion` | `VARCHAR(45)` | Obligatorio |
| `programa` | `INT` | **FK → `programa(id)`**, obligatoria (desplegable) |
| `activo` | `BIT` | Agregada por la v2 |

### 3.7 `premio` — Emmanuel · detalle de `programa`

| Columna | Tipo | Regla |
|---|---|---|
| `id` | `INT` | **PK** | Generada automáticamente por la secuencia de SQL Server |
| `nombre` | `VARCHAR(45)` | Obligatorio |
| `descripcion` | `VARCHAR(45)` | Obligatorio |
| `fecha` | `DATE` | Obligatorio (**`DateTime`**) |
| `entidad_otorgante` | `VARCHAR(45)` | Obligatorio |
| `pais` | `VARCHAR(45)` | Obligatorio |
| `programa` | `INT` | **FK → `programa(id)`**, obligatoria (desplegable) |
| `activo` | `BIT` | Agregada por la v2 |

### 3.8 `programa_ac` — Felipe · **puente** `programa` ↔ `area_conocimiento`

| Columna | Tipo | Regla |
|---|---|---|
| `programa` | `INT` | **PK compuesta**, FK → `programa(id)` (desplegable) |
| `area_conocimiento` | `INT` | **PK compuesta**, FK → `area_conocimiento(id)` (desplegable, v1) |
| `activo` | `BIT` | Agregada por la v2 |

### 3.9 `programa_pe` — Felipe · **puente** `programa` ↔ `practica_estrategia`

| Columna | Tipo | Regla |
|---|---|---|
| `programa` | `INT` | **PK compuesta**, FK → `programa(id)` |
| `practica_estrategia` | `INT` | **PK compuesta**, FK → `practica_estrategia(id)` (v1) |
| `activo` | `BIT` | Agregada por la v2 |

### 3.10 `programa_ci` — Felipe · **puente** `programa` ↔ `car_innovacion`

| Columna | Tipo | Regla |
|---|---|---|
| `programa` | `INT` | **PK compuesta**, FK → `programa(id)` |
| `car_innovacion` | `INT` | **PK compuesta**, FK → `car_innovacion(id)` (v1) |
| `activo` | `BIT` | Agregada por la v2 |

### 3.11 `an_programa` — Felipe · **puente** `aspecto_normativo` ↔ `programa`

| Columna | Tipo | Regla |
|---|---|---|
| `aspecto_normativo` | `INT` | **PK compuesta**, FK → `aspecto_normativo(id)` (v1) |
| `programa` | `INT` | **PK compuesta**, FK → `programa(id)` |
| `activo` | `BIT` | Agregada por la v2 |

> **Ojo con el orden de la PK:** en `an_programa` la primera columna de la PK es
> `aspecto_normativo` (no `programa`). La ruta de retiro lleva las claves en
> **este orden**.

### 3.12 `enfoque_rc` — Valentina · **puente** `enfoque` ↔ `registro_calificado`

| Columna | Tipo | Regla |
|---|---|---|
| `enfoque` | `INT` | **PK compuesta**, FK → `enfoque(id)` (v1) |
| `registro_calificado` | `INT` | **PK compuesta**, FK → `registro_calificado(codigo)` |
| `activo` | `BIT` | Agregada por la v2 |

### 3.13 `aa_rc` — Valentina · **puente** `activ_academica` ↔ `registro_calificado`

| Columna | Tipo | Regla |
|---|---|---|
| `activ_academicas_idcurso` | `INT` | **PK compuesta**, FK → `activ_academica(id)` |
| `registro_calificado_codigo` | `INT` | **PK compuesta**, FK → `registro_calificado(codigo)` |
| `componente` | `VARCHAR(45)` | Obligatorio (atributo de la pareja) |
| `semestre` | `VARCHAR(45)` | Obligatorio (atributo de la pareja) |
| `activo` | `BIT` | Agregada por la v2 |

> **Nombres largos.** Las columnas de la PK se llaman
> `activ_academicas_idcurso` y `registro_calificado_codigo` (no `activ_academica`
> ni `registro_calificado`). Los parámetros del procedimiento y los alias del
> `SELECT` deben usar **esos** nombres.

### 3.13a DOCENTE — catálogo local de personas de Gestión Profesoral

| Columna | Tipo | Regla |
|---|---|---|
| id | INT IDENTITY | PK interna autogenerada e incremental |
| documento | VARCHAR(20) | Obligatorio, único; identifica a la persona pero no es PK |
| nombre | VARCHAR(120) | Obligatorio |
| activo | BIT | Borrado lógico; sólo activos aparecen en los desplegables |

> Este catálogo local se incorpora por decisión explícita del propietario del
> proyecto para evitar códigos de docente digitados a mano. No reemplaza el
> número de documento ni expone información de otra base de datos.

### 3.14 `docente_departamento` — Felipe · vinculación (docente local ↔ `programa`)

| Columna | Tipo | Regla |
|---|---|---|
| `docente` | `INT` | **PK compuesta**, FK → `docente(id)` (desplegable) |
| `departamento` | `INT` | **PK compuesta**, FK → `programa(id)` (desplegable) |
| `dedicacion` | `VARCHAR(15)` | Obligatorio |
| `modalidad` | `VARCHAR(45)` | Obligatorio |
| `fecha_ingreso` | `DATE` | Obligatorio (**`DateTime`**) |
| `fecha_salida` | `DATE` | **Opcional** (`NULL`) |
| `activo` | `BIT` | Agregada por la v2 |

> El nombre `departamento` **apunta a `programa`** (así lo declara el script).
> `docente` referencia el catálogo local: su ID es interno e incremental, y el
> número de documento se guarda en una columna distinta. Dedicación, modalidad
> y fechas son atributos editables; la pareja docente-programa es inmutable.

### 3.15 `alianza` — Felipe · `aliado` ↔ `programa`

| Columna | Tipo | Regla |
|---|---|---|
| `aliado` | `INT` | **PK compuesta**, FK → `aliado(nit)` (desplegable, v1) |
| `departamento` | `INT` | **PK compuesta**, FK → `programa(id)` (desplegable) |
| `fecha_inicio` | `DATE` | Obligatorio (**`DateTime`**) |
| `fecha_fin` | `DATE` | **Opcional** (`NULL`) |
| `docente` | `INT` | **Opcional** (`NULL`), FK → `docente(id)` (selección de responsable) |
| `activo` | `BIT` | Agregada por la v2 |

> Validación de negocio de Felipe (Issue 3): **`fecha_fin ≥ fecha_inicio`** (y en
> `docente_departamento`, `fecha_salida ≥ fecha_ingreso`) → si no, **400** por regla de negocio; campos ausentes o tipos inválidos son **422**
> según la constitución. Ver [6_contracts.md](6_contracts.md) §8.
> Las fechas y el docente responsable se pueden reemplazar o actualizar por
> separado; `aliado` y `departamento` siguen siendo la identidad fija de la fila.

---

## 4. La columna del disparador y el mapa de hijos

### 4.1 Columna agregada

```sql
-- db/03_activar_borrado_logico_v2.sql (fragmento)
IF COL_LENGTH('programa', 'num_acreditaciones') IS NULL
    ALTER TABLE programa ADD num_acreditaciones INT NOT NULL DEFAULT 0;
```

- `programa.num_acreditaciones` es un **estado derivado**: lo mantiene el
  disparador `trg_acreditacion_contador` ([4_research.md](4_research.md) D3).
- La API **nunca** lo escribe ni lo envía; sólo lo **lee** en los listados.

### 4.2 Hijos activos de cada maestro (para el bloqueo del borrado)

La regla §3.1 de [2_spec.md](2_spec.md) — "un padre con hijos activos no se
retira" — necesita este mapa:

| Padre | Tablas hijas que se comprueban (`activo = 1`) |
|---|---|
| `facultad` | `programa` |
| `programa` | `acreditacion`, `registro_calificado`, `activ_academica`, `pasantia`, `premio`, `docente_departamento`, `alianza`, `programa_ac`, `programa_pe`, `programa_ci`, `an_programa` |
| `registro_calificado` | `enfoque_rc`, `aa_rc` |
| `activ_academica` | `aa_rc` |
| `aliado` | `alianza` |

> Las puentes (`programa_ac`, …) **sí** cuentan como hijos activos del maestro:
> una pareja activa impide retirar el `programa`.

---

## 5. Cómo viaja cada fila (modelo C# ↔ procedimiento)

Cada fila viaja como su **modelo entidad**, con una propiedad por columna, más
`Activo` y la propiedad de sólo lectura del nombre de la FK:

```csharp
// Modelos/Facultad.cs — dominio en castellano, como el molde de la v1.
public class Facultad
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Tipo { get; set; }
    public required DateTime FechaFun { get; set; }        // fecha_fun DATE
    public required int Universidad { get; set; }          // clave que se envía
    public string? UniversidadNombre { get; set; }         // alias del JOIN (sólo lectura)
    public bool Activo { get; set; } = true;
}
```

- El **nombre de la FK** (`UniversidadNombre`) lo llena el `LEFT JOIN` del
  procedimiento con alias `universidad_nombre`; el modelo lo expone para que el
  Frontend muestre el nombre sin una segunda consulta.
- Los campos de **fecha** siguen el tipo del script (§3): `DATE` → `DateTime`;
  `VARCHAR` → `string`.

## 6. Las dos murallas de validación

1. **La API** (peticiones con anotaciones): forma, tipos, largos máximos,
  obligatoriedad de FK → **422**, antes de tocar la base.
2. **La base de datos** (PK, FK, `NOT NULL`): última línea de defensa. Una
  violación de **PK** (pareja duplicada) o de **FK** (padre inexistente) la
  rechaza el motor y se traduce a **409**, nunca 500 (ver
  [4_research.md](4_research.md) D1).

## 7. Reglas de esta versión

- El código de la v2 sólo puede nombrar las **15 tablas con FK** (más las 7 de la
  v1, que **lee** para los desplegables, sin tocarlas).
- El script provisto `innovacion_curricular.ss.sql` **no se modifica**. Todo
  cambio de esquema va en `03_activar_borrado_logico_v2.sql`.
- El reinicio completo sigue siendo de Docker:
  `docker compose down -v && docker compose up -d --build` vuelve a correr todos
  los scripts desde cero.
