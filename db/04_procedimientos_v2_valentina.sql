-- =============================================================================
-- 04_procedimientos_v2_valentina.sql
-- Procedimientos almacenados de las tablas de la v2 a cargo de Valentina:
--   * programa             (maestro central, FK a facultad)
--   * acreditacion         (detalle de programa, PK resolucion)
--   * registro_calificado  (detalle de programa, PK codigo; fechas DATE)
--   * activ_academica      (detalle de programa via `disenio`, FK OPCIONAL)
--   * enfoque_rc           (puente enfoque <-> registro_calificado)
--   * aa_rc                (puente activ_academica <-> registro_calificado,
--                           con atributos componente y semestre)
-- Y el disparador trg_acreditacion_contador (mantiene programa.num_acreditaciones).
--
-- Juego de procedimientos (3_plan.md §5):
--   PK simple:  sp_listar_, sp_consultar_, sp_crear_, sp_actualizar_, sp_eliminar_
--   Puentes:    sp_listar_, sp_consultar_, sp_asignar_, sp_retirar_
--
-- Validacion de FK DENTRO del procedimiento (guia de Valentina): la restriccion
-- de la base NO detecta una FK retirada (activo = 0), y ademas su error 547 no
-- dice cual de las dos FK de una puente fallo. Por eso cada FK se comprueba
-- antes del INSERT/UPDATE y, si no existe o esta inactiva, se lanza THROW con
-- un numero de error propio (>= 50000):
--   50101 -> programa            (incluye `disenio` de activ_academica)
--   50102 -> facultad
--   50103 -> registro_calificado
--   50104 -> activ_academica
--   50105 -> enfoque
-- Los literales de estos THROW van SIN acentos a proposito: sqlcmd los ejecuta
-- con la codificacion del contenedor (los datos iniciales del repositorio usan
-- la misma convencion). El servicio de C# traduce el numero a su mensaje en
-- castellano CON acentos (el texto exacto de 6_contracts.md); el mensaje del
-- THROW solo se ve al probar el procedimiento a mano.
--
-- Seguro de repetir: CREATE OR ALTER. Todo el SQL de la v2 de Valentina vive
-- aqui (cero SQL en C#). Fuente: docs/spec_kit/versiones/v2/3_plan.md §5-6.
-- =============================================================================

USE [innovacion_curricular];
GO

-- ############################################################################
-- # programa — maestro central (FK a facultad; regla §3.1: no se retira con    #
-- # hijos activos; num_acreditaciones SOLO lo escribe el disparador)           #
-- ############################################################################

CREATE OR ALTER PROCEDURE sp_listar_programa
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite)
           p.id,
           p.nombre,
           p.tipo,
           p.nivel,
           p.fecha_creacion,
           p.fecha_cierre,
           p.numero_cohortes,
           p.cant_graduados,
           p.fecha_actualizacion,
           p.ciudad,
           p.facultad,
           f.nombre AS facultad_nombre,      -- alias para la propiedad FacultadNombre
           p.num_acreditaciones,             -- estado derivado: lo mantiene el disparador
           p.activo
      FROM programa p
      LEFT JOIN facultad f ON f.id = p.facultad
     WHERE p.activo = 1
     ORDER BY p.id;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_programa
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.id,
           p.nombre,
           p.tipo,
           p.nivel,
           p.fecha_creacion,
           p.fecha_cierre,
           p.numero_cohortes,
           p.cant_graduados,
           p.fecha_actualizacion,
           p.ciudad,
           p.facultad,
           f.nombre AS facultad_nombre,
           p.num_acreditaciones,
           p.activo
      FROM programa p
      LEFT JOIN facultad f ON f.id = p.facultad
     WHERE p.id = @id AND p.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_crear_programa
    @nombre             VARCHAR(60),
    @tipo               VARCHAR(45),
    @nivel              VARCHAR(45),
    @fecha_creacion     VARCHAR(45),
    @fecha_cierre       VARCHAR(45),
    @numero_cohortes    VARCHAR(45),
    @cant_graduados     VARCHAR(45),
    @fecha_actualizacion VARCHAR(45),
    @ciudad             VARCHAR(45),
    @facultad           INT
