-- =============================================================================
-- 03_activar_borrado_logico_v2.sql
-- Agrega la columna `activo` (BIT NOT NULL DEFAULT 1) a las 15 tablas con clave
-- foránea de la v2 y la columna del disparador `programa.num_acreditaciones`.
--
-- Seguro de repetir: IF COL_LENGTH(...) IS NULL evita duplicar la columna. El
-- script provisto (innovacion_curricular.ss.sql) no se modifica.
-- Fuente: docs/spec_kit/versiones/v2/5_data_model.md, Secciones 3 y 4.
--
-- Las 7 tablas sin FK de la v1 ya reciben `activo` en 01_activar_borrado_logico.sql.
-- =============================================================================

USE [innovacion_curricular];
GO

-- --- Dependencia raíz (Emmanuel) -------------------------------------------
IF COL_LENGTH('facultad', 'activo') IS NULL
    ALTER TABLE facultad ADD activo BIT NOT NULL DEFAULT 1;
GO

-- --- Maestro y bloque normativo (Valentina) --------------------------------
IF COL_LENGTH('programa', 'activo') IS NULL
    ALTER TABLE programa ADD activo BIT NOT NULL DEFAULT 1;
GO

IF COL_LENGTH('acreditacion', 'activo') IS NULL
    ALTER TABLE acreditacion ADD activo BIT NOT NULL DEFAULT 1;
GO

IF COL_LENGTH('registro_calificado', 'activo') IS NULL
    ALTER TABLE registro_calificado ADD activo BIT NOT NULL DEFAULT 1;
GO

IF COL_LENGTH('activ_academica', 'activo') IS NULL
    ALTER TABLE activ_academica ADD activo BIT NOT NULL DEFAULT 1;
GO

-- --- Detalle de programa (Emmanuel) ----------------------------------------
IF COL_LENGTH('pasantia', 'activo') IS NULL
    ALTER TABLE pasantia ADD activo BIT NOT NULL DEFAULT 1;
GO

IF COL_LENGTH('premio', 'activo') IS NULL
    ALTER TABLE premio ADD activo BIT NOT NULL DEFAULT 1;
GO

-- --- Tablas puente (Felipe / Valentina) ------------------------------------
IF COL_LENGTH('programa_ac', 'activo') IS NULL
    ALTER TABLE programa_ac ADD activo BIT NOT NULL DEFAULT 1;
GO

IF COL_LENGTH('programa_pe', 'activo') IS NULL
    ALTER TABLE programa_pe ADD activo BIT NOT NULL DEFAULT 1;
GO

IF COL_LENGTH('programa_ci', 'activo') IS NULL
    ALTER TABLE programa_ci ADD activo BIT NOT NULL DEFAULT 1;
GO

IF COL_LENGTH('an_programa', 'activo') IS NULL
    ALTER TABLE an_programa ADD activo BIT NOT NULL DEFAULT 1;
GO

IF COL_LENGTH('enfoque_rc', 'activo') IS NULL
    ALTER TABLE enfoque_rc ADD activo BIT NOT NULL DEFAULT 1;
GO

IF COL_LENGTH('aa_rc', 'activo') IS NULL
    ALTER TABLE aa_rc ADD activo BIT NOT NULL DEFAULT 1;
GO

-- --- Personas y aliados (Felipe) -------------------------------------------
IF COL_LENGTH('docente_departamento', 'activo') IS NULL
    ALTER TABLE docente_departamento ADD activo BIT NOT NULL DEFAULT 1;
GO

IF COL_LENGTH('alianza', 'activo') IS NULL
    ALTER TABLE alianza ADD activo BIT NOT NULL DEFAULT 1;
GO

-- --- Columna del disparador (estado derivado) ------------------------------
-- La mantiene el disparador trg_acreditacion_contador; la API sólo la lee.
-- Fuente: docs/spec_kit/versiones/v2/5_data_model.md, Sección 4.1.
IF COL_LENGTH('programa', 'num_acreditaciones') IS NULL
    ALTER TABLE programa ADD num_acreditaciones INT NOT NULL DEFAULT 0;
GO
