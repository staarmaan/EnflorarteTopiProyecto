-- PostgreSQL Schema for Enflorarte Topi Proyecto

-- Drop existing tables if they exist (for development/testing)
DROP INDEX IF EXISTS idx_comanda_estado;
DROP INDEX IF EXISTS idx_comanda_fecha;
DROP TABLE IF EXISTS comanda CASCADE;
DROP TABLE IF EXISTS usuario CASCADE;

-- Usuarios table
CREATE TABLE usuario (
    usuario_id SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    rol VARCHAR(20) NOT NULL,
    contrasena VARCHAR(255) NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT true
);

-- Check constraint for usuario roles
ALTER TABLE usuario
    ADD CONSTRAINT chk_usuario_rol CHECK (rol IN ('supervisor', 'vendedor', 'florista', 'repartidor'));

-- Comandas table
CREATE TABLE comanda (
    comanda_id SERIAL PRIMARY KEY,
    
    usuario_id INT NOT NULL,
    repartidor_id INT NULL,
    
    cliente_nombre VARCHAR(100) NOT NULL,
    cliente_telefono VARCHAR(20) NULL,
    direccion_entrega VARCHAR(255) NULL,
    
    fecha_entrega DATE NOT NULL,
    hora_entrega INTERVAL NULL,
    
    tipo_entrega VARCHAR(10) NOT NULL,
    nombre_arreglo VARCHAR(150) NOT NULL,
    precio_arreglo NUMERIC(7, 2) NOT NULL,
    
    foto_arreglo_ruta VARCHAR(300) NULL,
    
    estado VARCHAR(20) NOT NULL DEFAULT 'solicitado',
    liquidado BOOLEAN NOT NULL DEFAULT false,
    
    anticipo_total NUMERIC(7, 2) NOT NULL DEFAULT 0,
    anticipo_tipo VARCHAR(50) NULL,
    
    pago_envio NUMERIC(7, 2) NOT NULL DEFAULT 0,
    
    CONSTRAINT fk_comanda_usuario_creator FOREIGN KEY (usuario_id)
        REFERENCES usuario(usuario_id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    
    CONSTRAINT fk_comanda_repartidor FOREIGN KEY (repartidor_id)
        REFERENCES usuario(usuario_id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
);

-- Check constraints for comanda
ALTER TABLE comanda
    ADD CONSTRAINT chk_comanda_tipo_entrega CHECK (tipo_entrega IN ('envio', 'recoger', 'otro'));

ALTER TABLE comanda
    ADD CONSTRAINT chk_comanda_estado CHECK (estado IN ('solicitado', 'cancelado', 'pendiente', 'en_proceso', 'listo', 'entregado'));

ALTER TABLE comanda
    ADD CONSTRAINT chk_comanda_anticipo_tipo CHECK (anticipo_tipo IS NULL OR anticipo_tipo IN ('porcentaje', 'minimo', 'manual'));

-- Indexes for common queries (florista queries)
CREATE INDEX idx_comanda_fecha ON comanda(fecha_entrega);
CREATE INDEX idx_comanda_estado ON comanda(estado);