AS
BEGIN
    SET NOCOUNT ON;

    -- La FK se valida aqui y no solo con la restriccion: la base no distingue
    -- una facultad retirada (activo = 0) de una inexistente.
    IF NOT EXISTS (SELECT 1 FROM facultad WHERE id = @facultad AND activo = 1)
        THROW 50102, N'La facultad indicada no existe o esta inactiva.', 1;

    -- El id interno lo genera la secuencia de SQL Server (05_identificadores_
    -- incrementales.sql): el INSERT lo omite y OUTPUT captura el valor asignado
    -- para poder devolver la fila guardada (contrato §4).
    -- `num_acreditaciones` tampoco aparece: lo inicializa su DEFAULT 0 y lo
    -- mantiene el disparador; la API jamas lo escribe.
    DECLARE @id_generado INT;
    DECLARE @insertado TABLE (id INT NOT NULL);

    INSERT INTO programa (nombre, tipo, nivel, fecha_creacion, fecha_cierre,
                          numero_cohortes, cant_graduados, fecha_actualizacion,
                          ciudad, facultad, activo)
    OUTPUT INSERTED.id INTO @insertado (id)
    VALUES (@nombre, @tipo, @nivel, @fecha_creacion, @fecha_cierre,
            @numero_cohortes, @cant_graduados, @fecha_actualizacion,
            @ciudad, @facultad, 1);

    SELECT @id_generado = id FROM @insertado;

    -- Tropiezo 8: se devuelve la fila recien guardada (misma forma que consultar).
    SELECT p.id,
           p.nombre,
           p.tipo,
           p.nivel,
           p.fecha_creacion,
           p.fecha_cierre,
           p.numero_cohortes,
           p.cant_graduados,
           p.fecha_actualizacion,
           p.ciudad,
           p.facultad,
           f.nombre AS facultad_nombre,
           p.num_acreditaciones,
           p.activo
      FROM programa p
      LEFT JOIN facultad f ON f.id = p.facultad
     WHERE p.id = @id_generado;
END
GO

CREATE OR ALTER PROCEDURE sp_actualizar_programa
    @id                 INT,
    @nombre             VARCHAR(60),
    @tipo               VARCHAR(45),
    @nivel              VARCHAR(45),
    @fecha_creacion     VARCHAR(45),
    @fecha_cierre       VARCHAR(45),
    @numero_cohortes    VARCHAR(45),
    @cant_graduados     VARCHAR(45),
    @fecha_actualizacion VARCHAR(45),
    @ciudad             VARCHAR(45),
    @facultad           INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM facultad WHERE id = @facultad AND activo = 1)
        THROW 50102, N'La facultad indicada no existe o esta inactiva.', 1;

    -- `num_acreditaciones` no se toca: es estado derivado del disparador.
    -- `fecha_cierre` puede llegar NULL (es la unica columna opcional).
    UPDATE programa
       SET nombre              = @nombre,
           tipo                = @tipo,
           nivel               = @nivel,
           fecha_creacion      = @fecha_creacion,
           fecha_cierre        = @fecha_cierre,
           numero_cohortes     = @numero_cohortes,
           cant_graduados      = @cant_graduados,
           fecha_actualizacion = @fecha_actualizacion,
           ciudad              = @ciudad,
           facultad            = @facultad
     WHERE id = @id AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

CREATE OR ALTER PROCEDURE sp_eliminar_programa
    @id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Regla §3.1 (2_spec.md): un padre con hijos activos no se retira. El mapa
    -- de hijos es 5_data_model.md §4.2; las puentes (activas) tambien cuentan.
    -- En activ_academica el vinculo con el programa es la columna `disenio`.
    -- Si hay hijos, NO se actualiza ninguna fila: se devuelve 0 y el servicio
    -- lo traduce a 409.
    IF EXISTS (SELECT 1 FROM acreditacion         WHERE programa     = @id AND activo = 1)
    OR EXISTS (SELECT 1 FROM registro_calificado  WHERE programa     = @id AND activo = 1)
    OR EXISTS (SELECT 1 FROM activ_academica      WHERE disenio      = @id AND activo = 1)
    OR EXISTS (SELECT 1 FROM pasantia             WHERE programa     = @id AND activo = 1)
    OR EXISTS (SELECT 1 FROM premio               WHERE programa     = @id AND activo = 1)
    OR EXISTS (SELECT 1 FROM docente_departamento WHERE departamento = @id AND activo = 1)
    OR EXISTS (SELECT 1 FROM alianza              WHERE departamento = @id AND activo = 1)
    OR EXISTS (SELECT 1 FROM programa_ac          WHERE programa     = @id AND activo = 1)
    OR EXISTS (SELECT 1 FROM programa_pe          WHERE programa     = @id AND activo = 1)
    OR EXISTS (SELECT 1 FROM programa_ci          WHERE programa     = @id AND activo = 1)
    OR EXISTS (SELECT 1 FROM an_programa          WHERE programa     = @id AND activo = 1)
    BEGIN
        SELECT 0 AS filas;
        RETURN;
    END

    UPDATE programa
       SET activo = 0
     WHERE id = @id AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

