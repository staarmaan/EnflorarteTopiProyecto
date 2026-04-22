USE EnflorarteTopiProyectoDb;
GO

-- Actualizamos la contraseña a un hash que representa "123456"
UPDATE dbo.usuario 
SET contrasena = '$2a$11$ev6X/.Lpx9m8GskS.XpuneYyvT/iI8WkOsgN.XpYI3lB9vW.fG7yG' 
WHERE nombre IN ('Saul', 'Saul_Admin');
GO