IF DB_ID(N'EnflorarteTopiProyectoDb') IS NULL
BEGIN
    CREATE DATABASE EnflorarteTopiProyectoDb;
END
GO

USE EnflorarteTopiProyectoDb;
GO

-- Usuarios
CREATE TABLE dbo.usuario (
    usuario_id INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
    rol NVARCHAR(20) NOT NULL,
    contrasena NVARCHAR(255) NOT NULL,
    activo BIT NOT NULL DEFAULT(1)
);
GO
-- enums del usuario.
ALTER TABLE dbo.usuario
    ADD CONSTRAINT chk_usuario_rol CHECK (rol IN (N'supervisor', N'ventas', N'florista', N'repartidor'));
GO

-- Flores (catálogo)
CREATE TABLE dbo.flor (
    flor_id INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
    foto_ruta NVARCHAR(300) NULL,
    descripcion NVARCHAR(500) NULL
);
GO

CREATE TABLE dbo.flor_inventario_color (
    flor_inventario_color_id INT IDENTITY(1,1) PRIMARY KEY,
    flor_id INT NOT NULL,
    color NVARCHAR(50) NOT NULL,
    cantidad INT NOT NULL DEFAULT(0),

    CONSTRAINT fk_flor_inventario_color_flor FOREIGN KEY (flor_id)
        REFERENCES dbo.flor(flor_id)
        ON DELETE CASCADE
        ON UPDATE NO ACTION,
    CONSTRAINT ck_flor_inventario_color_cantidad_nonnegative CHECK (cantidad >= 0),
    CONSTRAINT uq_flor_inventario_color UNIQUE (flor_id, color)
);
GO

-- Arreglos (catálogo)
CREATE TABLE dbo.arreglo (
    arreglo_id INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
    foto_ruta NVARCHAR(300) NULL,
    descripcion NVARCHAR(500) NULL
);
GO

CREATE TABLE dbo.arreglo_flor (
    arreglo_id INT NOT NULL,
    flor_id INT NOT NULL,
    cantidad INT NOT NULL,
    color_seleccionado NVARCHAR(50) NOT NULL DEFAULT(N'a elegir'),

    CONSTRAINT pk_arreglo_flor PRIMARY KEY (arreglo_id, flor_id),
    CONSTRAINT fk_arreglo_flor_arreglo FOREIGN KEY (arreglo_id)
        REFERENCES dbo.arreglo(arreglo_id)
        ON DELETE CASCADE
        ON UPDATE NO ACTION,
    CONSTRAINT fk_arreglo_flor_flor FOREIGN KEY (flor_id)
        REFERENCES dbo.flor(flor_id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    CONSTRAINT chk_arreglo_flor_cantidad CHECK (cantidad > 0)
);
GO



-- Comandas
CREATE TABLE dbo.comanda (
    comanda_id INT IDENTITY(1,1) PRIMARY KEY,

    usuario_id INT NOT NULL, -- usuario creador de la comanda.

    cliente_nombre NVARCHAR(100) NOT NULL,
    cliente_telefono NVARCHAR(20) NULL,
    direccion_entrega NVARCHAR(255) NULL,

    fecha_entrega DATE NOT NULL,
    hora_entrega TIME NULL,

    tipo_entrega NVARCHAR(10) NOT NULL, -- envio o recoger.
    nombre_arreglo NVARCHAR(150) NOT NULL,
    precio_arreglo DECIMAL(10,2) NOT NULL,

    foto_arreglo_ruta NVARCHAR(300) NULL,

    estado NVARCHAR(20) NOT NULL DEFAULT (N'pendiente'), -- pendiente, en_proceso, listo y entregado.
    liquidado BIT NOT NULL DEFAULT(0),

    anticipo_total DECIMAL(10,2) NOT NULL DEFAULT(0),
    anticipo_tipo NVARCHAR(50) NULL,

    pago_envio DECIMAL(10,2) NOT NULL DEFAULT(0),
    pago_arreglo DECIMAL(10,2) NOT NULL DEFAULT(0), -- El pago por envio y el pago del arreglo son separados. Aunque el pago del arreglo es el precio "original" por asi decirlo.


    repartidor_id INT NULL, -- usuario asignado, que tiene rol de repartidor.

    CONSTRAINT fk_comanda_usuario FOREIGN KEY (usuario_id)
        REFERENCES dbo.usuario(usuario_id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,

    CONSTRAINT fk_comanda_repartidor FOREIGN KEY (repartidor_id)
        REFERENCES dbo.usuario(usuario_id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
);
GO
-- enums de la comanda.
ALTER TABLE dbo.comanda
    ADD CONSTRAINT chk_comanda_tipo_entrega CHECK (tipo_entrega IN (N'envio', N'recoger', N'otro'));
GO

ALTER TABLE dbo.comanda
    ADD CONSTRAINT chk_comanda_estado CHECK (estado IN (N'solicitado', N'cancelado', N'pendiente', N'en_proceso', N'listo', N'entregado'));
GO

ALTER TABLE dbo.comanda
    ADD CONSTRAINT chk_comanda_anticipo_tipo CHECK (anticipo_tipo IN (N'porcentaje', N'minimo', N'manual') OR anticipo_tipo IS NULL);
GO




-- Sabemos que la florista va a realizar muchas consultas filtrando por fecha de entrega y estado, por lo que los indexes van a acelerar las consultas en estos datos especifocos.
CREATE INDEX idx_comanda_fecha ON dbo.comanda(fecha_entrega);
CREATE INDEX idx_comanda_estado ON dbo.comanda(estado);
GO