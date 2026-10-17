-- =============================================================================
-- 06_datos_demostracion.sql
-- Reemplaza sólo los registros marcados como prueba por ejemplos académicos
-- reconocibles. Se ejecuta en Docker y es idempotente; no borra relaciones.
-- =============================================================================
USE [innovacion_curricular];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Normas colombianas identificadas por su número oficial.
IF EXISTS (SELECT 1 FROM dbo.aspecto_normativo WHERE descripcion = N'Norma de prueba')
    UPDATE dbo.aspecto_normativo
       SET tipo = N'Ley',
           descripcion = N'Ley 115 de 1994',
           fuente = N'Congreso de Colombia'
     WHERE descripcion = N'Norma de prueba';

UPDATE dbo.aspecto_normativo
   SET tipo = N'Ley',
       descripcion = N'Ley 115 de 1994',
       fuente = N'Congreso de Colombia'
 WHERE descripcion IN (N'Norma de prueba', N'Ley General de Educación', N'Ley 115 de 1994');

IF NOT EXISTS (SELECT 1 FROM dbo.aspecto_normativo WHERE descripcion = N'Ley 115 de 1994')
    INSERT INTO dbo.aspecto_normativo (tipo, descripcion, fuente, activo)
    VALUES (N'Ley', N'Ley 115 de 1994', N'Congreso de Colombia', 1);

UPDATE dbo.aspecto_normativo
   SET tipo = N'Decreto',
       descripcion = N'Decreto 1075 de 2015',
       fuente = N'Presidencia de la República'
 WHERE descripcion IN (N'Decreto Único de Educación', N'Decreto 1075 de 2015');

IF NOT EXISTS (SELECT 1 FROM dbo.aspecto_normativo WHERE descripcion = N'Decreto 1075 de 2015')
    INSERT INTO dbo.aspecto_normativo (tipo, descripcion, fuente, activo)
    VALUES (N'Decreto', N'Decreto 1075 de 2015', N'Presidencia de la República', 1);

-- Si una ejecución anterior creó el mismo ejemplo más de una vez, se conserva
-- activa la fila de menor ID. No se desactiva un duplicado que tenga una
-- relación activa con un programa.
;WITH NormasRepetidas AS
(
    SELECT id,
           ROW_NUMBER() OVER (PARTITION BY descripcion ORDER BY id) AS numero
      FROM dbo.aspecto_normativo
     WHERE activo = 1
       AND descripcion IN (N'Ley 115 de 1994', N'Decreto 1075 de 2015')
)
UPDATE an
   SET activo = 0
  FROM dbo.aspecto_normativo an
  JOIN NormasRepetidas nr ON nr.id = an.id
 WHERE nr.numero > 1
   AND NOT EXISTS
       (SELECT 1 FROM dbo.an_programa ap WHERE ap.aspecto_normativo = an.id AND ap.activo = 1);

-- Estrategias docentes conocidas y aplicables a una sustentación académica.
IF EXISTS (SELECT 1 FROM dbo.practica_estrategia WHERE nombre = N'Práctica de prueba')
    UPDATE dbo.practica_estrategia
       SET tipo = N'Metodología activa',
           nombre = N'Aprendizaje basado en proyectos',
           descripcion = N'Resolver retos mediante proyectos'
     WHERE id = (SELECT MIN(id) FROM dbo.practica_estrategia WHERE nombre = N'Práctica de prueba');

IF EXISTS (SELECT 1 FROM dbo.practica_estrategia WHERE nombre = N'Práctica de prueba')
    UPDATE dbo.practica_estrategia
       SET tipo = N'Estrategia didáctica',
           nombre = N'Aula invertida',
           descripcion = N'Estudio previo y práctica guiada'
     WHERE id = (SELECT MIN(id) FROM dbo.practica_estrategia WHERE nombre = N'Práctica de prueba');

IF NOT EXISTS (SELECT 1 FROM dbo.practica_estrategia WHERE nombre = N'Aprendizaje basado en proyectos')
    INSERT INTO dbo.practica_estrategia (tipo, nombre, descripcion, activo)
    VALUES (N'Metodología activa', N'Aprendizaje basado en proyectos', N'Resolver retos mediante proyectos', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.practica_estrategia WHERE nombre = N'Aula invertida')
    INSERT INTO dbo.practica_estrategia (tipo, nombre, descripcion, activo)
    VALUES (N'Estrategia didáctica', N'Aula invertida', N'Estudio previo y práctica guiada', 1);

-- Características curriculares documentadas por la Universidad de Antioquia.
-- Los nombres se basan en la evaluación de su renovación curricular; las
-- descripciones son síntesis propias para que se entiendan en el catálogo.
-- Fuentes: https://doi.org/10.17533/udea.unipluri.11839 y
-- https://www.udea.edu.co/wps/portal/udea/web/inicio/campanas/innovacion-curricular/Contenido/asMenuLateral/sobre-el-proyecto
DECLARE @CaracteristicasCurriculares TABLE
(
    orden INT PRIMARY KEY,
    nombre NVARCHAR(45) NOT NULL,
    descripcion NVARCHAR(MAX) NOT NULL,
    tipo NVARCHAR(45) NOT NULL
);

