# Versión 2 con IA — índice de las guías

> **La versión de la clave foránea.** Con ella llegan: el código **409**, el
> **desplegable** en lugar de la clave digitada, los **procedimientos
> almacenados** como única vía al SQL, y al menos un **disparador**.

---

## 1. Quién hace qué

| | Su guía | Lo que le toca en la v2 |
|---|---|---|
| **Emmanuel** | [`GUIA_IA2_EMMANUEL.md`](GUIA_IA2_EMMANUEL.md) | Spec kit de la v2 + `facultad`, `pasantia`, `premio` |
| **Valentina** | [`GUIA_IA2_VALENTINA.md`](GUIA_IA2_VALENTINA.md) | `programa` (maestro-detalle), `acreditacion`, `registro_calificado`, `activ_academica`, `enfoque_rc`, `aa_rc` + disparador |
| **Felipe** | [`GUIA_IA2_FELIPE.md`](GUIA_IA2_FELIPE.md) | `docente_departamento`, `alianza`, `programa_ac`, `programa_pe`, `programa_ci`, `an_programa` |

> **Cada quien escribe y mantiene SU guía.** El prompt, lo que se le sube a la
> IA y el registro de lo que hubo que corregirle son de cada integrante. El
> integrador (encargado del main) escribe este índice.

## 2. El orden

```
 1 · EMMANUEL            2 · VALENTINA y FELIPE, a la vez       3 · INTEGRADOR
 spec kit + facultad →   sus recursos (programa ya existe)  →   integra, regresión v1, tag v2
```

> **Emmanuel va primero:** sin spec kit no se construye (la spec se escribe
> ANTES), y sin `facultad` no se puede crear `programa`.

## 3. La decisión que define la versión: 409, no 422 ni 500

Alguien manda un `programa` con una `facultad` que no existe. ¿Qué responde la API?

| | Argumento |
|---|---|
| **422** | «El dato está mal» — pero el dato tiene la forma correcta |
| **500** | «Me caí» — pero la base funciona; solo rechazó la relación |
| **409** ✅ | Lo que se rompe es el **ESTADO** de la base: esa fila no existe (o la pareja ya existe) |

> El 422 se reserva para lo que se rechaza **sin consultar nada** (campo
> faltante, tipo equivocado). Saber si la facultad existe **exige ir a la
> base de datos**: eso es estado. **¿Quién traduce el error del motor, y en qué
> capa?** Es pregunta de sustentación casi segura.

## 4. Los tropiezos de esta versión (léanlos los tres)

Casi todos fallan **en silencio**: sin excepción, sin log, con HTTP 200.

| | Qué pasa | Cómo se ve |
|---|---|---|
| **1** | El front trata la respuesta como lista pelada | Pantalla **vacía sin error**. La API devuelve `{table, limit, total, data}`: hay que entrar a `data` |
| **2** | Dapper mapea por **nombre de columna** | Propiedad con nombre distinto a la columna llega `null`, con 200 |
| **3** | Nombres de columnas que devuelven los procedimientos | Con alias o JOIN llegan `null`/0 si la propiedad no coincide |
| **4** | Cadena vacía en un desplegable opcional | `""` no es `null`: error de conversión. «(ninguna)» debe enviar `null` |
| **5** | Desplegables cargados uno tras otro | Con la API lenta, la pantalla tarda. Cargarlos en paralelo (`forkJoin`) |
| **6** | **Disparador sordo al UPDATE** | Con borrado lógico, retirar es un UPDATE. El contador queda mal |
| **7** | Listado sin `WHERE activo = 1` | Lo retirado sigue apareciendo y se ofrece en desplegables |
| **8** | Procedimiento que no devuelve la fila | La interfaz no puede mostrar lo que acaba de crear |
| **9** | Botón de editar en una tabla puente | Una pareja existe o no existe; no se edita |

## 5. Lo que vale para todos

**La IA comenta lo que escribe** (va dentro del prompt). Desde la v2 la
interpretabilidad se califica en voz alta: **un archivo sin comentarios es un
archivo que hay que reconstruir de memoria frente al profesor.**

**Identidad de Git, antes del primer commit** (parado en la carpeta del repo):

```powershell
git config user.name "su-usuario-de-github"
git config user.email "su-correo-de-github"
git config user.name    # comprobar SIEMPRE
git config user.email
```

**Nadie trabaja en `main`.** Ramas: `rama-emmanuel-v2`, `rama-valentina-v2`,
`rama-felipe-v2`. Todo entra por Pull Request; solo el encargado del main fusiona.

## 6. Lo que NINGUNO sube a la IA

| No se sube | Por qué |
|---|---|
| `9_checklist.md` | Es **su** compuerta, no la de la IA |
| `0_mapa_versiones.md` | Revelaría lo que viene; la v2 no anticipa |
| Archivos de las guías de otros compañeros | Cada guía es personal |

> Sí se sube: la **constitución**, el **spec kit de la v2** ya escrito, el
> script SQL con las tablas y las clases de la v1 como molde.

## 7. Cuándo está terminada la versión

| Qué se comprueba | Cómo |
|---|---|
| 409 por clave foránea inexistente | `POST /api/programa` con una facultad que no existe |
| 409 por pareja repetida | `POST` dos veces la misma pareja en una tabla puente |
| Disparador | Hacer la operación **por la interfaz** y ver cambiar el dato sin que la API lo envíe; incluir **retirar** un hijo |
| Borrado lógico | Retirar: desaparece de la lista y **sigue en la base de datos** |
| Desplegable opcional | Crear un registro sin la FK opcional (queda `null`) |
| Maestro-detalle | Varios renglones del detalle en **un solo envío** |
| **Regresión de la v1** | Los criterios de la v1, completos, otra vez |

Con todo en verde y el `9_checklist.md` marcado **por una persona**:

```powershell
git tag -a v2 -m "Version 2: tablas con clave foranea, procedimientos y disparador"
git push origin v2
```
