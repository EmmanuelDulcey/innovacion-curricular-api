-- =============================================================================
-- 08_seeders.sql
-- Completa los datos de demostración para que TODAS las tablas tengan al menos
-- 5 filas activas (las que ya tienen 5 o más no se tocan):
--   * area_conocimiento (218), universidad (6) y car_innovacion (7) ya vienen
--     completas de 02_datos_iniciales.sql y 06_datos_demostracion.sql.
--
-- Características:
--   * Idempotente: cada bloque comprueba su clave natural (o su PK) antes de
--     insertar; se puede ejecutar varias veces sin duplicar filas.
--   * No borra ni actualiza filas existentes.
--   * Corre al final de db/init.sh, después de 07_docente.sql, porque usa la
--     tabla docente y las secuencias de SQL Server.
--   * Usa las secuencias existentes (no escribe IDs a mano en las tablas que
--     ya las tienen) y resuelve las claves foráneas por nombre, para no
--     depender de los IDs que la base haya generado.
-- =============================================================================
USE [innovacion_curricular];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- =============================================================================
-- 1. facultad (mínimo 5 activas)
--    La Facultad de Ingeniería ya la crea 06_datos_demostracion.sql; aquí se
--    agregan cuatro facultades más de la misma universidad (UdeA, id 2).
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM facultad WHERE nombre = N'Facultad de Ingeniería' AND universidad = 2)
    INSERT INTO facultad (nombre, tipo, fecha_fun, universidad, activo)
    VALUES (N'Facultad de Ingeniería', N'Pública', '1958-01-01', 2, 1);

IF NOT EXISTS (SELECT 1 FROM facultad WHERE nombre = N'Facultad de Ciencias Exactas y Naturales' AND universidad = 2)
    INSERT INTO facultad (nombre, tipo, fecha_fun, universidad, activo)
    VALUES (N'Facultad de Ciencias Exactas y Naturales', N'Pública', '1965-03-01', 2, 1);

IF NOT EXISTS (SELECT 1 FROM facultad WHERE nombre = N'Facultad de Ciencias Económicas' AND universidad = 2)
    INSERT INTO facultad (nombre, tipo, fecha_fun, universidad, activo)
    VALUES (N'Facultad de Ciencias Económicas', N'Pública', '1944-02-01', 2, 1);

IF NOT EXISTS (SELECT 1 FROM facultad WHERE nombre = N'Facultad de Ciencias de la Salud' AND universidad = 2)
    INSERT INTO facultad (nombre, tipo, fecha_fun, universidad, activo)
    VALUES (N'Facultad de Ciencias de la Salud', N'Pública', '1950-06-01', 2, 1);

IF NOT EXISTS (SELECT 1 FROM facultad WHERE nombre = N'Facultad de Ciencias Sociales y Humanas' AND universidad = 2)
    INSERT INTO facultad (nombre, tipo, fecha_fun, universidad, activo)
    VALUES (N'Facultad de Ciencias Sociales y Humanas', N'Pública', '1968-08-01', 2, 1);

DECLARE @facIngenieria INT = (SELECT TOP (1) id FROM facultad WHERE nombre = N'Facultad de Ingeniería' AND universidad = 2 ORDER BY id);
DECLARE @facCiencias   INT = (SELECT TOP (1) id FROM facultad WHERE nombre = N'Facultad de Ciencias Exactas y Naturales' AND universidad = 2 ORDER BY id);
DECLARE @facEconomicas INT = (SELECT TOP (1) id FROM facultad WHERE nombre = N'Facultad de Ciencias Económicas' AND universidad = 2 ORDER BY id);
DECLARE @facSalud      INT = (SELECT TOP (1) id FROM facultad WHERE nombre = N'Facultad de Ciencias de la Salud' AND universidad = 2 ORDER BY id);
DECLARE @facSociales   INT = (SELECT TOP (1) id FROM facultad WHERE nombre = N'Facultad de Ciencias Sociales y Humanas' AND universidad = 2 ORDER BY id);

-- =============================================================================
-- 2. programa (mínimo 5 activos)
--    La base ya trae Ingeniería Química e Ingeniería Electrónica; se agregan
--    cinco programas para repartir el detalle entre facultades distintas.
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM programa WHERE nombre = N'Ingeniería de Sistemas' AND facultad = @facIngenieria)
    INSERT INTO programa (nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes,
                          cant_graduados, fecha_actualizacion, ciudad, facultad, activo)
    VALUES (N'Ingeniería de Sistemas', N'Universitario', N'Pregrado', N'2001-01-15', NULL, N'24',
            N'1250', N'2026-01-20', N'Medellín', @facIngenieria, 1);

IF NOT EXISTS (SELECT 1 FROM programa WHERE nombre = N'Administración de Empresas' AND facultad = @facEconomicas)
    INSERT INTO programa (nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes,
                          cant_graduados, fecha_actualizacion, ciudad, facultad, activo)
    VALUES (N'Administración de Empresas', N'Universitario', N'Pregrado', N'1962-02-01', NULL, N'30',
            N'2100', N'2026-01-20', N'Medellín', @facEconomicas, 1);

IF NOT EXISTS (SELECT 1 FROM programa WHERE nombre = N'Medicina' AND facultad = @facSalud)
    INSERT INTO programa (nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes,
                          cant_graduados, fecha_actualizacion, ciudad, facultad, activo)
    VALUES (N'Medicina', N'Universitario', N'Pregrado', N'1950-06-01', NULL, N'18',
            N'980', N'2026-01-20', N'Medellín', @facSalud, 1);

IF NOT EXISTS (SELECT 1 FROM programa WHERE nombre = N'Derecho' AND facultad = @facSociales)
    INSERT INTO programa (nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes,
                          cant_graduados, fecha_actualizacion, ciudad, facultad, activo)
    VALUES (N'Derecho', N'Universitario', N'Pregrado', N'1968-08-01', NULL, N'20',
            N'1450', N'2026-01-20', N'Medellín', @facSociales, 1);

