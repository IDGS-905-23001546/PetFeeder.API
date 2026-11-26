-- Migración: foto de perfil para usuarios
IF COL_LENGTH('dbo.usuarios', 'foto_url') IS NULL
BEGIN
    ALTER TABLE dbo.usuarios ADD foto_url NVARCHAR(MAX) NULL;
    PRINT 'Columna usuarios.foto_url creada.';
END
ELSE
    PRINT 'Columna usuarios.foto_url ya existía (omitida).';
GO