-- ############################################################################
-- # acreditacion — detalle de programa (PK `resolucion`; fechas de TEXTO)      #
-- # Es la tabla que dispara el contador de programa (ver disparador al final). #
-- ############################################################################

CREATE OR ALTER PROCEDURE sp_listar_acreditacion
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite)
           a.resolucion,
           a.tipo,
           a.calificacion,
           a.fecha_inicio,
           a.fecha_fin,
           a.programa,
           pr.nombre AS programa_nombre,
           a.activo
      FROM acreditacion a
      LEFT JOIN programa pr ON pr.id = a.programa
     WHERE a.activo = 1
     ORDER BY a.resolucion;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_acreditacion
    @resolucion INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.resolucion,
           a.tipo,
           a.calificacion,
           a.fecha_inicio,
           a.fecha_fin,
           a.programa,
           pr.nombre AS programa_nombre,
           a.activo
      FROM acreditacion a
      LEFT JOIN programa pr ON pr.id = a.programa
     WHERE a.resolucion = @resolucion AND a.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_crear_acreditacion
    @resolucion   INT,
    @tipo         VARCHAR(45),
    @calificacion VARCHAR(45),
    @fecha_inicio VARCHAR(45),
    @fecha_fin    VARCHAR(45),
    @programa     INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM programa WHERE id = @programa AND activo = 1)
        THROW 50101, N'El programa indicado no existe o esta inactivo.', 1;

    INSERT INTO acreditacion (resolucion, tipo, calificacion, fecha_inicio, fecha_fin, programa, activo)
    VALUES (@resolucion, @tipo, @calificacion, @fecha_inicio, @fecha_fin, @programa, 1);

    -- El disparador trg_acreditacion_contador ya actualizo num_acreditaciones
    -- del programa con este INSERT (AFTER INSERT).
    SELECT a.resolucion,
           a.tipo,
           a.calificacion,
           a.fecha_inicio,
           a.fecha_fin,
           a.programa,
           pr.nombre AS programa_nombre,
           a.activo
      FROM acreditacion a
      LEFT JOIN programa pr ON pr.id = a.programa
     WHERE a.resolucion = @resolucion;
END
GO

CREATE OR ALTER PROCEDURE sp_actualizar_acreditacion
    @resolucion   INT,
    @tipo         VARCHAR(45),
    @calificacion VARCHAR(45),
    @fecha_inicio VARCHAR(45),
    @fecha_fin    VARCHAR(45),
    @programa     INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM programa WHERE id = @programa AND activo = 1)
        THROW 50101, N'El programa indicado no existe o esta inactivo.', 1;

    -- Si el UPDATE cambia `programa`, el disparador recalcula el contador del
    -- programa viejo Y del nuevo (mira inserted UNION deleted).
    UPDATE acreditacion
       SET tipo         = @tipo,
           calificacion = @calificacion,
           fecha_inicio = @fecha_inicio,
           fecha_fin    = @fecha_fin,
           programa     = @programa
     WHERE resolucion = @resolucion AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

CREATE OR ALTER PROCEDURE sp_eliminar_acreditacion
    @resolucion INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Borrado logico: para el disparador esto es un UPDATE (tropiezo 6), y por
    -- eso el contador del programa baja solo.
    UPDATE acreditacion
       SET activo = 0
     WHERE resolucion = @resolucion AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

