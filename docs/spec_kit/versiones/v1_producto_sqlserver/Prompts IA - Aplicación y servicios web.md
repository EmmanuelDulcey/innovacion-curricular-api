## **Prompt Valentina – Utilizo chat para generar el prompt con base en los documentos y terminal para que haga código**

```text
“Debo de hacer la issue 3. Quiero que leas cada uno de los documentos y me digas que entiendes que hay que hacer.”
```

- Respuesta de la IA de chat
```text
# Prompt para la IA que implementará la Issue #3

Copia y pega esto tal cual en la IA que va a escribir el código:

---

## Contexto del proyecto

Vas a implementar la **Issue #3** de la **Versión 1** del proyecto de aula **"Innovación Curricular"**. Es un monorepo con una API en **C# / [ASP.NET](https://asp.net/) Core** sobre **SQL Server** (acceso a datos con **Dapper**) y un frontend en **Angular**. La v1 construye el CRUD completo de las **7 tablas sin llave foránea** del módulo. El trabajo se repartió en 3 issues; tú trabajas **solo la Issue #3**, que cubre las tablas `car_innovacion` y `aliado`.

## Antes de escribir una sola línea de código

**Lee TODOS estos documentos, en este orden, y no asumas nada que no esté escrito en ellos:**

1. `docs/spec_kit/1_constitution.md` — reglas permanentes del proyecto (prevalece sobre todo lo demás).  
2. `docs/spec_kit/versiones/0_mapa_versiones.md` — dónde encaja la v1 en el proyecto.  
3. `docs/spec_kit/versiones/v1_producto_sqlserver/2_spec.md` — qué construir en v1 y criterios de aceptación.  
4. `docs/spec_kit/versiones/v1_producto_sqlserver/3_plan.md` — cómo construirlo (stack, estructura de carpetas, patrón de capas). 
5. `docs/spec_kit/versiones/v1_producto_sqlserver/4_research.md` — por qué de las decisiones (lectura opcional pero útil). 
6. `docs/spec_kit/versiones/v1_producto_sqlserver/5_data_model.md` — modelo de datos exacto de las 7 tablas.  
7. `docs/spec_kit/versiones/v1_producto_sqlserver/6_contracts.md` — **contratos HTTP exactos** (verbos, rutas, códigos, formatos de respuesta).  
8. `docs/spec_kit/versiones/v1_producto_sqlserver/7_quickstart.md` — cómo levantar y probar.  
9. `docs/spec_kit/versiones/v1_producto_sqlserver/8_tasks.md` — orden de construcción por fases.  
10. `docs/spec_kit/versiones/v1_producto_sqlserver/9_frontend.md` — especificación de la interfaz Angular.  
11. `issues_v1.md` — división del trabajo; **tu issue es la #3**.   

## Tu tarea exacta

Implementar el **CRUD completo de las tablas `car_innovacion` y `aliado`** de punta a punta (repositorio → servicio → controlador en la API, y su listado/formulario en el frontend Angular), respetando **al pie de la letra** los contratos de `6_contracts.md` y el patrón de capas de `3_plan.md`.

Puntos críticos que NO puedes pasar por alto:

- **`aliado` es especial:** su llave primaria se llama **`nit`**, no `id`. Las rutas son `/api/aliado/{nit}` y el `[Range]` de la PK aplica sobre `Nit`. El campo `correo` lleva validación de formato (`[EmailAddress]`).   
- **`car_innovacion.descripcion` es VARCHAR(MAX)** → sin `[StringLength]` en la petición (sin límite práctico en la API). 
- **Ninguna de las dos tablas usa `IDENTITY`:** el cliente asigna `id`/`nit` en el POST. 
- **Borrado lógico:** el DELETE es un `UPDATE ... SET activo = 0`, nunca un `DELETE` físico.
- **Las listas vacías responden 204 sin cuerpo.** Con datos, 200 con envoltura `{tabla, limite, total, datos}`.
- **PATCH con body vacío (`{}`) → 400** (lo decide el servicio, no la validación de forma). 
- **PK duplicada → 500** con el detalle del motor SQL Server. 
- **Segundo DELETE sobre un registro ya borrado → 404.**  
- **`limite` por defecto 1000;** `limite <= 0` → 400.  
- **Sin anticipación:** nada de JWT, roles, login, ni tablas con FK. Eso es de versiones posteriores (v2, v3, v4).  

## Reglas de la constitución que rigen tu trabajo

- El controlador no toca SQL; el servicio no conoce HTTP ni el motor; el repositorio no conoce HTTP. Los contratos entre capas son **interfaces de C#**. 
- Solo el ensamblador (`Program.cs`) conoce clases concretas. 
- SQL escrito a mano y **parametrizado** (`@parametro`). Todo `async/await`. 
- Código y API en **inglés**; textos de la interfaz del frontend pueden estar en **español**; comentarios explicativos para claridad académica.   
- Cero secretos en el código; cadena de conexión por variables de entorno.   
- Trabajas en la rama **`rama-valentina`**, nunca en `main`. Commits pequeños, frecuentes y descriptivos en español.   

## Coordinación con los otros issues

El esqueleto compartido (BD + Docker, proyecto .NET, frontend genérico, `Program.cs`, `nav-bar`, `dashboard`) lo construyen entre los tres o ya existe. Tú **solo aportas tus 2 tablas dentro de ese esqueleto**. Al tocar `Program.cs` (registro DI) y `nav-bar`/`dashboard` (enlaces y tarjetas), hazlo de forma que no rompas los registros de los otros issues.

## Cómo quiero que trabajes

1. **Primero lee todo.** No empieces a codificar hasta haber leído los 11 documentos.  
2. **Si algo no te queda claro, PREGUNTA ANTES de tomar una decisión.** No inventes, no asumas, no "rellenes" huecos con criterio propio. Casos típicos donde debes preguntar:
    - Si un documento contradice a otro (ej. `dotnet run` vs `dotnet watch` en `3_plan.md` vs `4_research.md`/`8_tasks.md`).  
    - Si el esqueleto compartido no existe todavía y no sabes si debes crearlo o esperar. 
    - Si no está claro dónde va un archivo o cómo se llama una clase.  
    - Si dudas sobre el formato exacto de una respuesta HTTP.  
    - Si el patrón de una tabla no aplica limpiamente a `aliado` (por el `nit`). 
3. **Sigue el orden de fases de `8_tasks.md`** (Fases 1–5 para la API, 8–10 para el frontend), verificando al final de cada fase que compila.
4. **Verifica contra `7_quickstart.md`** antes de dar por terminada tu issue.
5. **Definition of Done** (de `issues_v1.md` §Issue #3): `dotnet build` sin errores; CRUD completo probado en `aliado` (POST → PUT → PATCH → GET → DELETE → segundo DELETE 404); correo inválido → 422; PK duplicada → 500; PATCH `{}` → 400; desde `http://localhost:8037` crear/editar/eliminar un `aliado` y que persista al recargar; formulario inválido muestra errores 422 por campo sin romper la UI; estado vacío cuando no hay registros.

