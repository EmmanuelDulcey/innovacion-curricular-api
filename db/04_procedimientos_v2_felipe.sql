-- =============================================================================
-- QUÉ ES: procedimientos de las seis relaciones de Felipe en la v2.
-- PAPEL: la API invoca estos procedimientos; el repositorio C# no escribe SQL.
-- docente_departamento y alianza tienen CRUD de sus atributos; su pareja de
-- claves es inmutable. Las cuatro tablas puente puras solo se asignan/retiran.
-- Se conserva el borrado lógico de la v1 y se devuelven nombres para la UI.
-- =============================================================================
USE [innovacion_curricular];
GO

-- El formulario necesita programas activos mientras llega el CRUD completo
-- del módulo responsable. Una ruta de opciones separada evita duplicar su API.
CREATE OR ALTER PROCEDURE sp_listar_opciones_programa
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite) id, nombre
      FROM programa
     WHERE activo = 1
     ORDER BY nombre, id;
END
GO

-- docente_departamento: la clave compuesta siempre se usa en el orden (docente, departamento).
CREATE OR ALTER PROCEDURE sp_listar_docente_departamento
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite) t.*,
           COALESCE(d.nombre, N'Docente #' + CONVERT(NVARCHAR(20), t.docente) + N' (sin ficha)') AS docente_nombre,
           m1.nombre AS departamento_nombre
      FROM docente_departamento t
      LEFT JOIN docente d ON d.id = t.docente
      LEFT JOIN programa m1 ON m1.id = t.departamento
     WHERE t.activo = 1
     ORDER BY t.docente, t.departamento;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_docente_departamento
    @docente INT,
    @departamento INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.*,
           COALESCE(d.nombre, N'Docente #' + CONVERT(NVARCHAR(20), t.docente) + N' (sin ficha)') AS docente_nombre,
           m1.nombre AS departamento_nombre
      FROM docente_departamento t
      LEFT JOIN docente d ON d.id = t.docente
      LEFT JOIN programa m1 ON m1.id = t.departamento
     WHERE t.docente = @docente AND t.departamento = @departamento AND t.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_crear_docente_departamento
    @docente INT,
    @departamento INT,
    @dedicacion VARCHAR(15),
    @modalidad VARCHAR(45),
    @fecha_ingreso DATE,
    @fecha_salida DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM docente WHERE id = @docente AND activo = 1)
        THROW 51000, 'El docente indicado no existe o está inactivo.', 1;
    -- La FK de SQL Server no impide enlazar un padre retirado lógicamente.
    IF NOT EXISTS (SELECT 1 FROM programa WHERE id = @departamento AND activo = 1)
        THROW 51000, 'El programa indicado no existe o está inactivo.', 1;
    INSERT INTO docente_departamento (docente, departamento, dedicacion, modalidad, fecha_ingreso, fecha_salida, activo)
    VALUES (@docente, @departamento, @dedicacion, @modalidad, @fecha_ingreso, @fecha_salida, 1);

    -- Igual que sp_consultar: el cliente recibe la fila tal como quedó guardada.
    SELECT t.*,
           COALESCE(d.nombre, N'Docente #' + CONVERT(NVARCHAR(20), t.docente) + N' (sin ficha)') AS docente_nombre,
           m1.nombre AS departamento_nombre
      FROM docente_departamento t
      LEFT JOIN docente d ON d.id = t.docente
      LEFT JOIN programa m1 ON m1.id = t.departamento
     WHERE t.docente = @docente AND t.departamento = @departamento AND t.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_actualizar_docente_departamento
    @docente INT,
    @departamento INT,
    @dedicacion VARCHAR(15),
    @modalidad VARCHAR(45),
    @fecha_ingreso DATE,
    @fecha_salida DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE docente_departamento
       SET dedicacion = @dedicacion,
           modalidad = @modalidad,
           fecha_ingreso = @fecha_ingreso,
           fecha_salida = @fecha_salida
     WHERE docente = @docente AND departamento = @departamento AND activo = 1;
    SELECT @@ROWCOUNT AS filas;
END
GO

CREATE OR ALTER PROCEDURE sp_eliminar_docente_departamento
    @docente INT,
    @departamento INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE docente_departamento
       SET activo = 0
     WHERE docente = @docente AND departamento = @departamento AND activo = 1;
    SELECT @@ROWCOUNT AS filas;
END
GO