-- ############################################################################
-- # registro_calificado — detalle de programa (PK `codigo`; fechas DATE)       #
-- # Es padre de las puentes enfoque_rc y aa_rc.                                #
-- ############################################################################

CREATE OR ALTER PROCEDURE sp_listar_registro_calificado
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite)
           r.codigo,
           r.cant_creditos,
           r.hora_acom,
           r.hora_ind,
           r.metodologia,
           r.fecha_inicio,
           r.fecha_fin,
           r.duracion_anios,
           r.duracion_semestres,
           r.tipo_titulacion,
           r.programa,
           pr.nombre AS programa_nombre,
           r.activo
      FROM registro_calificado r
      LEFT JOIN programa pr ON pr.id = r.programa
     WHERE r.activo = 1
     ORDER BY r.codigo;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_registro_calificado
    @codigo INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.codigo,
           r.cant_creditos,
           r.hora_acom,
           r.hora_ind,
           r.metodologia,
           r.fecha_inicio,
           r.fecha_fin,
           r.duracion_anios,
           r.duracion_semestres,
           r.tipo_titulacion,
           r.programa,
           pr.nombre AS programa_nombre,
           r.activo
      FROM registro_calificado r
      LEFT JOIN programa pr ON pr.id = r.programa
     WHERE r.codigo = @codigo AND r.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_crear_registro_calificado
    @codigo              INT,
    @cant_creditos       VARCHAR(45),
    @hora_acom           VARCHAR(45),
    @hora_ind            VARCHAR(45),
    @metodologia         VARCHAR(45),
    @fecha_inicio        DATE,
    @fecha_fin           DATE,
    @duracion_anios      VARCHAR(45),
    @duracion_semestres  VARCHAR(45),
    @tipo_titulacion     VARCHAR(45),
    @programa            INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM programa WHERE id = @programa AND activo = 1)
        THROW 50101, N'El programa indicado no existe o esta inactivo.', 1;

    INSERT INTO registro_calificado (codigo, cant_creditos, hora_acom, hora_ind, metodologia,
                                     fecha_inicio, fecha_fin, duracion_anios,
                                     duracion_semestres, tipo_titulacion, programa, activo)
    VALUES (@codigo, @cant_creditos, @hora_acom, @hora_ind, @metodologia,
            @fecha_inicio, @fecha_fin, @duracion_anios,
            @duracion_semestres, @tipo_titulacion, @programa, 1);

    -- Fila guardada: aqui las fechas DATE vuelven como date (el modelo C# las
    -- expone DateTime y SoloFechaJsonConverter las serializa "yyyy-MM-dd").
    SELECT r.codigo,
           r.cant_creditos,
           r.hora_acom,
           r.hora_ind,
           r.metodologia,
           r.fecha_inicio,
           r.fecha_fin,
           r.duracion_anios,
           r.duracion_semestres,
           r.tipo_titulacion,
           r.programa,
           pr.nombre AS programa_nombre,
           r.activo
      FROM registro_calificado r
      LEFT JOIN programa pr ON pr.id = r.programa
     WHERE r.codigo = @codigo;
END
GO

CREATE OR ALTER PROCEDURE sp_actualizar_registro_calificado
    @codigo              INT,
    @cant_creditos       VARCHAR(45),
    @hora_acom           VARCHAR(45),
    @hora_ind            VARCHAR(45),
    @metodologia         VARCHAR(45),
    @fecha_inicio        DATE,
    @fecha_fin           DATE,
    @duracion_anios      VARCHAR(45),
    @duracion_semestres  VARCHAR(45),
    @tipo_titulacion     VARCHAR(45),
    @programa            INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM programa WHERE id = @programa AND activo = 1)
        THROW 50101, N'El programa indicado no existe o esta inactivo.', 1;

    UPDATE registro_calificado
       SET cant_creditos      = @cant_creditos,
           hora_acom          = @hora_acom,
           hora_ind           = @hora_ind,
           metodologia        = @metodologia,
           fecha_inicio       = @fecha_inicio,
           fecha_fin          = @fecha_fin,
           duracion_anios     = @duracion_anios,
           duracion_semestres = @duracion_semestres,
           tipo_titulacion    = @tipo_titulacion,
           programa           = @programa
     WHERE codigo = @codigo AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