INSERT INTO @CaracteristicasCurriculares (orden, nombre, descripcion, tipo)
VALUES
    (1, N'Flexibilidad curricular',
     N'Ofrece rutas formativas y opciones de profundización que responden a los intereses del estudiante.',
     N'Principio curricular'),
    (2, N'Interdisciplinariedad',
     N'Integra áreas complementarias, cursos y fortalezas docentes para abordar la formación desde varias disciplinas.',
     N'Principio curricular'),
    (3, N'Articulación teoría-práctica',
     N'Conecta los conceptos académicos con experiencias, proyectos y situaciones del ejercicio profesional.',
     N'Principio curricular'),
    (4, N'Formación integral',
     N'Articula el desarrollo disciplinar con capacidades personales, éticas, ciudadanas e investigativas.',
     N'Principio curricular'),
    (5, N'Pertinencia social',
     N'Vincula los contenidos y experiencias de formación con las necesidades del contexto y del campo profesional.',
     N'Principio curricular'),
    (6, N'Formación en investigación',
     N'Incorpora la indagación, los proyectos y la participación en semilleros al proceso de aprendizaje.',
     N'Principio curricular'),
    (7, N'Transversalidad curricular',
     N'Integra temas como lenguajes, tecnologías digitales y emprendimiento a través de distintas áreas y cursos.',
     N'Principio curricular');

-- Convierte los dos registros genéricos existentes sin cambiar sus IDs, para
-- conservar cualquier relación programa-característica que ya los referencie.
;WITH CaracteristicasDePrueba AS
(
    SELECT id,
           ROW_NUMBER() OVER (ORDER BY id) AS orden
      FROM dbo.car_innovacion
     WHERE nombre IN (N'Innovación de prueba', N'Característica de prueba')
)
UPDATE ci
   SET nombre = cc.nombre,
       descripcion = cc.descripcion,
       tipo = cc.tipo,
       activo = 1
  FROM dbo.car_innovacion ci
  JOIN CaracteristicasDePrueba cp ON cp.id = ci.id
  JOIN @CaracteristicasCurriculares cc ON cc.orden = cp.orden;

-- Completa las características que falten sin duplicar ni reactivar las que
-- alguien haya retirado explícitamente del catálogo.
INSERT INTO dbo.car_innovacion (nombre, descripcion, tipo, activo)
SELECT cc.nombre, cc.descripcion, cc.tipo, 1
  FROM @CaracteristicasCurriculares cc
 WHERE NOT EXISTS
       (SELECT 1 FROM dbo.car_innovacion ci WHERE ci.nombre = cc.nombre);

-- Actualiza uno de los aliados de prueba de Felipe con una institución real.
-- La columna NIT almacena el número base sin dígito de verificación porque el
-- esquema heredado la define como INT; el documento de una persona no se usa aquí.
IF EXISTS (SELECT 1 FROM dbo.aliado WHERE nit = 900100001 AND razon_social LIKE N'Aliado de prueba%')
   AND NOT EXISTS (SELECT 1 FROM dbo.aliado WHERE nit = 890980040)
   AND NOT EXISTS (SELECT 1 FROM dbo.alianza WHERE aliado = 900100001)
BEGIN
    UPDATE dbo.aliado
       SET nit = 890980040,
           razon_social = N'Universidad de Antioquia',
           nombre_contacto = N'Atención al ciudadano',
           correo = N'atencionciudadano@udea.edu.co',
           telefono = N'604 219 8332',
           ciudad = N'Medellín',
           activo = 1
     WHERE nit = 900100001;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.aliado WHERE nit = 890904996)
    INSERT INTO dbo.aliado (nit, razon_social, nombre_contacto, correo, telefono, ciudad, activo)
    VALUES (890904996, N'Empresas Públicas de Medellín E.S.P.', N'Atención al cliente EPM',
            N'epm@epm.com.co', N'604 444 4800', N'Medellín', 1);

-- No se quitan datos que estén participando en una alianza ya creada.
UPDATE dbo.aliado
   SET activo = 0
 WHERE razon_social LIKE N'Aliado de prueba%'
   AND NOT EXISTS
       (SELECT 1 FROM dbo.alianza a WHERE a.aliado = dbo.aliado.nit AND a.activo = 1);

-- Ejemplo real de facultad de la Universidad de Antioquia. Su historia oficial
-- indica que la Escuela de Ciencias Químicas nació en 1943 y se transformó en
-- Facultad en 1958. Como fecha_fun exige DATE y la fuente sólo da el año, se usa
-- 1 de enero de 1958 como convención de año conocido, no como día histórico exacto.
IF EXISTS (SELECT 1 FROM dbo.facultad WHERE id = 9001 AND nombre = N'Facultad de prueba Felipe')
    UPDATE dbo.facultad
       SET nombre = N'Facultad de Ingeniería',
           tipo = N'Pública',
           fecha_fun = '1958-01-01',
           universidad = 2
     WHERE id = 9001;