IF NOT EXISTS (SELECT 1 FROM programa WHERE nombre = N'Biología' AND facultad = @facCiencias)
    INSERT INTO programa (nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes,
                          cant_graduados, fecha_actualizacion, ciudad, facultad, activo)
    VALUES (N'Biología', N'Universitario', N'Pregrado', N'1965-03-01', NULL, N'14',
            N'620', N'2026-01-20', N'Medellín', @facCiencias, 1);

DECLARE @progSistemas INT = (SELECT TOP (1) id FROM programa WHERE nombre = N'Ingeniería de Sistemas' AND facultad = @facIngenieria ORDER BY id);
DECLARE @progAdmin    INT = (SELECT TOP (1) id FROM programa WHERE nombre = N'Administración de Empresas' AND facultad = @facEconomicas ORDER BY id);
DECLARE @progMedicina INT = (SELECT TOP (1) id FROM programa WHERE nombre = N'Medicina' AND facultad = @facSalud ORDER BY id);
DECLARE @progDerecho  INT = (SELECT TOP (1) id FROM programa WHERE nombre = N'Derecho' AND facultad = @facSociales ORDER BY id);
DECLARE @progBiologia INT = (SELECT TOP (1) id FROM programa WHERE nombre = N'Biología' AND facultad = @facCiencias ORDER BY id);

-- =============================================================================
-- 3. aspecto_normativo (mínimo 5)
--    La base ya trae la Ley 115 de 1994 y el Decreto 1075 de 2015.
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM aspecto_normativo WHERE descripcion = N'Ley 30 de 1992')
    INSERT INTO aspecto_normativo (tipo, descripcion, fuente, activo)
    VALUES (N'Ley', N'Ley 30 de 1992', N'Congreso de la República', 1);

IF NOT EXISTS (SELECT 1 FROM aspecto_normativo WHERE descripcion = N'Decreto 1330 de 2019')
    INSERT INTO aspecto_normativo (tipo, descripcion, fuente, activo)
    VALUES (N'Decreto', N'Decreto 1330 de 2019', N'Ministerio de Educación Nacional', 1);

IF NOT EXISTS (SELECT 1 FROM aspecto_normativo WHERE descripcion = N'Resolución 015224 de 2020')
    INSERT INTO aspecto_normativo (tipo, descripcion, fuente, activo)
    VALUES (N'Resolución', N'Resolución 015224 de 2020', N'Ministerio de Educación Nacional', 1);

DECLARE @normaLey115    INT = (SELECT TOP (1) id FROM aspecto_normativo WHERE descripcion = N'Ley 115 de 1994' ORDER BY id);
DECLARE @normaDecreto1075 INT = (SELECT TOP (1) id FROM aspecto_normativo WHERE descripcion = N'Decreto 1075 de 2015' ORDER BY id);
DECLARE @normaLey30     INT = (SELECT TOP (1) id FROM aspecto_normativo WHERE descripcion = N'Ley 30 de 1992' ORDER BY id);
DECLARE @normaDecreto1330 INT = (SELECT TOP (1) id FROM aspecto_normativo WHERE descripcion = N'Decreto 1330 de 2019' ORDER BY id);
DECLARE @normaResolucion INT = (SELECT TOP (1) id FROM aspecto_normativo WHERE descripcion = N'Resolución 015224 de 2020' ORDER BY id);

-- =============================================================================
-- 4. practica_estrategia (mínimo 5)
--    La base ya trae Aprendizaje basado en proyectos y Aula invertida.
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM practica_estrategia WHERE nombre = N'Aprendizaje servicio')
    INSERT INTO practica_estrategia (tipo, nombre, descripcion, activo)
    VALUES (N'Aprendizaje experiencial', N'Aprendizaje servicio', N'Resolver necesidades reales del entorno', 1);

IF NOT EXISTS (SELECT 1 FROM practica_estrategia WHERE nombre = N'Gamificación')
    INSERT INTO practica_estrategia (tipo, nombre, descripcion, activo)
    VALUES (N'Estrategia didáctica', N'Gamificación', N'Juegos y retos para motivar el aprendizaje', 1);

IF NOT EXISTS (SELECT 1 FROM practica_estrategia WHERE nombre = N'Seminario alemán')
    INSERT INTO practica_estrategia (tipo, nombre, descripcion, activo)
    VALUES (N'Estrategia didáctica', N'Seminario alemán', N'Discusión guiada de lecturas académicas', 1);

DECLARE @peProyectos INT = (SELECT TOP (1) id FROM practica_estrategia WHERE nombre = N'Aprendizaje basado en proyectos' ORDER BY id);
DECLARE @peAula      INT = (SELECT TOP (1) id FROM practica_estrategia WHERE nombre = N'Aula invertida' ORDER BY id);
DECLARE @peServicio  INT = (SELECT TOP (1) id FROM practica_estrategia WHERE nombre = N'Aprendizaje servicio' ORDER BY id);
DECLARE @peGamifica  INT = (SELECT TOP (1) id FROM practica_estrategia WHERE nombre = N'Gamificación' ORDER BY id);
DECLARE @peSeminario INT = (SELECT TOP (1) id FROM practica_estrategia WHERE nombre = N'Seminario alemán' ORDER BY id);

-- =============================================================================
-- 5. enfoque (mínimo 5; esta tabla no trae datos de fábrica)
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM enfoque WHERE nombre = N'Enfoque por competencias')
    INSERT INTO enfoque (nombre, descripcion, activo)
    VALUES (N'Enfoque por competencias', N'Formar capacidades para el desempeño laboral', 1);

IF NOT EXISTS (SELECT 1 FROM enfoque WHERE nombre = N'Enfoque investigativo')
    INSERT INTO enfoque (nombre, descripcion, activo)
    VALUES (N'Enfoque investigativo', N'Integrar la indagación en la formación', 1);

IF NOT EXISTS (SELECT 1 FROM enfoque WHERE nombre = N'Enfoque socioformativo')
    INSERT INTO enfoque (nombre, descripcion, activo)
    VALUES (N'Enfoque socioformativo', N'Resolver problemas del contexto con ética', 1);