CREATE OR ALTER PROCEDURE sp_eliminar_registro_calificado
    @codigo INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Hijos activos (5_data_model.md §4.2): las puentes enfoque_rc y aa_rc.
    IF EXISTS (SELECT 1 FROM enfoque_rc WHERE registro_calificado = @codigo AND activo = 1)
    OR EXISTS (SELECT 1 FROM aa_rc      WHERE registro_calificado_codigo = @codigo AND activo = 1)
    BEGIN
        SELECT 0 AS filas;
        RETURN;
    END

    UPDATE registro_calificado
       SET activo = 0
     WHERE codigo = @codigo AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

-- ############################################################################
-- # activ_academica — detalle de programa con la UNICA FK opcional (`disenio`) #
-- # Es padre de la puente aa_rc.                                              #
-- ############################################################################

CREATE OR ALTER PROCEDURE sp_listar_activ_academica
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite)
           a.id,
           a.nombre,
           a.num_creditos,
           a.tipo,
           a.area_formacion,
           a.h_acom,
           a.h_indep,
           a.idioma,
           a.espejo,
           a.entidad_espejo,
           a.pais_espejo,
           a.disenio,
           d.nombre AS disenio_nombre,       -- alias para la propiedad DisenioNombre
           a.activo
      FROM activ_academica a
      LEFT JOIN programa d ON d.id = a.disenio
     WHERE a.activo = 1
     ORDER BY a.id;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_activ_academica
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.id,
           a.nombre,
           a.num_creditos,
           a.tipo,
           a.area_formacion,
           a.h_acom,
           a.h_indep,
           a.idioma,
           a.espejo,
           a.entidad_espejo,
           a.pais_espejo,
           a.disenio,
           d.nombre AS disenio_nombre,
           a.activo
      FROM activ_academica a
      LEFT JOIN programa d ON d.id = a.disenio
     WHERE a.id = @id AND a.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_crear_activ_academica
    @nombre         VARCHAR(45),
    @num_creditos   INT,
    @tipo           VARCHAR(20),
    @area_formacion VARCHAR(45),
    @h_acom         INT,
    @h_indep        INT,
    @idioma         VARCHAR(45),
    @espejo         TINYINT,
    @entidad_espejo VARCHAR(45),
    @pais_espejo    VARCHAR(45),
    @disenio        INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- `disenio` es OPCIONAL (tropiezo 4): llega NULL, nunca cadena vacia. Solo
    -- se valida cuando viene con valor; el `IS NOT NULL` evita que un NULL
    -- entre al NOT EXISTS (que seria siempre falso y no lanzaria el error).
    IF @disenio IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM programa WHERE id = @disenio AND activo = 1)
        THROW 50101, N'El programa indicado no existe o esta inactivo.', 1;

    -- El id interno lo genera la secuencia de SQL Server; OUTPUT captura el
    -- valor para devolver la fila guardada, igual que en programa.
    DECLARE @id_generado INT;
    DECLARE @insertado TABLE (id INT NOT NULL);

    INSERT INTO activ_academica (nombre, num_creditos, tipo, area_formacion,
                                 h_acom, h_indep, idioma, espejo, entidad_espejo,
                                 pais_espejo, disenio, activo)
    OUTPUT INSERTED.id INTO @insertado (id)
    VALUES (@nombre, @num_creditos, @tipo, @area_formacion,
            @h_acom, @h_indep, @idioma, @espejo, @entidad_espejo,
            @pais_espejo, @disenio, 1);

    SELECT @id_generado = id FROM @insertado;

    SELECT a.id,
           a.nombre,
           a.num_creditos,
           a.tipo,
           a.area_formacion,
           a.h_acom,
           a.h_indep,
           a.idioma,
           a.espejo,
           a.entidad_espejo,
           a.pais_espejo,
           a.disenio,
           d.nombre AS disenio_nombre,
           a.activo
      FROM activ_academica a
      LEFT JOIN programa d ON d.id = a.disenio
     WHERE a.id = @id_generado;
END
GO

