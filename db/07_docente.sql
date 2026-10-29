-- =============================================================================
-- 07_docente.sql
-- Catálogo local solicitado para seleccionar docentes por su nombre. El ID se
-- genera en SQL Server; documento es un dato único de la persona, no la PK.
-- El script se puede repetir y conserva fichas y relaciones existentes.
-- =============================================================================
USE [innovacion_curricular];
GO

IF OBJECT_ID(N'dbo.docente', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.docente
    (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_docente PRIMARY KEY,
        documento VARCHAR(20) NOT NULL,
        nombre VARCHAR(120) NOT NULL,
        activo BIT NOT NULL CONSTRAINT DF_docente_activo DEFAULT 1,
        CONSTRAINT UQ_docente_documento UNIQUE (documento)
    );
END;
GO

IF COL_LENGTH(N'dbo.docente', N'activo') IS NULL
    ALTER TABLE dbo.docente ADD activo BIT NOT NULL CONSTRAINT DF_docente_activo DEFAULT 1;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.key_constraints
     WHERE parent_object_id = OBJECT_ID(N'dbo.docente')
       AND name = N'UQ_docente_documento'
)
    ALTER TABLE dbo.docente ADD CONSTRAINT UQ_docente_documento UNIQUE (documento);
GO

-- La validación WITH NOCHECK conserva asociaciones históricas que pudieran
-- contener códigos externos previos; toda asociación nueva sí queda protegida.
IF NOT EXISTS
(
    SELECT 1 FROM sys.foreign_keys
     WHERE parent_object_id = OBJECT_ID(N'dbo.docente_departamento')
       AND name = N'FK_docente_departamento_docente'
)
    ALTER TABLE dbo.docente_departamento WITH NOCHECK
        ADD CONSTRAINT FK_docente_departamento_docente FOREIGN KEY (docente)
        REFERENCES dbo.docente(id);
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.foreign_keys
     WHERE parent_object_id = OBJECT_ID(N'dbo.alianza')
       AND name = N'FK_alianza_docente'
)
    ALTER TABLE dbo.alianza WITH NOCHECK
        ADD CONSTRAINT FK_alianza_docente FOREIGN KEY (docente)
        REFERENCES dbo.docente(id);
GO

CREATE OR ALTER PROCEDURE dbo.sp_listar_docente
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite) id, documento, nombre, activo
      FROM dbo.docente
     WHERE activo = 1
     ORDER BY id;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_consultar_docente
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id, documento, nombre, activo
      FROM dbo.docente
     WHERE id = @id AND activo = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_crear_docente
    @documento VARCHAR(20),
    @nombre VARCHAR(120)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @id_generado INT;
    DECLARE @insertado TABLE (id INT NOT NULL);

    INSERT INTO dbo.docente (documento, nombre, activo)
    OUTPUT INSERTED.id INTO @insertado (id)
    VALUES (@documento, @nombre, 1);

    SELECT @id_generado = id FROM @insertado;
    SELECT id, documento, nombre, activo
      FROM dbo.docente
     WHERE id = @id_generado;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_actualizar_docente
    @id INT,
    @documento VARCHAR(20),
    @nombre VARCHAR(120)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.docente
       SET documento = @documento,
           nombre = @nombre
     WHERE id = @id AND activo = 1;
    SELECT @@ROWCOUNT AS filas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_eliminar_docente
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.docente_departamento WHERE docente = @id AND activo = 1)
       OR EXISTS (SELECT 1 FROM dbo.alianza WHERE docente = @id AND activo = 1)
    BEGIN
        SELECT 0 AS filas;
        RETURN;
    END;

    UPDATE dbo.docente SET activo = 0 WHERE id = @id AND activo = 1;
    SELECT @@ROWCOUNT AS filas;
END
GO
