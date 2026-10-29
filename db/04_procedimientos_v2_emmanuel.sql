-- =============================================================================
-- 04_procedimientos_v2_emmanuel.sql
-- Procedimientos almacenados de las tablas de la v2 a cargo de Emmanuel:
--   * facultad  (maestro de programa)
--   * pasantia  (detalle de programa)
--   * premio    (detalle de programa)
--
-- Juego de cinco procedimientos por tabla (3_plan.md, Sección 5):
--   sp_listar_<tabla>     -> filas activas con el nombre de la FK resuelto
--   sp_consultar_<tabla>  -> una fila activa por PK
--   sp_crear_<tabla>      -> INSERT (activo = 1) y devuelve la fila guardada
--   sp_actualizar_<tabla> -> UPDATE de la fila activa; devuelve @@ROWCOUNT
--   sp_eliminar_<tabla>   -> borrado lógico (activo = 0); devuelve @@ROWCOUNT
--
-- Seguro de repetir: CREATE OR ALTER PROCEDURE. Todo el SQL de la v2 vive aquí
-- (cero SQL en C#). Fuente: docs/spec_kit/versiones/v2/3_plan.md §5-6.
-- =============================================================================

USE [innovacion_curricular];
GO

-- ############################################################################
-- # facultad — maestro de `programa` (regla §3.1: no se retira con hijos)      #
-- ############################################################################

CREATE OR ALTER PROCEDURE sp_listar_facultad
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite)
           f.id,
           f.nombre,
           f.tipo,
           f.fecha_fun,
           f.universidad,
           u.nombre AS universidad_nombre,   -- alias para la propiedad UniversidadNombre
           f.activo
      FROM facultad f
      LEFT JOIN universidad u ON u.id = f.universidad
     WHERE f.activo = 1
     ORDER BY f.id;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_facultad
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.id,
           f.nombre,
           f.tipo,
           f.fecha_fun,
           f.universidad,
           u.nombre AS universidad_nombre,
           f.activo
      FROM facultad f
      LEFT JOIN universidad u ON u.id = f.universidad
     WHERE f.id = @id AND f.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_crear_facultad
    @nombre      VARCHAR(60),
    @tipo        VARCHAR(45),
    @fecha_fun   DATE,
    @universidad INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @id_generado INT;
    DECLARE @insertado TABLE (id INT NOT NULL);

    INSERT INTO facultad (nombre, tipo, fecha_fun, universidad, activo)
    OUTPUT INSERTED.id INTO @insertado (id)
    VALUES (@nombre, @tipo, @fecha_fun, @universidad, 1);

    SELECT @id_generado = id FROM @insertado;
    SELECT f.id,
           f.nombre,
           f.tipo,
           f.fecha_fun,
           f.universidad,
           u.nombre AS universidad_nombre,
           f.activo
      FROM facultad f
      LEFT JOIN universidad u ON u.id = f.universidad
     WHERE f.id = @id_generado;
END
GO

CREATE OR ALTER PROCEDURE sp_actualizar_facultad
    @id          INT,
    @nombre      VARCHAR(60),
    @tipo        VARCHAR(45),
    @fecha_fun   DATE,
    @universidad INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE facultad
       SET nombre      = @nombre,
           tipo        = @tipo,
           fecha_fun   = @fecha_fun,
           universidad = @universidad
     WHERE id = @id AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

CREATE OR ALTER PROCEDURE sp_eliminar_facultad
    @id INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Regla §3.1 (2_spec.md): un padre con hijos activos no se retira.
    -- No se actualiza ninguna fila; el servicio traduce filas = 0 a 409.
    IF EXISTS (SELECT 1 FROM programa WHERE facultad = @id AND activo = 1)
    BEGIN
        SELECT 0 AS filas;
        RETURN;
    END

    UPDATE facultad
       SET activo = 0
     WHERE id = @id AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

-- ############################################################################
-- # pasantia — detalle de `programa`                                          #
-- ############################################################################

