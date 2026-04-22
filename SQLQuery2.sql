USE EnflorarteTopiProyectoDb;
GO

-- 1. Creamos la tabla 'usuario' (con la estructura que espera tu ApplicationDbContext)
IF OBJECT_ID('dbo.usuario', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.usuario (
        usuario_id INT IDENTITY(1,1) PRIMARY KEY,
        nombre NVARCHAR(200) NOT NULL,
        rol NVARCHAR(20) NOT NULL,
        contrasena NVARCHAR(200) NOT NULL,
        activo BIT NOT NULL DEFAULT(1),
        CONSTRAINT chk_usuario_rol CHECK (rol IN (N'supervisor', N'vendedor', N'florista', N'repartidor'))
    );
END
GO

-- 2. Insertamos al usuario supervisor
-- NOTA: Si tu app usa el 'HasheadorContrasenas', el login fallará con '123456'.
-- Por ahora lo ponemos así para que la tabla tenga datos.
INSERT INTO dbo.usuario (nombre, rol, contrasena, activo)
VALUES ('Saul_Admin', 'supervisor', '123456', 1);
GO