## Entregable esperado

Cuando termines, entrégame:

- La lista de archivos creados/modificados (agrupados por capa).
- La evidencia de que `dotnet build` compila.
- Los comandos `curl` (o equivalente) que ejecutaste para verificar cada caso del Definition of Done, con la respuesta obtenida.
- Cualquier duda que hayas resuelto preguntando y la decisión final tomada.
- Nada de código sin explicar: quiero entender qué hiciste y por qué, porque el repositorio es material de estudio.

**Recuerda: ante la duda, pregunta. No improvises.**
```

```text
“listo, ahora teniendo el contexto de la documentación que es como lo más importante que tengo hasta ahora, quiero que me des un buen prompt para la IA que va a hacer el codigo, dile que lea todo el spec_kit, el documento de los issues para que no se desvie y sepa que hacer. dile que si no entiende algo primero pregunte antes de tomar alguna decision.”
```
## **Prompt Emmanuel – Utilizo terminal**

```text
“Emmanuel Alexander Dulcey Atehortua

Documentación spec_kit: Revisa este documento en md. Conserva la misma estructura y modifica la información con base en mi proyecto.

Modificación frontend en spec_kit:

Revisa todo el spec_kit y modifica la información necesaria para que cada versión tenga front.

Issues: Con base en el spec_kit de la v1, reparte todo el trabajo en 3 issues equitativos.