CREATE OR ALTER PROCEDURE sp_actualizar_activ_academica
    @id             INT,
    @nombre         VARCHAR(45),
    @num_creditos   INT,
    @tipo           VARCHAR(20),
    @area_formacion VARCHAR(45),
    @h_acom         INT,
    @h_indep        INT,
    @idioma         VARCHAR(45),
    @espejo         TINYINT,
    @entidad_espejo VARCHAR(45),
    @pais_espejo    VARCHAR(45),
    @disenio        INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @disenio IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM programa WHERE id = @disenio AND activo = 1)
        THROW 50101, N'El programa indicado no existe o esta inactivo.', 1;

    UPDATE activ_academica
       SET nombre         = @nombre,
           num_creditos   = @num_creditos,
           tipo           = @tipo,
           area_formacion = @area_formacion,
           h_acom         = @h_acom,
           h_indep        = @h_indep,
           idioma         = @idioma,
           espejo         = @espejo,
           entidad_espejo = @entidad_espejo,
           pais_espejo    = @pais_espejo,
           disenio        = @disenio
     WHERE id = @id AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

CREATE OR ALTER PROCEDURE sp_eliminar_activ_academica
    @id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Hijo activo (5_data_model.md §4.2): la puente aa_rc.
    IF EXISTS (SELECT 1 FROM aa_rc WHERE activ_academicas_idcurso = @id AND activo = 1)
    BEGIN
        SELECT 0 AS filas;
        RETURN;
    END

    UPDATE activ_academica
       SET activo = 0
     WHERE id = @id AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

-- ############################################################################
-- # enfoque_rc — puente enfoque <-> registro_calificado (sin PUT ni PATCH)     #
-- # registro_calificado no tiene columna `nombre`: el texto legible se calcula #
-- # como "codigo - tipo_titulacion" (mismo criterio que el desplegable del      #
-- # frontend), para que el listado nunca muestre solo una clave.                #
-- ############################################################################

CREATE OR ALTER PROCEDURE sp_listar_enfoque_rc
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite)
           er.enfoque,
           er.registro_calificado,
           e.nombre AS enfoque_nombre,
           CAST(rc.codigo AS VARCHAR(20)) + ' - ' + rc.tipo_titulacion AS registro_calificado_nombre,
           er.activo
      FROM enfoque_rc er
      LEFT JOIN enfoque e ON e.id = er.enfoque
      LEFT JOIN registro_calificado rc ON rc.codigo = er.registro_calificado
     WHERE er.activo = 1
     ORDER BY er.enfoque, er.registro_calificado;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_enfoque_rc
    @enfoque            INT,
    @registro_calificado INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT er.enfoque,
           er.registro_calificado,
           e.nombre AS enfoque_nombre,
           CAST(rc.codigo AS VARCHAR(20)) + ' - ' + rc.tipo_titulacion AS registro_calificado_nombre,
           er.activo
      FROM enfoque_rc er
      LEFT JOIN enfoque e ON e.id = er.enfoque
      LEFT JOIN registro_calificado rc ON rc.codigo = er.registro_calificado
     WHERE er.enfoque = @enfoque
       AND er.registro_calificado = @registro_calificado
       AND er.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_asignar_enfoque_rc
    @enfoque             INT,
    @registro_calificado INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Las dos FK se validan con mensajes distintos (por eso el numero propio):
    -- la restriccion 547 de la base no diria cual de las dos fallo.
    IF NOT EXISTS (SELECT 1 FROM enfoque WHERE id = @enfoque AND activo = 1)
        THROW 50105, N'El enfoque indicado no existe o esta inactivo.', 1;

    IF NOT EXISTS (SELECT 1 FROM registro_calificado WHERE codigo = @registro_calificado AND activo = 1)
        THROW 50103, N'El registro calificado indicado no existe o esta inactivo.', 1;

    -- Si la pareja ya existe (incluso retirada, activo = 0) la rechaza la PK
    -- compuesta (2627): una pareja existe o no existe, no se "reasigna".
    INSERT INTO enfoque_rc (enfoque, registro_calificado, activo)
    VALUES (@enfoque, @registro_calificado, 1);

    SELECT er.enfoque,
           er.registro_calificado,
           e.nombre AS enfoque_nombre,
           CAST(rc.codigo AS VARCHAR(20)) + ' - ' + rc.tipo_titulacion AS registro_calificado_nombre,
           er.activo
      FROM enfoque_rc er
      LEFT JOIN enfoque e ON e.id = er.enfoque
      LEFT JOIN registro_calificado rc ON rc.codigo = er.registro_calificado
     WHERE er.enfoque = @enfoque AND er.registro_calificado = @registro_calificado;
