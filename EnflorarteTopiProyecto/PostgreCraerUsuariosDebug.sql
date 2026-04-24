-- PostgreSQL INSERT command to create a user row in the usuario table

-- Example: Create a new user
INSERT INTO usuario (nombre, rol, contrasena, activo)
VALUES ('Juan Carlos', 'supervisor', 'admin', true);

-- Additional example users for testing:
-- INSERT INTO usuario (nombre, rol, contrasena, activo) VALUES ('Maria', 'vendedor', 'hashed_password', true);
-- INSERT INTO usuario (nombre, rol, contrasena, activo) VALUES ('Pedro', 'florista', 'hashed_password', true);
-- INSERT INTO usuario (nombre, rol, contrasena, activo) VALUES ('Ana', 'repartidor', 'hashed_password', true);

-- Note: 
-- - usuario_id is auto-generated (SERIAL PRIMARY KEY), so you don't need to specify it
-- - rol must be one of: 'supervisor', 'vendedor', 'florista', 'repartidor'
-- - contrasena should be hashed using the hash algorithm (see HasheadorContrasenas.cs)
-- - activo defaults to true if not specified