Materialización issue 1: Ayúdame a realizar todo lo del issue 1. Tu objetivo es hacer todo lo del issue 1, no acabes hasta cumplir tu objetivo”
```

## **Prompt Felipe –** 

```text
“Usar la documentación y especificaciones reales del proyecto Innovación Curricular para crear los archivos faltantes de la V1, siguiendo la estructura proporcionada por el profesor como referencia, pero adaptándola al proyecto y sin copiarla literalmente. Mantener consistencia entre los documentos, la base de datos, los contratos HTTP y la implementación que posteriormente se realizará.”

“Crear 6_contracts.md, 7_quickstart.md y 8_tasks.md según el proyecto Innovación Curricular, adaptando la estructura del material del profesor y no copiándolo, los documentos anteriores del proyecto, especialmente 2_spec.md y 5_data_model.md, para que todo quede consistente”

“Ahora toca 7_quickstart.md — Arranque y smoke test. Con eso construimos 7_quickstart.md alineándolo con lo que ya habíamos definido en 5_data_model.md y 6_contracts.md: Docker Compose, puerto 8036, Swagger, los 218 registros de area_conocimiento, las 6 universidades y el smoke test completo del CRUD de aliado. ”

“Basándote en 2_spec.md, 5_data_model.md, 6_contracts.md y 7_quickstart.md, crea 8_tasks.md para la V1 de Innovación Curricular. Divide la implementación en fases y tareas concretas, ordenadas por dependencias. Cada tarea debe indicar claramente qué se debe implementar y qué criterio permite considerarla terminada. Mantén la arquitectura Controller → Service → Repository, Dapper + SQL Server, Docker, validaciones, los 7 recursos sin FK, borrado lógico y los contratos HTTP definidos. No agregues funcionalidades de versiones posteriores.”

“Revisa el repositorio “innovacion curricular api”, creado por Emmanuel. Busca los issues asignados a Felipe y trabaja únicamente sobre ellos.

Antes de escribir código:

1. Lee todo el repositorio y los documentos de requisitos.
2. Toma como referencia la estructura y el commit 752900a de Emmanuel.
3. Revisa las propuestas de Astra.
4. Define primero la estructura, el alcance y los criterios de aceptación.
5. El código debe ser la última etapa.
6. Trabaja en la rama felipe-correa, nunca en main.
7. No crees soluciones nuevas ni modifiques otros proyectos.
8. No hagas commits ni push sin autorización explícita.

Issue de Felipe:

CRUD completo de aspecto_normativo, practica_estrategia y enfoque — API REST + Frontend.

Debe incluir:

- Modelos tipados con Activo=true por defecto.
- Peticiones Crear, Reemplazo y Actualizar con Required, StringLength y Range.
- Repositorios Dapper con SQL parametrizado.
- Servicios con validación de PATCH vacío y registros inexistentes.
- Controladores REST con GET, POST, PUT, PATCH y DELETE.
- Borrado lógico mediante activo=0.
- Registro de dependencias en Program.cs sin romper los otros issues.
- Interfaces Angular en camelCase.
- Listados, formularios Crear/Editar, navegación y tarjetas del dashboard.
- PUT al editar, refresco del listado y mensajes de error por campo.

Definition of Done:

- dotnet build sin errores ni advertencias.
- GET sin registros devuelve 204.
- GET con registros devuelve la envoltura tabla, limite, total y datos.
- CRUD completo probado en las tres tablas.
- POST inválido devuelve 422.
- PATCH {} devuelve 400.
- DELETE marca el registro como inactivo y el segundo DELETE devuelve 404.
- Frontend funcionando en http://localhost:8037.
- No crear commits hasta recibir autorización.”
```