END
GO

CREATE OR ALTER PROCEDURE sp_retirar_enfoque_rc
    @enfoque             INT,
    @registro_calificado INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Retirar es borrado logico; el 404 lo decide el servicio cuando @@ROWCOUNT = 0.
    UPDATE enfoque_rc
       SET activo = 0
     WHERE enfoque = @enfoque
       AND registro_calificado = @registro_calificado
       AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

-- ############################################################################
-- # aa_rc — puente activ_academica <-> registro_calificado, con atributos      #
-- # propios (`componente`, `semestre`). Los nombres de la PK son largos        #
-- # (activ_academicas_idcurso, registro_calificado_codigo) y asi se usan en    #
-- # parametros y alias.                                                        #
-- ############################################################################

CREATE OR ALTER PROCEDURE sp_listar_aa_rc
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite)
           ar.activ_academicas_idcurso,
           ar.registro_calificado_codigo,
           aa.nombre AS activ_academica_nombre,
           CAST(rc.codigo AS VARCHAR(20)) + ' - ' + rc.tipo_titulacion AS registro_calificado_nombre,
           ar.componente,
           ar.semestre,
           ar.activo
      FROM aa_rc ar
      LEFT JOIN activ_academica aa ON aa.id = ar.activ_academicas_idcurso
      LEFT JOIN registro_calificado rc ON rc.codigo = ar.registro_calificado_codigo
     WHERE ar.activo = 1
     ORDER BY ar.activ_academicas_idcurso, ar.registro_calificado_codigo;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_aa_rc
    @activ_academicas_idcurso   INT,
    @registro_calificado_codigo INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ar.activ_academicas_idcurso,
           ar.registro_calificado_codigo,
           aa.nombre AS activ_academica_nombre,
           CAST(rc.codigo AS VARCHAR(20)) + ' - ' + rc.tipo_titulacion AS registro_calificado_nombre,
           ar.componente,
           ar.semestre,
           ar.activo
      FROM aa_rc ar
      LEFT JOIN activ_academica aa ON aa.id = ar.activ_academicas_idcurso
      LEFT JOIN registro_calificado rc ON rc.codigo = ar.registro_calificado_codigo
     WHERE ar.activ_academicas_idcurso = @activ_academicas_idcurso
       AND ar.registro_calificado_codigo = @registro_calificado_codigo
       AND ar.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_asignar_aa_rc
    @activ_academicas_idcurso   INT,
    @registro_calificado_codigo INT,
    @componente                 VARCHAR(45),
    @semestre                   VARCHAR(45)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM activ_academica WHERE id = @activ_academicas_idcurso AND activo = 1)
        THROW 50104, N'La actividad academica indicada no existe o esta inactiva.', 1;

    IF NOT EXISTS (SELECT 1 FROM registro_calificado WHERE codigo = @registro_calificado_codigo AND activo = 1)
        THROW 50103, N'El registro calificado indicado no existe o esta inactivo.', 1;

    -- `componente` y `semestre` son atributos de la pareja: viajan en el INSERT
    -- y no se editan despues (las puentes no tienen actualizacion).
    INSERT INTO aa_rc (activ_academicas_idcurso, registro_calificado_codigo,
                       componente, semestre, activo)
    VALUES (@activ_academicas_idcurso, @registro_calificado_codigo,
            @componente, @semestre, 1);

    SELECT ar.activ_academicas_idcurso,
           ar.registro_calificado_codigo,
           aa.nombre AS activ_academica_nombre,
           CAST(rc.codigo AS VARCHAR(20)) + ' - ' + rc.tipo_titulacion AS registro_calificado_nombre,
           ar.componente,
           ar.semestre,
           ar.activo
      FROM aa_rc ar
      LEFT JOIN activ_academica aa ON aa.id = ar.activ_academicas_idcurso
      LEFT JOIN registro_calificado rc ON rc.codigo = ar.registro_calificado_codigo
     WHERE ar.activ_academicas_idcurso = @activ_academicas_idcurso
       AND ar.registro_calificado_codigo = @registro_calificado_codigo;
