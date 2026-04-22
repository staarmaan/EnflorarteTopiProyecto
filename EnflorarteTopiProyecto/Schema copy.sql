-- PostgreSQL Schema for EnflorarteTopiProyecto
-- 
-- To use this script:
-- 1. Create the database first (in psql or your PostgreSQL client):
--    CREATE DATABASE "EnflorarteBD";
-- 
-- 2. Connect to the database:
--    \c EnflorarteBD
-- 
-- 3. Then run this script
-- 
-- Or execute directly with: psql -U username -d EnflorarteBD -f "Schema copy.sql"

-- Usuarios
CREATE TABLE IF NOT EXISTS usuario (
    usuario_id SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    rol VARCHAR(20) NOT NULL,
    contrasena VARCHAR(255) NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT chk_usuario_rol CHECK (rol IN ('supervisor', 'vendedor', 'florista', 'repartidor'))
);


-- Comandas
CREATE TABLE IF NOT EXISTS comanda (
    comanda_id SERIAL PRIMARY KEY,
    cliente_nombre VARCHAR(100) NOT NULL,
    cliente_telefono VARCHAR(20) NULL,
    direccion_entrega VARCHAR(255) NULL,
    fecha_entrega DATE NOT NULL,
    hora_entrega TIME NULL,
    tipo_entrega VARCHAR(10) NOT NULL,
    nombre_arreglo VARCHAR(150) NOT NULL,
    precio_arreglo NUMERIC(10,2) NOT NULL,
    foto_arreglo VARCHAR(300) NULL,
    estado VARCHAR(20) NOT NULL DEFAULT 'pendiente',
    liquidado BOOLEAN NOT NULL DEFAULT FALSE,
    anticipo_total NUMERIC(10,2) NOT NULL DEFAULT 0,
    anticipo_tipo VARCHAR(50) NULL,
    pago_envio NUMERIC(10,2) NOT NULL DEFAULT 0,
    pago_arreglo NUMERIC(10,2) NOT NULL DEFAULT 0,
    repartidor_id INT NULL,
    CONSTRAINT fk_comanda_repartidor FOREIGN KEY (repartidor_id)
        REFERENCES usuario(usuario_id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    CONSTRAINT chk_comanda_tipo_entrega CHECK (tipo_entrega IN ('envio', 'recoger')),
    CONSTRAINT chk_comanda_estado CHECK (estado IN ('pendiente', 'en_proceso', 'listo', 'entregado')),
    CONSTRAINT chk_comanda_anticipo_tipo CHECK (anticipo_tipo IN ('porcentaje', 'minimo', 'manual'))
);

-- Indexes for performance optimization
CREATE INDEX idx_comanda_fecha ON comanda(fecha_entrega);
CREATE INDEX idx_comanda_estado ON comanda(estado);