IF NOT EXISTS (SELECT 1 FROM enfoque WHERE nombre = N'Enfoque inclusivo')
    INSERT INTO enfoque (nombre, descripcion, activo)
    VALUES (N'Enfoque inclusivo', N'Reconocer la diversidad y eliminar barreras', 1);

IF NOT EXISTS (SELECT 1 FROM enfoque WHERE nombre = N'Enfoque intercultural')
    INSERT INTO enfoque (nombre, descripcion, activo)
    VALUES (N'Enfoque intercultural', N'Dialogar entre saberes y culturas', 1);

DECLARE @enfCompetencias  INT = (SELECT TOP (1) id FROM enfoque WHERE nombre = N'Enfoque por competencias' ORDER BY id);
DECLARE @enfInvestigativo INT = (SELECT TOP (1) id FROM enfoque WHERE nombre = N'Enfoque investigativo' ORDER BY id);
DECLARE @enfSocioformativo INT = (SELECT TOP (1) id FROM enfoque WHERE nombre = N'Enfoque socioformativo' ORDER BY id);
DECLARE @enfInclusivo     INT = (SELECT TOP (1) id FROM enfoque WHERE nombre = N'Enfoque inclusivo' ORDER BY id);
DECLARE @enfIntercultural INT = (SELECT TOP (1) id FROM enfoque WHERE nombre = N'Enfoque intercultural' ORDER BY id);

-- =============================================================================
-- 6. aliado (mínimo 5)
--    La base ya trae Empresas Públicas de Medellín E.S.P. (NIT 890904996).
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM aliado WHERE nit = 890980040)
    INSERT INTO aliado (nit, razon_social, nombre_contacto, correo, telefono, ciudad, activo)
    VALUES (890980040, N'Universidad de Antioquia', N'Atención al ciudadano',
            N'atencionciudadano@udea.edu.co', N'604 219 8332', N'Medellín', 1);

IF NOT EXISTS (SELECT 1 FROM aliado WHERE nit = 860015503)
    INSERT INTO aliado (nit, razon_social, nombre_contacto, correo, telefono, ciudad, activo)
    VALUES (860015503, N'Universidad Nacional de Colombia', N'Oficina de Comunicaciones',
            N'comunicaciones@unal.edu.co', N'601 316 5000', N'Bogotá', 1);

IF NOT EXISTS (SELECT 1 FROM aliado WHERE nit = 890900608)
    INSERT INTO aliado (nit, razon_social, nombre_contacto, correo, telefono, ciudad, activo)
    VALUES (890900608, N'Alcaldía de Medellín', N'Atención a la ciudadanía',
            N'atencion@medellin.gov.co', N'604 385 5555', N'Medellín', 1);

IF NOT EXISTS (SELECT 1 FROM aliado WHERE nit = 890902653)
    INSERT INTO aliado (nit, razon_social, nombre_contacto, correo, telefono, ciudad, activo)
    VALUES (890902653, N'Universidad Pontificia Bolivariana', N'Dirección de Extensión',
            N'extension@upb.edu.co', N'604 354 4522', N'Medellín', 1);

-- =============================================================================
-- 7. acreditacion (mínimo 5 activas)
--    Cada programa nuevo recibe una acreditación. El disparador
--    trg_acreditacion_contador actualiza solo programa.num_acreditaciones.
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM acreditacion WHERE resolucion = 90001)
    INSERT INTO acreditacion (resolucion, tipo, calificacion, fecha_inicio, fecha_fin, programa, activo)
    VALUES (90001, N'Nacional', N'Alta', N'2021-01-01', N'2027-12-31', @progSistemas, 1);

IF NOT EXISTS (SELECT 1 FROM acreditacion WHERE resolucion = 90002)
    INSERT INTO acreditacion (resolucion, tipo, calificacion, fecha_inicio, fecha_fin, programa, activo)
    VALUES (90002, N'Nacional', N'Alta', N'2020-07-01', N'2026-06-30', @progAdmin, 1);

IF NOT EXISTS (SELECT 1 FROM acreditacion WHERE resolucion = 90003)
    INSERT INTO acreditacion (resolucion, tipo, calificacion, fecha_inicio, fecha_fin, programa, activo)
    VALUES (90003, N'Nacional', N'Superior', N'2022-02-01', N'2030-01-31', @progMedicina, 1);

IF NOT EXISTS (SELECT 1 FROM acreditacion WHERE resolucion = 90004)
    INSERT INTO acreditacion (resolucion, tipo, calificacion, fecha_inicio, fecha_fin, programa, activo)
    VALUES (90004, N'Nacional', N'Alta', N'2021-09-01', N'2027-08-31', @progDerecho, 1);

IF NOT EXISTS (SELECT 1 FROM acreditacion WHERE resolucion = 90005)
    INSERT INTO acreditacion (resolucion, tipo, calificacion, fecha_inicio, fecha_fin, programa, activo)
    VALUES (90005, N'Internacional', N'Superior', N'2023-03-01', N'2029-02-28', @progBiologia, 1);

-- =============================================================================
-- 8. registro_calificado (mínimo 5 activos)
--    Un registro por programa nuevo. Las fechas son DATE (no texto).
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM registro_calificado WHERE codigo = 90001)
    INSERT INTO registro_calificado (codigo, cant_creditos, hora_acom, hora_ind, metodologia,
                                     fecha_inicio, fecha_fin, duracion_anios, duracion_semestres,
                                     tipo_titulacion, programa, activo)
    VALUES (90001, N'160', N'20', N'30', N'Presencial', '2020-01-15', '2026-01-15',
            N'5', N'10', N'Profesional', @progSistemas, 1);

IF NOT EXISTS (SELECT 1 FROM registro_calificado WHERE codigo = 90002)
    INSERT INTO registro_calificado (codigo, cant_creditos, hora_acom, hora_ind, metodologia,
                                     fecha_inicio, fecha_fin, duracion_anios, duracion_semestres,
                                     tipo_titulacion, programa, activo)
    VALUES (90002, N'150', N'18', N'27', N'Presencial', '2021-01-15', '2025-01-15',
            N'4', N'8', N'Profesional', @progAdmin, 1);