-- alianza: la clave compuesta siempre se usa en el orden (aliado, departamento).
CREATE OR ALTER PROCEDURE sp_listar_alianza
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite) t.*,
           m1.razon_social AS aliado_nombre,
           m2.nombre AS departamento_nombre,
           COALESCE(d.nombre, N'Docente #' + CONVERT(NVARCHAR(20), t.docente) + N' (sin ficha)') AS docente_nombre
      FROM alianza t
      LEFT JOIN aliado m1 ON m1.nit = t.aliado
      LEFT JOIN programa m2 ON m2.id = t.departamento
      LEFT JOIN docente d ON d.id = t.docente
     WHERE t.activo = 1
     ORDER BY t.aliado, t.departamento;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_alianza
    @aliado INT,
    @departamento INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.*,
           m1.razon_social AS aliado_nombre,
           m2.nombre AS departamento_nombre,
           COALESCE(d.nombre, N'Docente #' + CONVERT(NVARCHAR(20), t.docente) + N' (sin ficha)') AS docente_nombre
      FROM alianza t
      LEFT JOIN aliado m1 ON m1.nit = t.aliado
      LEFT JOIN programa m2 ON m2.id = t.departamento
      LEFT JOIN docente d ON d.id = t.docente
     WHERE t.aliado = @aliado AND t.departamento = @departamento AND t.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_crear_alianza
    @aliado INT,
    @departamento INT,
    @fecha_inicio DATE,
    @fecha_fin DATE = NULL,
    @docente INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- La FK de SQL Server no impide enlazar un padre retirado lógicamente.
    IF NOT EXISTS (SELECT 1 FROM aliado WHERE nit = @aliado AND activo = 1)
        THROW 51000, 'El aliado indicado no existe o está inactivo.', 1;
    -- La FK de SQL Server no impide enlazar un padre retirado lógicamente.
    IF NOT EXISTS (SELECT 1 FROM programa WHERE id = @departamento AND activo = 1)
        THROW 51000, 'El programa indicado no existe o está inactivo.', 1;
    IF @docente IS NOT NULL AND NOT EXISTS (SELECT 1 FROM docente WHERE id = @docente AND activo = 1)
        THROW 51000, 'El docente responsable no existe o está inactivo.', 1;
    INSERT INTO alianza (aliado, departamento, fecha_inicio, fecha_fin, docente, activo)
    VALUES (@aliado, @departamento, @fecha_inicio, @fecha_fin, @docente, 1);

    -- Igual que sp_consultar: el cliente recibe la fila tal como quedó guardada.
    SELECT t.*,
           m1.razon_social AS aliado_nombre,
           m2.nombre AS departamento_nombre,
           COALESCE(d.nombre, N'Docente #' + CONVERT(NVARCHAR(20), t.docente) + N' (sin ficha)') AS docente_nombre
      FROM alianza t
      LEFT JOIN aliado m1 ON m1.nit = t.aliado
      LEFT JOIN programa m2 ON m2.id = t.departamento
      LEFT JOIN docente d ON d.id = t.docente
     WHERE t.aliado = @aliado AND t.departamento = @departamento AND t.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_actualizar_alianza
    @aliado INT,
    @departamento INT,
    @fecha_inicio DATE,
    @fecha_fin DATE = NULL,
    @docente INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @docente IS NOT NULL AND NOT EXISTS (SELECT 1 FROM docente WHERE id = @docente AND activo = 1)
        THROW 51000, 'El docente responsable no existe o está inactivo.', 1;
    UPDATE alianza
       SET fecha_inicio = @fecha_inicio,
           fecha_fin = @fecha_fin,
           docente = @docente
     WHERE aliado = @aliado AND departamento = @departamento AND activo = 1;
    SELECT @@ROWCOUNT AS filas;
END
GO

CREATE OR ALTER PROCEDURE sp_eliminar_alianza
    @aliado INT,
    @departamento INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE alianza
       SET activo = 0
     WHERE aliado = @aliado AND departamento = @departamento AND activo = 1;
    SELECT @@ROWCOUNT AS filas;
END
GO

-- programa_ac: la clave compuesta siempre se usa en el orden (programa, area_conocimiento).
CREATE OR ALTER PROCEDURE sp_listar_programa_ac
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite) t.*,
           m1.nombre AS programa_nombre,
           CONCAT(m2.area, N' — ', m2.disciplina) AS area_conocimiento_nombre
      FROM programa_ac t
      LEFT JOIN programa m1 ON m1.id = t.programa
      LEFT JOIN area_conocimiento m2 ON m2.id = t.area_conocimiento
     WHERE t.activo = 1
     ORDER BY t.programa, t.area_conocimiento;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_programa_ac
    @programa INT,
    @area_conocimiento INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.*,
           m1.nombre AS programa_nombre,
           CONCAT(m2.area, N' — ', m2.disciplina) AS area_conocimiento_nombre
      FROM programa_ac t
      LEFT JOIN programa m1 ON m1.id = t.programa
      LEFT JOIN area_conocimiento m2 ON m2.id = t.area_conocimiento
     WHERE t.programa = @programa AND t.area_conocimiento = @area_conocimiento AND t.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_asignar_programa_ac
    @programa INT,
    @area_conocimiento INT