UPDATE dbo.facultad
   SET tipo = N'Pública',
       fecha_fun = '1958-01-01'
 WHERE nombre = N'Facultad de Ingeniería'
   AND universidad = 2
   AND (tipo <> N'Pública' OR fecha_fun = '1943-01-26');

IF NOT EXISTS
   (SELECT 1 FROM dbo.facultad WHERE nombre = N'Facultad de Ingeniería' AND universidad = 2)
    INSERT INTO dbo.facultad (nombre, tipo, fecha_fun, universidad, activo)
    VALUES (N'Facultad de Ingeniería', N'Pública', '1958-01-01', 2, 1);

DECLARE @facultadUdea INT =
    (SELECT TOP (1) id FROM dbo.facultad
      WHERE nombre = N'Facultad de Ingeniería' AND universidad = 2 ORDER BY id);

-- Programas existentes en la Facultad; sólo se renombran los fixtures de prueba.
IF EXISTS (SELECT 1 FROM dbo.programa WHERE id = 9001 AND nombre = N'Programa de prueba Felipe')
    UPDATE dbo.programa
       SET nombre = N'Ingeniería Química', facultad = @facultadUdea, ciudad = N'Medellín'
     WHERE id = 9001;

IF EXISTS (SELECT 1 FROM dbo.programa WHERE id = 9002 AND nombre = N'Programa secundario de prueba')
    UPDATE dbo.programa
       SET nombre = N'Ingeniería Electrónica', facultad = @facultadUdea, ciudad = N'Medellín'
     WHERE id = 9002;

-- La base inicial no contiene programas: se crean ejemplos con IDs generados.
IF NOT EXISTS (SELECT 1 FROM dbo.programa WHERE nombre = N'Ingeniería Química' AND facultad = @facultadUdea)
    INSERT INTO dbo.programa
        (nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes,
         cant_graduados, fecha_actualizacion, ciudad, facultad, activo)
    VALUES
        (N'Ingeniería Química', N'Universitario', N'Pregrado', N'Vigente', NULL,
         N'Sin dato', N'Sin dato', N'2026-10-16', N'Medellín', @facultadUdea, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.programa WHERE nombre = N'Ingeniería Electrónica' AND facultad = @facultadUdea)
    INSERT INTO dbo.programa
        (nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes,
         cant_graduados, fecha_actualizacion, ciudad, facultad, activo)
    VALUES
        (N'Ingeniería Electrónica', N'Universitario', N'Pregrado', N'Vigente', NULL,
         N'Sin dato', N'Sin dato', N'2026-10-16', N'Medellín', @facultadUdea, 1);

DECLARE @programaQuimica INT =
    (SELECT TOP (1) id FROM dbo.programa
      WHERE nombre = N'Ingeniería Química' AND facultad = @facultadUdea ORDER BY id);

-- Ejemplos documentados asociados a Ingeniería Química en la UdeA.
IF EXISTS
   (SELECT 1 FROM dbo.pasantia
     WHERE nombre = N'Verano de Investigación Programa Delfín' AND programa = @programaQuimica)
    UPDATE dbo.pasantia
       SET pais = N'Colombia',
           empresa = N'Universidad de Antioquia',
           descripcion = N'Estancia de investigación de siete semanas'
     WHERE nombre = N'Verano de Investigación Programa Delfín'
       AND programa = @programaQuimica;

IF NOT EXISTS
   (SELECT 1 FROM dbo.pasantia
     WHERE nombre = N'Verano de Investigación Programa Delfín' AND programa = @programaQuimica)
    INSERT INTO dbo.pasantia (nombre, pais, empresa, descripcion, programa, activo)
    VALUES (N'Verano de Investigación Programa Delfín', N'Colombia', N'Universidad de Antioquia',
            N'Estancia de investigación de siete semanas', @programaQuimica, 1);

IF EXISTS
   (SELECT 1 FROM dbo.premio
     WHERE nombre = N'Premios Medellín Investiga 2025' AND programa = @programaQuimica)
    UPDATE dbo.premio
       SET nombre = N'Premio Investigación Estudiantil UdeA 2025',
           descripcion = N'Remoción de N y P con microalgas',
           fecha = '2025-09-16',
           entidad_otorgante = N'Universidad de Antioquia',
           pais = N'Colombia'
     WHERE nombre = N'Premios Medellín Investiga 2025'
       AND programa = @programaQuimica;

IF NOT EXISTS
   (SELECT 1 FROM dbo.premio
     WHERE nombre = N'Premio Investigación Estudiantil UdeA 2025' AND programa = @programaQuimica)
    INSERT INTO dbo.premio
        (nombre, descripcion, fecha, entidad_otorgante, pais, programa, activo)
    VALUES
        (N'Premio Investigación Estudiantil UdeA 2025', N'Remoción de N y P con microalgas',
         '2025-09-16', N'Universidad de Antioquia', N'Colombia', @programaQuimica, 1);

COMMIT TRANSACTION;
GO