IF NOT EXISTS (SELECT 1 FROM registro_calificado WHERE codigo = 90003)
    INSERT INTO registro_calificado (codigo, cant_creditos, hora_acom, hora_ind, metodologia,
                                     fecha_inicio, fecha_fin, duracion_anios, duracion_semestres,
                                     tipo_titulacion, programa, activo)
    VALUES (90003, N'240', N'30', N'40', N'Presencial', '2022-01-15', '2028-01-15',
            N'6', N'12', N'Profesional', @progMedicina, 1);

IF NOT EXISTS (SELECT 1 FROM registro_calificado WHERE codigo = 90004)
    INSERT INTO registro_calificado (codigo, cant_creditos, hora_acom, hora_ind, metodologia,
                                     fecha_inicio, fecha_fin, duracion_anios, duracion_semestres,
                                     tipo_titulacion, programa, activo)
    VALUES (90004, N'170', N'20', N'30', N'Presencial', '2021-07-01', '2026-06-30',
            N'5', N'10', N'Profesional', @progDerecho, 1);

IF NOT EXISTS (SELECT 1 FROM registro_calificado WHERE codigo = 90005)
    INSERT INTO registro_calificado (codigo, cant_creditos, hora_acom, hora_ind, metodologia,
                                     fecha_inicio, fecha_fin, duracion_anios, duracion_semestres,
                                     tipo_titulacion, programa, activo)
    VALUES (90005, N'165', N'22', N'33', N'Presencial', '2023-01-15', '2028-01-15',
            N'5', N'10', N'Profesional', @progBiologia, 1);

-- =============================================================================
-- 9. activ_academica (mínimo 5 activas)
--    Una actividad por programa; `disenio` es la FK opcional a programa.
--    La última es espejo para dejar un ejemplo con espejo = 1.
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM activ_academica WHERE nombre = N'Programación Orientada a Objetos')
    INSERT INTO activ_academica (nombre, num_creditos, tipo, area_formacion, h_acom, h_indep,
                                 idioma, espejo, entidad_espejo, pais_espejo, disenio, activo)
    VALUES (N'Programación Orientada a Objetos', 4, N'Curso', N'Profesional', 64, 128,
            N'Español', 0, N'No aplica', N'Colombia', @progSistemas, 1);

IF NOT EXISTS (SELECT 1 FROM activ_academica WHERE nombre = N'Fundamentos de Economía')
    INSERT INTO activ_academica (nombre, num_creditos, tipo, area_formacion, h_acom, h_indep,
                                 idioma, espejo, entidad_espejo, pais_espejo, disenio, activo)
    VALUES (N'Fundamentos de Economía', 3, N'Curso', N'Básico', 48, 96,
            N'Español', 0, N'No aplica', N'Colombia', @progAdmin, 1);

IF NOT EXISTS (SELECT 1 FROM activ_academica WHERE nombre = N'Anatomía Humana')
    INSERT INTO activ_academica (nombre, num_creditos, tipo, area_formacion, h_acom, h_indep,
                                 idioma, espejo, entidad_espejo, pais_espejo, disenio, activo)
    VALUES (N'Anatomía Humana', 5, N'Curso', N'Disciplinar', 80, 160,
            N'Español', 0, N'No aplica', N'Colombia', @progMedicina, 1);

IF NOT EXISTS (SELECT 1 FROM activ_academica WHERE nombre = N'Derecho Constitucional')
    INSERT INTO activ_academica (nombre, num_creditos, tipo, area_formacion, h_acom, h_indep,
                                 idioma, espejo, entidad_espejo, pais_espejo, disenio, activo)
    VALUES (N'Derecho Constitucional', 4, N'Curso', N'Profesional', 64, 128,
            N'Español', 0, N'No aplica', N'Colombia', @progDerecho, 1);

IF NOT EXISTS (SELECT 1 FROM activ_academica WHERE nombre = N'Biología Celular')
    INSERT INTO activ_academica (nombre, num_creditos, tipo, area_formacion, h_acom, h_indep,
                                 idioma, espejo, entidad_espejo, pais_espejo, disenio, activo)
    VALUES (N'Biología Celular', 4, N'Curso', N'Básico', 64, 128,
            N'Español', 1, N'Universidad de Barcelona', N'España', @progBiologia, 1);

DECLARE @aaProgramacion INT = (SELECT TOP (1) id FROM activ_academica WHERE nombre = N'Programación Orientada a Objetos' ORDER BY id);
DECLARE @aaEconomia     INT = (SELECT TOP (1) id FROM activ_academica WHERE nombre = N'Fundamentos de Economía' ORDER BY id);
DECLARE @aaAnatomia     INT = (SELECT TOP (1) id FROM activ_academica WHERE nombre = N'Anatomía Humana' ORDER BY id);
DECLARE @aaConstitucional INT = (SELECT TOP (1) id FROM activ_academica WHERE nombre = N'Derecho Constitucional' ORDER BY id);
DECLARE @aaBiologia    INT = (SELECT TOP (1) id FROM activ_academica WHERE nombre = N'Biología Celular' ORDER BY id);

-- =============================================================================
-- 10. pasantia (mínimo 5 activas)
--     La base ya trae el Verano de Investigación Programa Delfín.
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM pasantia WHERE nombre = N'Pasantía de Investigación en Sistemas' AND programa = @progSistemas)
    INSERT INTO pasantia (nombre, pais, empresa, descripcion, programa, activo)
    VALUES (N'Pasantía de Investigación en Sistemas', N'Colombia', N'Universidad de Antioquia',
            N'Apoyo a proyectos del grupo de investigación', @progSistemas, 1);

IF NOT EXISTS (SELECT 1 FROM pasantia WHERE nombre = N'Pasantía Empresarial en Gestión' AND programa = @progAdmin)
    INSERT INTO pasantia (nombre, pais, empresa, descripcion, programa, activo)
    VALUES (N'Pasantía Empresarial en Gestión', N'Colombia', N'Cámara de Comercio de Medellín',
            N'Acompañamiento en procesos administrativos', @progAdmin, 1);