END
GO

CREATE OR ALTER PROCEDURE sp_retirar_aa_rc
    @activ_academicas_idcurso   INT,
    @registro_calificado_codigo INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aa_rc
       SET activo = 0
     WHERE activ_academicas_idcurso = @activ_academicas_idcurso
       AND registro_calificado_codigo = @registro_calificado_codigo
       AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

-- ############################################################################
-- # trg_acreditacion_contador — estado derivado programa.num_acreditaciones    #
-- #                                                                            #
-- # POR QUE asi:                                                                #
-- #  * AFTER INSERT, UPDATE, DELETE: retirar una acreditacion es un UPDATE      #
-- #    (activo = 0), no un DELETE; un disparador sordo al UPDATE dejaria el     #
-- #    contador mal (tropiezo 6).                                               #
-- #  * Trabaja con inserted y deleted (NUNCA variables escalares): si una       #
-- #    operacion toca varias filas a la vez, todas quedan contadas.             #
-- #  * El programa afectado sale de la UNION de inserted y deleted: si un       #
-- #    UPDATE cambia la FK `programa`, se recalculan el programa viejo Y el     #
-- #    nuevo.                                                                   #
-- # La API nunca escribe num_acreditaciones: este disparador es su unico dueno. #
-- ############################################################################

CREATE OR ALTER TRIGGER trg_acreditacion_contador
ON acreditacion
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
       SET num_acreditaciones = (SELECT COUNT(*)
                                   FROM acreditacion a
                                  WHERE a.programa = p.id
                                    AND a.activo = 1)
      FROM programa p
     WHERE p.id IN (SELECT programa FROM inserted
                    UNION
                    SELECT programa FROM deleted);
END
GO

-- =============================================================================
-- SCRIPT DE PRUEBA DEL DISPARADOR (NO lo ejecuta init.sh; esta comentado para
-- que puedas copiarlo y correrlo a mano contra la base `innovacion_curricular`).
--
-- Caso 1 — asignar y retirar una acreditacion (la demostracion de la guia):
--   1) EXEC sp_crear_acreditacion @resolucion = 9001, @tipo = N'Nacional',
--          @calificacion = N'Alta', @fecha_inicio = N'2020-01-01',
--          @fecha_fin = N'2024-01-01', @programa = 10;
--      SELECT id, num_acreditaciones FROM programa WHERE id = 10;   -- sube en 1
--   2) EXEC sp_eliminar_acreditacion @resolucion = 9001;           -- UPDATE activo = 0
--      SELECT id, num_acreditaciones FROM programa WHERE id = 10;   -- baja en 1
--      (si el disparador no escuchara el UPDATE, este numero quedaria alto)
--
-- Caso 2 — un UPDATE que cambia la FK recalcula los dos programas:
--   3) EXEC sp_crear_acreditacion @resolucion = 9002, ... @programa = 10;
--      SELECT id, num_acreditaciones FROM programa WHERE id IN (10, 11); -- 10 sube
--   4) EXEC sp_actualizar_acreditacion @resolucion = 9002, ..., @programa = 11;
--      SELECT id, num_acreditaciones FROM programa WHERE id IN (10, 11);
--      -- el 10 baja y el 11 sube: el disparador miro inserted UNION deleted
--
-- Caso 3 — varias filas en una sola operacion (sin variables escalares):
--   5) UPDATE acreditacion SET activo = 0
--       WHERE resolucion IN (9002, 9003);   -- dos programas afectados a la vez
--      SELECT id, num_acreditaciones FROM programa WHERE id IN (10, 11); -- ambos correctos
--
-- Pasos para demostrarlo DESDE LA INTERFAZ (criterio 9):
--   a) Abrir el catálogo de Programa y entrar al detalle de un programa
--      (botón «Detalle») o crear uno primero desde «+ Crear».
--   b) Anotar «Acreditaciones registradas» (num_acreditaciones del programa).
--   c) En la sección «Acreditaciones», agregar un renglón y guardarlo.
--      Volver a mirar el contador: subió solo (la API no envía ese campo).
--   d) Retirar esa acreditación con el botón «Retirar» de su fila.
--      El contador baja solo: eso demuestra que el disparador escucha el UPDATE.
-- =============================================================================