CREATE OR ALTER PROCEDURE sp_listar_pasantia
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite)
           p.id,
           p.nombre,
           p.pais,
           p.empresa,
           p.descripcion,
           p.programa,
           pr.nombre AS programa_nombre,   -- alias para la propiedad ProgramaNombre
           p.activo
      FROM pasantia p
      LEFT JOIN programa pr ON pr.id = p.programa
     WHERE p.activo = 1
     ORDER BY p.id;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_pasantia
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.id,
           p.nombre,
           p.pais,
           p.empresa,
           p.descripcion,
           p.programa,
           pr.nombre AS programa_nombre,
           p.activo
      FROM pasantia p
      LEFT JOIN programa pr ON pr.id = p.programa
     WHERE p.id = @id AND p.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_crear_pasantia
    @nombre      VARCHAR(45),
    @pais        VARCHAR(45),
    @empresa     VARCHAR(45),
    @descripcion VARCHAR(45),
    @programa    INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @id_generado INT;
    DECLARE @insertado TABLE (id INT NOT NULL);

    INSERT INTO pasantia (nombre, pais, empresa, descripcion, programa, activo)
    OUTPUT INSERTED.id INTO @insertado (id)
    VALUES (@nombre, @pais, @empresa, @descripcion, @programa, 1);

    SELECT @id_generado = id FROM @insertado;
    SELECT p.id,
           p.nombre,
           p.pais,
           p.empresa,
           p.descripcion,
           p.programa,
           pr.nombre AS programa_nombre,
           p.activo
      FROM pasantia p
      LEFT JOIN programa pr ON pr.id = p.programa
     WHERE p.id = @id_generado;
END
GO

CREATE OR ALTER PROCEDURE sp_actualizar_pasantia
    @id          INT,
    @nombre      VARCHAR(45),
    @pais        VARCHAR(45),
    @empresa     VARCHAR(45),
    @descripcion VARCHAR(45),
    @programa    INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE pasantia
       SET nombre      = @nombre,
           pais        = @pais,
           empresa     = @empresa,
           descripcion = @descripcion,
           programa    = @programa
     WHERE id = @id AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

CREATE OR ALTER PROCEDURE sp_eliminar_pasantia
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE pasantia
       SET activo = 0
     WHERE id = @id AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

-- ############################################################################
-- # premio — detalle de `programa`                                            #
-- ############################################################################

CREATE OR ALTER PROCEDURE sp_listar_premio
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite)
           p.id,
           p.nombre,
           p.descripcion,
           p.fecha,
           p.entidad_otorgante,
           p.pais,
           p.programa,
           pr.nombre AS programa_nombre,
           p.activo
      FROM premio p
      LEFT JOIN programa pr ON pr.id = p.programa
     WHERE p.activo = 1
     ORDER BY p.id;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_premio
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.id,
           p.nombre,
           p.descripcion,
           p.fecha,
           p.entidad_otorgante,
           p.pais,
           p.programa,
           pr.nombre AS programa_nombre,
           p.activo
      FROM premio p
      LEFT JOIN programa pr ON pr.id = p.programa
     WHERE p.id = @id AND p.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_crear_premio
    @nombre            VARCHAR(45),
    @descripcion       VARCHAR(45),
    @fecha             DATE,
    @entidad_otorgante VARCHAR(45),
    @pais              VARCHAR(45),
    @programa          INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @id_generado INT;
    DECLARE @insertado TABLE (id INT NOT NULL);

    INSERT INTO premio (nombre, descripcion, fecha, entidad_otorgante, pais, programa, activo)
    OUTPUT INSERTED.id INTO @insertado (id)
    VALUES (@nombre, @descripcion, @fecha, @entidad_otorgante, @pais, @programa, 1);

    SELECT @id_generado = id FROM @insertado;
    SELECT p.id,
           p.nombre,
           p.descripcion,
           p.fecha,
           p.entidad_otorgante,
           p.pais,
           p.programa,
           pr.nombre AS programa_nombre,
           p.activo
      FROM premio p
      LEFT JOIN programa pr ON pr.id = p.programa
     WHERE p.id = @id_generado;
END
GO

CREATE OR ALTER PROCEDURE sp_actualizar_premio
    @id                INT,
    @nombre            VARCHAR(45),
    @descripcion       VARCHAR(45),
    @fecha             DATE,
    @entidad_otorgante VARCHAR(45),
    @pais              VARCHAR(45),
    @programa          INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE premio
       SET nombre            = @nombre,
           descripcion       = @descripcion,
           fecha             = @fecha,
           entidad_otorgante = @entidad_otorgante,
           pais              = @pais,
           programa          = @programa
     WHERE id = @id AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO

CREATE OR ALTER PROCEDURE sp_eliminar_premio
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE premio
       SET activo = 0
     WHERE id = @id AND activo = 1;

    SELECT @@ROWCOUNT AS filas;
END
GO