IF NOT EXISTS (SELECT 1 FROM pasantia WHERE nombre = N'Internado Rotatorio' AND programa = @progMedicina)
    INSERT INTO pasantia (nombre, pais, empresa, descripcion, programa, activo)
    VALUES (N'Internado Rotatorio', N'Colombia', N'Hospital Universitario San Vicente',
            N'Rotación clínica en servicios hospitalarios', @progMedicina, 1);

IF NOT EXISTS (SELECT 1 FROM pasantia WHERE nombre = N'Práctica Jurídica' AND programa = @progDerecho)
    INSERT INTO pasantia (nombre, pais, empresa, descripcion, programa, activo)
    VALUES (N'Práctica Jurídica', N'Colombia', N'Personería de Medellín',
            N'Apoyo en atención y orientación jurídica', @progDerecho, 1);

-- =============================================================================
-- 11. premio (mínimo 5 activos)
--     La base ya trae el Premio Investigación Estudiantil UdeA 2025.
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM premio WHERE nombre = N'Premio a la Excelencia Académica' AND programa = @progSistemas)
    INSERT INTO premio (nombre, descripcion, fecha, entidad_otorgante, pais, programa, activo)
    VALUES (N'Premio a la Excelencia Académica', N'Mejor promedio del programa', '2025-11-20',
            N'Universidad de Antioquia', N'Colombia', @progSistemas, 1);

IF NOT EXISTS (SELECT 1 FROM premio WHERE nombre = N'Premio Innovación Empresarial' AND programa = @progAdmin)
    INSERT INTO premio (nombre, descripcion, fecha, entidad_otorgante, pais, programa, activo)
    VALUES (N'Premio Innovación Empresarial', N'Mejor propuesta de negocio', '2025-10-15',
            N'Cámara de Comercio de Medellín', N'Colombia', @progAdmin, 1);

IF NOT EXISTS (SELECT 1 FROM premio WHERE nombre = N'Premio Investigación Médica' AND programa = @progMedicina)
    INSERT INTO premio (nombre, descripcion, fecha, entidad_otorgante, pais, programa, activo)
    VALUES (N'Premio Investigación Médica', N'Estudio clínico destacado', '2025-09-10',
            N'Academia Nacional de Medicina', N'Colombia', @progMedicina, 1);

IF NOT EXISTS (SELECT 1 FROM premio WHERE nombre = N'Premio Mejor Trabajo Jurídico' AND programa = @progDerecho)
    INSERT INTO premio (nombre, descripcion, fecha, entidad_otorgante, pais, programa, activo)
    VALUES (N'Premio Mejor Trabajo Jurídico', N'Investigación jurídica sobresaliente', '2025-08-05',
            N'Colegio de Abogados de Antioquia', N'Colombia', @progDerecho, 1);

-- =============================================================================
-- 12. Tablas puente de programa (mínimo 5 filas activas cada una)
-- =============================================================================
-- 12.1 programa_ac: cada programa con un área de conocimiento del catálogo.
IF NOT EXISTS (SELECT 1 FROM programa_ac WHERE programa = @progSistemas AND area_conocimiento = 63)
    INSERT INTO programa_ac (programa, area_conocimiento, activo) VALUES (@progSistemas, 63, 1);
IF NOT EXISTS (SELECT 1 FROM programa_ac WHERE programa = @progAdmin AND area_conocimiento = 176)
    INSERT INTO programa_ac (programa, area_conocimiento, activo) VALUES (@progAdmin, 176, 1);
IF NOT EXISTS (SELECT 1 FROM programa_ac WHERE programa = @progMedicina AND area_conocimiento = 193)
    INSERT INTO programa_ac (programa, area_conocimiento, activo) VALUES (@progMedicina, 193, 1);
IF NOT EXISTS (SELECT 1 FROM programa_ac WHERE programa = @progDerecho AND area_conocimiento = 142)
    INSERT INTO programa_ac (programa, area_conocimiento, activo) VALUES (@progDerecho, 142, 1);
IF NOT EXISTS (SELECT 1 FROM programa_ac WHERE programa = @progBiologia AND area_conocimiento = 35)
    INSERT INTO programa_ac (programa, area_conocimiento, activo) VALUES (@progBiologia, 35, 1);

-- 12.2 programa_pe: cada programa con una práctica/estrategia del catálogo.
IF NOT EXISTS (SELECT 1 FROM programa_pe WHERE programa = @progSistemas AND practica_estrategia = @peProyectos)
    INSERT INTO programa_pe (programa, practica_estrategia, activo) VALUES (@progSistemas, @peProyectos, 1);
IF NOT EXISTS (SELECT 1 FROM programa_pe WHERE programa = @progAdmin AND practica_estrategia = @peAula)
    INSERT INTO programa_pe (programa, practica_estrategia, activo) VALUES (@progAdmin, @peAula, 1);
IF NOT EXISTS (SELECT 1 FROM programa_pe WHERE programa = @progMedicina AND practica_estrategia = @peServicio)
    INSERT INTO programa_pe (programa, practica_estrategia, activo) VALUES (@progMedicina, @peServicio, 1);
IF NOT EXISTS (SELECT 1 FROM programa_pe WHERE programa = @progDerecho AND practica_estrategia = @peSeminario)
    INSERT INTO programa_pe (programa, practica_estrategia, activo) VALUES (@progDerecho, @peSeminario, 1);
IF NOT EXISTS (SELECT 1 FROM programa_pe WHERE programa = @progBiologia AND practica_estrategia = @peGamifica)
    INSERT INTO programa_pe (programa, practica_estrategia, activo) VALUES (@progBiologia, @peGamifica, 1);

-- 12.3 programa_ci: cada programa con una característica de innovación curricular.
DECLARE @ciFlexibilidad INT = (SELECT TOP (1) id FROM car_innovacion WHERE nombre = N'Flexibilidad curricular' ORDER BY id);
DECLARE @ciInterdisciplina INT = (SELECT TOP (1) id FROM car_innovacion WHERE nombre = N'Interdisciplinariedad' ORDER BY id);
DECLARE @ciTeoriaPractica INT = (SELECT TOP (1) id FROM car_innovacion WHERE nombre = N'Articulación teoría-práctica' ORDER BY id);
DECLARE @ciIntegral INT = (SELECT TOP (1) id FROM car_innovacion WHERE nombre = N'Formación integral' ORDER BY id);
DECLARE @ciPertinencia INT = (SELECT TOP (1) id FROM car_innovacion WHERE nombre = N'Pertinencia social' ORDER BY id);

