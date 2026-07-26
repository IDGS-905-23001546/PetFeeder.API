-- =============================================================================
-- MIGRACIÓN ADITIVA para petfeeder_db (SQL Server / T-SQL)  -- 2026-07-01
-- Agrega mascotas.edad_meses
-- Idempotente: no borra datos; si algo ya existe, lo omite.
-- =============================================================================
USE petfeeder_db;
GO

-- ============ mascotas.edad_meses ============
IF COL_LENGTH('dbo.mascotas', 'edad_meses') IS NULL
BEGIN
    ALTER TABLE dbo.mascotas ADD edad_meses SMALLINT NOT NULL DEFAULT 0;
    PRINT 'Columna mascotas.edad_meses creada.';
END
ELSE
    PRINT 'Columna mascotas.edad_meses ya existia (omitida).';
GO

-- Backfill: convierte anios ya guardados a meses (solo donde aun esta en 0)
UPDATE dbo.mascotas
   SET edad_meses = edad_anos * 12
 WHERE edad_meses = 0 AND edad_anos > 0;
GO

PRINT '=== Migracion completada ===';
GO
