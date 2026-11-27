-- =============================================================================
-- 10_revocacion_tokens_v3.sql
-- Revocacion de sesion (v3): lista negra de JWT cerrados por logout.
--   * tabla token_revocado   -> un jti por token cerrado
--   * sp_revocar_token       -> marca un jti como revocado (idempotente)
--   * sp_token_revocado      -> consulta si un jti esta revocado
--
-- La expiracion natural del JWT (Jwt:ExpirationMinutes) ya la valida el
-- middleware de autenticacion; esta tabla cubre el caso que el JWT por si
-- solo no resuelve: invalidar un token ANTES de que expire (logout real).
-- Seguro de repetir: CREATE OR ALTER + IF NOT EXISTS para la tabla.
-- =============================================================================
USE [innovacion_curricular];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'token_revocado')
BEGIN
    CREATE TABLE token_revocado (
        jti VARCHAR(50) NOT NULL PRIMARY KEY,
        fecha_revocacion DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

CREATE OR ALTER PROCEDURE sp_revocar_token
    @jti VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM token_revocado WHERE jti = @jti)
        INSERT INTO token_revocado (jti) VALUES (@jti);
END
GO

CREATE OR ALTER PROCEDURE sp_token_revocado
    @jti VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE WHEN EXISTS (SELECT 1 FROM token_revocado WHERE jti = @jti)
                THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS revocado;
END
GO

-- Comprobacion rapida (opcional):
--   EXEC sp_revocar_token @jti = 'un-jti-de-prueba';
--   EXEC sp_token_revocado @jti = 'un-jti-de-prueba';   -- revocado = 1
-- =============================================================================