IF NOT EXISTS (SELECT 1 FROM programa_ci WHERE programa = @progSistemas AND car_innovacion = @ciFlexibilidad)
    INSERT INTO programa_ci (programa, car_innovacion, activo) VALUES (@progSistemas, @ciFlexibilidad, 1);
IF NOT EXISTS (SELECT 1 FROM programa_ci WHERE programa = @progAdmin AND car_innovacion = @ciInterdisciplina)
    INSERT INTO programa_ci (programa, car_innovacion, activo) VALUES (@progAdmin, @ciInterdisciplina, 1);
IF NOT EXISTS (SELECT 1 FROM programa_ci WHERE programa = @progMedicina AND car_innovacion = @ciTeoriaPractica)
    INSERT INTO programa_ci (programa, car_innovacion, activo) VALUES (@progMedicina, @ciTeoriaPractica, 1);
IF NOT EXISTS (SELECT 1 FROM programa_ci WHERE programa = @progDerecho AND car_innovacion = @ciIntegral)
    INSERT INTO programa_ci (programa, car_innovacion, activo) VALUES (@progDerecho, @ciIntegral, 1);
IF NOT EXISTS (SELECT 1 FROM programa_ci WHERE programa = @progBiologia AND car_innovacion = @ciPertinencia)
    INSERT INTO programa_ci (programa, car_innovacion, activo) VALUES (@progBiologia, @ciPertinencia, 1);

-- 12.4 an_programa: cada programa con un aspecto normativo.
IF NOT EXISTS (SELECT 1 FROM an_programa WHERE aspecto_normativo = @normaLey115 AND programa = @progSistemas)
    INSERT INTO an_programa (aspecto_normativo, programa, activo) VALUES (@normaLey115, @progSistemas, 1);
IF NOT EXISTS (SELECT 1 FROM an_programa WHERE aspecto_normativo = @normaDecreto1075 AND programa = @progAdmin)
    INSERT INTO an_programa (aspecto_normativo, programa, activo) VALUES (@normaDecreto1075, @progAdmin, 1);
IF NOT EXISTS (SELECT 1 FROM an_programa WHERE aspecto_normativo = @normaLey30 AND programa = @progMedicina)
    INSERT INTO an_programa (aspecto_normativo, programa, activo) VALUES (@normaLey30, @progMedicina, 1);
IF NOT EXISTS (SELECT 1 FROM an_programa WHERE aspecto_normativo = @normaDecreto1330 AND programa = @progDerecho)
    INSERT INTO an_programa (aspecto_normativo, programa, activo) VALUES (@normaDecreto1330, @progDerecho, 1);
IF NOT EXISTS (SELECT 1 FROM an_programa WHERE aspecto_normativo = @normaResolucion AND programa = @progBiologia)
    INSERT INTO an_programa (aspecto_normativo, programa, activo) VALUES (@normaResolucion, @progBiologia, 1);

-- =============================================================================
-- 13. Puentes del registro calificado (mínimo 5 filas activas cada una)
-- =============================================================================
-- 13.1 enfoque_rc: un enfoque por registro calificado.
IF NOT EXISTS (SELECT 1 FROM enfoque_rc WHERE enfoque = @enfCompetencias AND registro_calificado = 90001)
    INSERT INTO enfoque_rc (enfoque, registro_calificado, activo) VALUES (@enfCompetencias, 90001, 1);
IF NOT EXISTS (SELECT 1 FROM enfoque_rc WHERE enfoque = @enfInvestigativo AND registro_calificado = 90002)
    INSERT INTO enfoque_rc (enfoque, registro_calificado, activo) VALUES (@enfInvestigativo, 90002, 1);
IF NOT EXISTS (SELECT 1 FROM enfoque_rc WHERE enfoque = @enfSocioformativo AND registro_calificado = 90003)
    INSERT INTO enfoque_rc (enfoque, registro_calificado, activo) VALUES (@enfSocioformativo, 90003, 1);
IF NOT EXISTS (SELECT 1 FROM enfoque_rc WHERE enfoque = @enfInclusivo AND registro_calificado = 90004)
    INSERT INTO enfoque_rc (enfoque, registro_calificado, activo) VALUES (@enfInclusivo, 90004, 1);
IF NOT EXISTS (SELECT 1 FROM enfoque_rc WHERE enfoque = @enfIntercultural AND registro_calificado = 90005)
    INSERT INTO enfoque_rc (enfoque, registro_calificado, activo) VALUES (@enfIntercultural, 90005, 1);

-- 13.2 aa_rc: una actividad académica por registro calificado, con componente
--      y semestre (atributos propios de la pareja).
IF NOT EXISTS (SELECT 1 FROM aa_rc WHERE activ_academicas_idcurso = @aaProgramacion AND registro_calificado_codigo = 90001)
    INSERT INTO aa_rc (activ_academicas_idcurso, registro_calificado_codigo, componente, semestre, activo)
    VALUES (@aaProgramacion, 90001, N'Específico', N'2024-1', 1);
IF NOT EXISTS (SELECT 1 FROM aa_rc WHERE activ_academicas_idcurso = @aaEconomia AND registro_calificado_codigo = 90002)
    INSERT INTO aa_rc (activ_academicas_idcurso, registro_calificado_codigo, componente, semestre, activo)
    VALUES (@aaEconomia, 90002, N'Básico', N'2024-1', 1);
IF NOT EXISTS (SELECT 1 FROM aa_rc WHERE activ_academicas_idcurso = @aaAnatomia AND registro_calificado_codigo = 90003)
    INSERT INTO aa_rc (activ_academicas_idcurso, registro_calificado_codigo, componente, semestre, activo)
    VALUES (@aaAnatomia, 90003, N'Disciplinar', N'2024-2', 1);
