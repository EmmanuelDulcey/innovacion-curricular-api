-- =============================================================================
-- 05_identificadores_incrementales.sql
-- Genera IDs internos mediante secuencias sin reconstruir tablas existentes.
-- Las claves naturales (NIT, código de registro y resoluciones), las claves
-- externas de docentes y las PK compuestas conservan su significado original.
-- =============================================================================
USE [innovacion_curricular];
GO

SET NOCOUNT ON;

DECLARE @tablas TABLE (tabla SYSNAME NOT NULL, secuencia SYSNAME NOT NULL);
INSERT INTO @tablas (tabla, secuencia) VALUES
    (N'area_conocimiento', N'seq_area_conocimiento_id'),
    (N'universidad', N'seq_universidad_id'),
    (N'aspecto_normativo', N'seq_aspecto_normativo_id'),
    (N'practica_estrategia', N'seq_practica_estrategia_id'),
    (N'enfoque', N'seq_enfoque_id'),
    (N'car_innovacion', N'seq_car_innovacion_id'),
    (N'facultad', N'seq_facultad_id'),
    (N'programa', N'seq_programa_id'),
    (N'activ_academica', N'seq_activ_academica_id'),
    (N'pasantia', N'seq_pasantia_id'),
    (N'premio', N'seq_premio_id');

DECLARE @tabla SYSNAME;
DECLARE @secuencia SYSNAME;
DECLARE @sql NVARCHAR(MAX);
DECLARE @maximo BIGINT;
DECLARE @actual sql_variant;
DECLARE @inicio BIGINT;
DECLARE @restriccion SYSNAME;

DECLARE cursor_tablas CURSOR LOCAL FAST_FORWARD FOR
    SELECT tabla, secuencia FROM @tablas;
OPEN cursor_tablas;
FETCH NEXT FROM cursor_tablas INTO @tabla, @secuencia;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @maximo = 0;
    SET @sql = N'SELECT @valor = ISNULL(MAX(CONVERT(BIGINT, id)), 0) FROM dbo.'
        + QUOTENAME(@tabla) + N';';
    EXEC sys.sp_executesql @sql, N'@valor BIGINT OUTPUT', @valor = @maximo OUTPUT;

    IF OBJECT_ID(N'dbo.' + @secuencia, N'SO') IS NULL
    BEGIN
        SET @inicio = @maximo + 1;
        SET @sql = N'CREATE SEQUENCE dbo.' + QUOTENAME(@secuencia)
            + N' AS INT START WITH ' + CONVERT(NVARCHAR(30), @inicio)
            + N' INCREMENT BY 1;';
        EXEC sys.sp_executesql @sql;
    END
    ELSE
    BEGIN
        SELECT @actual = current_value
          FROM sys.sequences
         WHERE object_id = OBJECT_ID(N'dbo.' + @secuencia, N'SO');

        IF @actual IS NULL OR TRY_CONVERT(BIGINT, @actual) < @maximo
        BEGIN
            SET @inicio = @maximo + 1;
            SET @sql = N'ALTER SEQUENCE dbo.' + QUOTENAME(@secuencia)
                + N' RESTART WITH ' + CONVERT(NVARCHAR(30), @inicio) + N';';
            EXEC sys.sp_executesql @sql;
        END
    END;

    SET @restriccion = N'DF_' + @tabla + N'_id';
    IF NOT EXISTS
    (
        SELECT 1
          FROM sys.default_constraints d
          JOIN sys.columns c
            ON c.object_id = d.parent_object_id
           AND c.column_id = d.parent_column_id
         WHERE d.parent_object_id = OBJECT_ID(N'dbo.' + @tabla)
           AND c.name = N'id'
    )
    BEGIN
        SET @sql = N'ALTER TABLE dbo.' + QUOTENAME(@tabla)
            + N' ADD CONSTRAINT ' + QUOTENAME(@restriccion)
            + N' DEFAULT (NEXT VALUE FOR dbo.' + QUOTENAME(@secuencia)
            + N') FOR [id];';
        EXEC sys.sp_executesql @sql;
    END;

    FETCH NEXT FROM cursor_tablas INTO @tabla, @secuencia;
END;

CLOSE cursor_tablas;
DEALLOCATE cursor_tablas;
GO
