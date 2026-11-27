-- =============================================================================
-- 09_seguridad_v3.sql
-- Autenticacion JWT de la v3:
--   * sp_consultar_usuario_por_username -> usuario (incluye el hash bcrypt)
--   * sp_listar_roles_usuario           -> roles activos del usuario
--   * migracion de las contrasenas planas de demostracion a bcrypt.
--
-- Seguro de repetir: CREATE OR ALTER y el UPDATE solo actua si la contrasena
-- sigue siendo el literal de demostracion. Las contrasenas ya hasheadas no se
-- tocan. Fuente: docs/spec_kit/versiones/0_mapa_versiones.md (v3).
-- =============================================================================
USE [innovacion_curricular];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE sp_consultar_usuario_por_username
    @username VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT id,
           username,
           password,
           email,
           nombre_completo,
           activo
      FROM usuario
     WHERE username = @username;
END
GO

CREATE OR ALTER PROCEDURE sp_listar_roles_usuario
    @usuario_id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT r.nombre
      FROM rol r
      INNER JOIN rol_usuario ru ON ru.rol_id = r.id
     WHERE ru.usuario_id = @usuario_id
       AND r.activo = 1
     ORDER BY r.nombre;
END
GO

-- -----------------------------------------------------------------------------
-- Migracion de credenciales de demostracion (texto plano -> bcrypt).
-- Contrasena de demostracion: Demo123!
-- Cada usuario recibe una sal distinta; los hashes se generaron con bcrypt
-- work factor 12. Si un usuario ya cambio su contrasena, no se sobrescribe.
-- -----------------------------------------------------------------------------
UPDATE usuario
   SET password = N'$2a$12$KV3WyeNony.xApgaVAAqjub4mv6e8G8FBOFhPGmn52iGXZr/cASVi',
       fecha_actualizacion = GETDATE()
 WHERE username = N'admin'
   AND password = N'Demo123!';

UPDATE usuario
   SET password = N'$2a$12$54xubhy.uF6CMb3lXCFuW.8XG2TbRIZ4hs9xVz2rIsCGIsPjKoL4a',
       fecha_actualizacion = GETDATE()
 WHERE username = N'coordinador'
   AND password = N'Demo123!';

UPDATE usuario
   SET password = N'$2a$12$GjpixS9tqEvMRd3PMfMNLe0QXociHp2kK2McH9D4HXsjB9jOiTmM2',
       fecha_actualizacion = GETDATE()
 WHERE username = N'docente.demo'
   AND password = N'Demo123!';

UPDATE usuario
   SET password = N'$2a$12$R55tEaoWaTtJ3828QN9N4ug9o9FXGv8bXROrOgo5tUp52nGNa8gtS',
       fecha_actualizacion = GETDATE()
 WHERE username = N'secretaria'
   AND password = N'Demo123!';

UPDATE usuario
   SET password = N'$2a$12$3yZCpJtn0zU9CKaUnhWHSOon1wlPz7xkHsJ3zy4FSTERsyiuK0ZDu',
       fecha_actualizacion = GETDATE()
 WHERE username = N'consulta'
   AND password = N'Demo123!';
GO

-- Comprobacion rapida (opcional):
--   SELECT username, LEFT(password, 7) AS prefijo_hash, activo FROM usuario;
--   EXEC sp_listar_roles_usuario @usuario_id = 1;
-- =============================================================================