IF NOT EXISTS (SELECT 1 FROM aa_rc WHERE activ_academicas_idcurso = @aaConstitucional AND registro_calificado_codigo = 90004)
    INSERT INTO aa_rc (activ_academicas_idcurso, registro_calificado_codigo, componente, semestre, activo)
    VALUES (@aaConstitucional, 90004, N'Específico', N'2024-1', 1);
IF NOT EXISTS (SELECT 1 FROM aa_rc WHERE activ_academicas_idcurso = @aaBiologia AND registro_calificado_codigo = 90005)
    INSERT INTO aa_rc (activ_academicas_idcurso, registro_calificado_codigo, componente, semestre, activo)
    VALUES (@aaBiologia, 90005, N'Básico', N'2024-2', 1);

-- =============================================================================
-- 14. docente (mínimo 5 activos)
--     El id es IDENTITY: no se escribe; se busca por documento.
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM docente WHERE documento = N'1010101010')
    INSERT INTO docente (documento, nombre, activo) VALUES (N'1010101010', N'Ana María Gómez', 1);
IF NOT EXISTS (SELECT 1 FROM docente WHERE documento = N'1020202020')
    INSERT INTO docente (documento, nombre, activo) VALUES (N'1020202020', N'Carlos Andrés Pérez', 1);
IF NOT EXISTS (SELECT 1 FROM docente WHERE documento = N'1030303030')
    INSERT INTO docente (documento, nombre, activo) VALUES (N'1030303030', N'Laura Fernanda Ríos', 1);
IF NOT EXISTS (SELECT 1 FROM docente WHERE documento = N'1040404040')
    INSERT INTO docente (documento, nombre, activo) VALUES (N'1040404040', N'Juan Sebastián Torres', 1);
IF NOT EXISTS (SELECT 1 FROM docente WHERE documento = N'1050505050')
    INSERT INTO docente (documento, nombre, activo) VALUES (N'1050505050', N'Marta Lucía Herrera', 1);

DECLARE @docAna    INT = (SELECT TOP (1) id FROM docente WHERE documento = N'1010101010' ORDER BY id);
DECLARE @docCarlos INT = (SELECT TOP (1) id FROM docente WHERE documento = N'1020202020' ORDER BY id);
DECLARE @docLaura  INT = (SELECT TOP (1) id FROM docente WHERE documento = N'1030303030' ORDER BY id);
DECLARE @docJuan   INT = (SELECT TOP (1) id FROM docente WHERE documento = N'1040404040' ORDER BY id);
DECLARE @docMarta  INT = (SELECT TOP (1) id FROM docente WHERE documento = N'1050505050' ORDER BY id);

-- =============================================================================
-- 15. docente_departamento (mínimo 5 filas activas)
--     La columna `departamento` es la FK a programa (nombre heredado del script).
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM docente_departamento WHERE docente = @docAna AND departamento = @progSistemas)
    INSERT INTO docente_departamento (docente, departamento, dedicacion, modalidad, fecha_ingreso, fecha_salida, activo)
    VALUES (@docAna, @progSistemas, N'Tiempo completo', N'Presencial', '2018-02-01', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM docente_departamento WHERE docente = @docCarlos AND departamento = @progAdmin)
    INSERT INTO docente_departamento (docente, departamento, dedicacion, modalidad, fecha_ingreso, fecha_salida, activo)
    VALUES (@docCarlos, @progAdmin, N'Medio tiempo', N'Híbrida', '2019-07-15', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM docente_departamento WHERE docente = @docLaura AND departamento = @progMedicina)
    INSERT INTO docente_departamento (docente, departamento, dedicacion, modalidad, fecha_ingreso, fecha_salida, activo)
    VALUES (@docLaura, @progMedicina, N'Tiempo completo', N'Presencial', '2015-01-20', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM docente_departamento WHERE docente = @docJuan AND departamento = @progDerecho)
    INSERT INTO docente_departamento (docente, departamento, dedicacion, modalidad, fecha_ingreso, fecha_salida, activo)
    VALUES (@docJuan, @progDerecho, N'Tiempo completo', N'Presencial', '2020-08-10', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM docente_departamento WHERE docente = @docMarta AND departamento = @progBiologia)
    INSERT INTO docente_departamento (docente, departamento, dedicacion, modalidad, fecha_ingreso, fecha_salida, activo)
    VALUES (@docMarta, @progBiologia, N'Medio tiempo', N'Híbrida', '2021-02-01', NULL, 1);

-- =============================================================================
-- 16. alianza (mínimo 5 filas activas)
--     `aliado` es el NIT y `departamento` es la FK a programa.
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM alianza WHERE aliado = 890904996 AND departamento = @progSistemas)
    INSERT INTO alianza (aliado, departamento, fecha_inicio, fecha_fin, docente, activo)
    VALUES (890904996, @progSistemas, '2022-03-01', NULL, @docAna, 1);
IF NOT EXISTS (SELECT 1 FROM alianza WHERE aliado = 890980040 AND departamento = @progAdmin)
    INSERT INTO alianza (aliado, departamento, fecha_inicio, fecha_fin, docente, activo)
    VALUES (890980040, @progAdmin, '2021-08-01', NULL, @docCarlos, 1);
IF NOT EXISTS (SELECT 1 FROM alianza WHERE aliado = 860015503 AND departamento = @progMedicina)
    INSERT INTO alianza (aliado, departamento, fecha_inicio, fecha_fin, docente, activo)
    VALUES (860015503, @progMedicina, '2020-02-01', NULL, @docLaura, 1);
IF NOT EXISTS (SELECT 1 FROM alianza WHERE aliado = 890900608 AND departamento = @progDerecho)
    INSERT INTO alianza (aliado, departamento, fecha_inicio, fecha_fin, docente, activo)
    VALUES (890900608, @progDerecho, '2023-01-15', NULL, @docJuan, 1);
