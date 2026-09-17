-- Agrega la columna `activo` (BIT NOT NULL DEFAULT 1) a las 7 tablas sin FK de la v1.
-- Seguro de repetir: IF COL_LENGTH(...) IS NULL evita duplicar la columna.
-- Fuente: docs/spec_kit/versiones/v1_producto_sqlserver/5_data_model.md, Seccion 3.

USE [innovacion_curricular];
GO

IF COL_LENGTH('area_conocimiento', 'activo') IS NULL
    ALTER TABLE area_conocimiento ADD activo BIT NOT NULL DEFAULT 1;

IF COL_LENGTH('universidad', 'activo') IS NULL
    ALTER TABLE universidad ADD activo BIT NOT NULL DEFAULT 1;

IF COL_LENGTH('aspecto_normativo', 'activo') IS NULL
    ALTER TABLE aspecto_normativo ADD activo BIT NOT NULL DEFAULT 1;

IF COL_LENGTH('practica_estrategia', 'activo') IS NULL
    ALTER TABLE practica_estrategia ADD activo BIT NOT NULL DEFAULT 1;

IF COL_LENGTH('enfoque', 'activo') IS NULL
    ALTER TABLE enfoque ADD activo BIT NOT NULL DEFAULT 1;

IF COL_LENGTH('car_innovacion', 'activo') IS NULL
    ALTER TABLE car_innovacion ADD activo BIT NOT NULL DEFAULT 1;

IF COL_LENGTH('aliado', 'activo') IS NULL
    ALTER TABLE aliado ADD activo BIT NOT NULL DEFAULT 1;
GO