AS
BEGIN
    SET NOCOUNT ON;
    -- La FK de SQL Server no impide enlazar un padre retirado lógicamente.
    IF NOT EXISTS (SELECT 1 FROM programa WHERE id = @programa AND activo = 1)
        THROW 51000, 'El programa indicado no existe o está inactivo.', 1;
    -- La FK de SQL Server no impide enlazar un padre retirado lógicamente.
    IF NOT EXISTS (SELECT 1 FROM area_conocimiento WHERE id = @area_conocimiento AND activo = 1)
        THROW 51000, 'El area_conocimiento indicado no existe o está inactivo.', 1;
    INSERT INTO programa_ac (programa, area_conocimiento, activo)
    VALUES (@programa, @area_conocimiento, 1);

    -- Igual que sp_consultar: el cliente recibe la fila tal como quedó guardada.
    SELECT t.*,
           m1.nombre AS programa_nombre,
           CONCAT(m2.area, N' — ', m2.disciplina) AS area_conocimiento_nombre
      FROM programa_ac t
      LEFT JOIN programa m1 ON m1.id = t.programa
      LEFT JOIN area_conocimiento m2 ON m2.id = t.area_conocimiento
     WHERE t.programa = @programa AND t.area_conocimiento = @area_conocimiento AND t.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_retirar_programa_ac
    @programa INT,
    @area_conocimiento INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE programa_ac
       SET activo = 0
     WHERE programa = @programa AND area_conocimiento = @area_conocimiento AND activo = 1;
    SELECT @@ROWCOUNT AS filas;
END
GO

-- programa_pe: la clave compuesta siempre se usa en el orden (programa, practica_estrategia).
CREATE OR ALTER PROCEDURE sp_listar_programa_pe
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite) t.*,
           m1.nombre AS programa_nombre,
           m2.nombre AS practica_estrategia_nombre
      FROM programa_pe t
      LEFT JOIN programa m1 ON m1.id = t.programa
      LEFT JOIN practica_estrategia m2 ON m2.id = t.practica_estrategia
     WHERE t.activo = 1
     ORDER BY t.programa, t.practica_estrategia;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_programa_pe
    @programa INT,
    @practica_estrategia INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.*,
           m1.nombre AS programa_nombre,
           m2.nombre AS practica_estrategia_nombre
      FROM programa_pe t
      LEFT JOIN programa m1 ON m1.id = t.programa
      LEFT JOIN practica_estrategia m2 ON m2.id = t.practica_estrategia
     WHERE t.programa = @programa AND t.practica_estrategia = @practica_estrategia AND t.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_asignar_programa_pe
    @programa INT,
    @practica_estrategia INT
AS
BEGIN
    SET NOCOUNT ON;
    -- La FK de SQL Server no impide enlazar un padre retirado lógicamente.
    IF NOT EXISTS (SELECT 1 FROM programa WHERE id = @programa AND activo = 1)
        THROW 51000, 'El programa indicado no existe o está inactivo.', 1;
    -- La FK de SQL Server no impide enlazar un padre retirado lógicamente.
    IF NOT EXISTS (SELECT 1 FROM practica_estrategia WHERE id = @practica_estrategia AND activo = 1)
        THROW 51000, 'El practica_estrategia indicado no existe o está inactivo.', 1;
    INSERT INTO programa_pe (programa, practica_estrategia, activo)
    VALUES (@programa, @practica_estrategia, 1);

    -- Igual que sp_consultar: el cliente recibe la fila tal como quedó guardada.
    SELECT t.*,
           m1.nombre AS programa_nombre,
           m2.nombre AS practica_estrategia_nombre
      FROM programa_pe t
      LEFT JOIN programa m1 ON m1.id = t.programa
      LEFT JOIN practica_estrategia m2 ON m2.id = t.practica_estrategia
     WHERE t.programa = @programa AND t.practica_estrategia = @practica_estrategia AND t.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_retirar_programa_pe
    @programa INT,
    @practica_estrategia INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE programa_pe
       SET activo = 0
     WHERE programa = @programa AND practica_estrategia = @practica_estrategia AND activo = 1;
    SELECT @@ROWCOUNT AS filas;
END
GO