IF NOT EXISTS (SELECT 1 FROM alianza WHERE aliado = 890902653 AND departamento = @progBiologia)
    INSERT INTO alianza (aliado, departamento, fecha_inicio, fecha_fin, docente, activo)
    VALUES (890902653, @progBiologia, '2022-07-01', NULL, @docMarta, 1);

-- =============================================================================
-- 17. Tablas del módulo de usuarios (mínimo 5 filas cada una)
--     Son tablas de la v3: la v2 no tiene login. Las contraseñas son de
--     demostración y deberán guardarse con hash cuando se implemente la v3.
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM rol WHERE nombre = N'Administrador')
    INSERT INTO rol (nombre, descripcion, activo) VALUES (N'Administrador', N'Acceso total al sistema', 1);
IF NOT EXISTS (SELECT 1 FROM rol WHERE nombre = N'Coordinador académico')
    INSERT INTO rol (nombre, descripcion, activo) VALUES (N'Coordinador académico', N'Gestiona programas y su detalle', 1);
IF NOT EXISTS (SELECT 1 FROM rol WHERE nombre = N'Docente')
    INSERT INTO rol (nombre, descripcion, activo) VALUES (N'Docente', N'Consulta la información académica', 1);
IF NOT EXISTS (SELECT 1 FROM rol WHERE nombre = N'Secretaría académica')
    INSERT INTO rol (nombre, descripcion, activo) VALUES (N'Secretaría académica', N'Administra registros y acreditaciones', 1);
IF NOT EXISTS (SELECT 1 FROM rol WHERE nombre = N'Consulta')
    INSERT INTO rol (nombre, descripcion, activo) VALUES (N'Consulta', N'Solo lectura de catálogos', 1);

IF NOT EXISTS (SELECT 1 FROM usuario WHERE username = N'admin')
    INSERT INTO usuario (username, password, email, nombre_completo, activo)
    VALUES (N'admin', N'Demo123!', N'admin@innovacion.edu.co', N'Administrador del sistema', 1);
IF NOT EXISTS (SELECT 1 FROM usuario WHERE username = N'coordinador')
    INSERT INTO usuario (username, password, email, nombre_completo, activo)
    VALUES (N'coordinador', N'Demo123!', N'coordinador@innovacion.edu.co', N'Coordinación académica', 1);
IF NOT EXISTS (SELECT 1 FROM usuario WHERE username = N'docente.demo')
    INSERT INTO usuario (username, password, email, nombre_completo, activo)
    VALUES (N'docente.demo', N'Demo123!', N'docente.demo@innovacion.edu.co', N'Docente de demostración', 1);
IF NOT EXISTS (SELECT 1 FROM usuario WHERE username = N'secretaria')
    INSERT INTO usuario (username, password, email, nombre_completo, activo)
    VALUES (N'secretaria', N'Demo123!', N'secretaria@innovacion.edu.co', N'Secretaría académica', 1);
IF NOT EXISTS (SELECT 1 FROM usuario WHERE username = N'consulta')
    INSERT INTO usuario (username, password, email, nombre_completo, activo)
    VALUES (N'consulta', N'Demo123!', N'consulta@innovacion.edu.co', N'Usuario de consulta', 1);

DECLARE @rolAdmin INT = (SELECT TOP (1) id FROM rol WHERE nombre = N'Administrador' ORDER BY id);
DECLARE @rolCoord INT = (SELECT TOP (1) id FROM rol WHERE nombre = N'Coordinador académico' ORDER BY id);
DECLARE @rolDocente INT = (SELECT TOP (1) id FROM rol WHERE nombre = N'Docente' ORDER BY id);
DECLARE @rolSecretaria INT = (SELECT TOP (1) id FROM rol WHERE nombre = N'Secretaría académica' ORDER BY id);
DECLARE @rolConsulta INT = (SELECT TOP (1) id FROM rol WHERE nombre = N'Consulta' ORDER BY id);

DECLARE @usrAdmin INT = (SELECT TOP (1) id FROM usuario WHERE username = N'admin' ORDER BY id);
DECLARE @usrCoord INT = (SELECT TOP (1) id FROM usuario WHERE username = N'coordinador' ORDER BY id);
DECLARE @usrDocente INT = (SELECT TOP (1) id FROM usuario WHERE username = N'docente.demo' ORDER BY id);
DECLARE @usrSecretaria INT = (SELECT TOP (1) id FROM usuario WHERE username = N'secretaria' ORDER BY id);
DECLARE @usrConsulta INT = (SELECT TOP (1) id FROM usuario WHERE username = N'consulta' ORDER BY id);

IF NOT EXISTS (SELECT 1 FROM rol_usuario WHERE usuario_id = @usrAdmin AND rol_id = @rolAdmin)
    INSERT INTO rol_usuario (usuario_id, rol_id) VALUES (@usrAdmin, @rolAdmin);
IF NOT EXISTS (SELECT 1 FROM rol_usuario WHERE usuario_id = @usrCoord AND rol_id = @rolCoord)
    INSERT INTO rol_usuario (usuario_id, rol_id) VALUES (@usrCoord, @rolCoord);
IF NOT EXISTS (SELECT 1 FROM rol_usuario WHERE usuario_id = @usrDocente AND rol_id = @rolDocente)
    INSERT INTO rol_usuario (usuario_id, rol_id) VALUES (@usrDocente, @rolDocente);
IF NOT EXISTS (SELECT 1 FROM rol_usuario WHERE usuario_id = @usrSecretaria AND rol_id = @rolSecretaria)
    INSERT INTO rol_usuario (usuario_id, rol_id) VALUES (@usrSecretaria, @rolSecretaria);
IF NOT EXISTS (SELECT 1 FROM rol_usuario WHERE usuario_id = @usrConsulta AND rol_id = @rolConsulta)
    INSERT INTO rol_usuario (usuario_id, rol_id) VALUES (@usrConsulta, @rolConsulta);

COMMIT TRANSACTION;
GO

-- =============================================================================
-- Comprobación rápida (opcional, ejecutar a mano):
--   SELECT t.name AS tabla, SUM(p.rows) AS filas
--     FROM sys.tables t
--     JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1)
--    GROUP BY t.name
--    ORDER BY t.name;
-- =============================================================================