-- programa_ci: la clave compuesta siempre se usa en el orden (programa, car_innovacion).
CREATE OR ALTER PROCEDURE sp_listar_programa_ci
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite) t.*,
           m1.nombre AS programa_nombre,
           m2.nombre AS car_innovacion_nombre
      FROM programa_ci t
      LEFT JOIN programa m1 ON m1.id = t.programa
      LEFT JOIN car_innovacion m2 ON m2.id = t.car_innovacion
     WHERE t.activo = 1
     ORDER BY t.programa, t.car_innovacion;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_programa_ci
    @programa INT,
    @car_innovacion INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.*,
           m1.nombre AS programa_nombre,
           m2.nombre AS car_innovacion_nombre
      FROM programa_ci t
      LEFT JOIN programa m1 ON m1.id = t.programa
      LEFT JOIN car_innovacion m2 ON m2.id = t.car_innovacion
     WHERE t.programa = @programa AND t.car_innovacion = @car_innovacion AND t.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_asignar_programa_ci
    @programa INT,
    @car_innovacion INT
AS
BEGIN
    SET NOCOUNT ON;
    -- La FK de SQL Server no impide enlazar un padre retirado lógicamente.
    IF NOT EXISTS (SELECT 1 FROM programa WHERE id = @programa AND activo = 1)
        THROW 51000, 'El programa indicado no existe o está inactivo.', 1;
    -- La FK de SQL Server no impide enlazar un padre retirado lógicamente.
    IF NOT EXISTS (SELECT 1 FROM car_innovacion WHERE id = @car_innovacion AND activo = 1)
        THROW 51000, 'El car_innovacion indicado no existe o está inactivo.', 1;
    INSERT INTO programa_ci (programa, car_innovacion, activo)
    VALUES (@programa, @car_innovacion, 1);

    -- Igual que sp_consultar: el cliente recibe la fila tal como quedó guardada.
    SELECT t.*,
           m1.nombre AS programa_nombre,
           m2.nombre AS car_innovacion_nombre
      FROM programa_ci t
      LEFT JOIN programa m1 ON m1.id = t.programa
      LEFT JOIN car_innovacion m2 ON m2.id = t.car_innovacion
     WHERE t.programa = @programa AND t.car_innovacion = @car_innovacion AND t.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_retirar_programa_ci
    @programa INT,
    @car_innovacion INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE programa_ci
       SET activo = 0
     WHERE programa = @programa AND car_innovacion = @car_innovacion AND activo = 1;
    SELECT @@ROWCOUNT AS filas;
END
GO

-- an_programa: la clave compuesta siempre se usa en el orden (aspecto_normativo, programa).
CREATE OR ALTER PROCEDURE sp_listar_an_programa
    @limite INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@limite) t.*,
           m1.descripcion AS aspecto_normativo_nombre,
           m2.nombre AS programa_nombre
      FROM an_programa t
      LEFT JOIN aspecto_normativo m1 ON m1.id = t.aspecto_normativo
      LEFT JOIN programa m2 ON m2.id = t.programa
     WHERE t.activo = 1
     ORDER BY t.aspecto_normativo, t.programa;
END
GO

CREATE OR ALTER PROCEDURE sp_consultar_an_programa
    @aspecto_normativo INT,
    @programa INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.*,
           m1.descripcion AS aspecto_normativo_nombre,
           m2.nombre AS programa_nombre
      FROM an_programa t
      LEFT JOIN aspecto_normativo m1 ON m1.id = t.aspecto_normativo
      LEFT JOIN programa m2 ON m2.id = t.programa
     WHERE t.aspecto_normativo = @aspecto_normativo AND t.programa = @programa AND t.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_asignar_an_programa
    @aspecto_normativo INT,
    @programa INT
AS
BEGIN
    SET NOCOUNT ON;
    -- La FK de SQL Server no impide enlazar un padre retirado lógicamente.
    IF NOT EXISTS (SELECT 1 FROM aspecto_normativo WHERE id = @aspecto_normativo AND activo = 1)
        THROW 51000, 'El aspecto_normativo indicado no existe o está inactivo.', 1;
    -- La FK de SQL Server no impide enlazar un padre retirado lógicamente.
    IF NOT EXISTS (SELECT 1 FROM programa WHERE id = @programa AND activo = 1)
        THROW 51000, 'El programa indicado no existe o está inactivo.', 1;
    INSERT INTO an_programa (aspecto_normativo, programa, activo)
    VALUES (@aspecto_normativo, @programa, 1);

    -- Igual que sp_consultar: el cliente recibe la fila tal como quedó guardada.
    SELECT t.*,
           m1.descripcion AS aspecto_normativo_nombre,
           m2.nombre AS programa_nombre
      FROM an_programa t
      LEFT JOIN aspecto_normativo m1 ON m1.id = t.aspecto_normativo
      LEFT JOIN programa m2 ON m2.id = t.programa
     WHERE t.aspecto_normativo = @aspecto_normativo AND t.programa = @programa AND t.activo = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_retirar_an_programa
    @aspecto_normativo INT,
    @programa INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE an_programa
       SET activo = 0
     WHERE aspecto_normativo = @aspecto_normativo AND programa = @programa AND activo = 1;
    SELECT @@ROWCOUNT AS filas;
END
GO
