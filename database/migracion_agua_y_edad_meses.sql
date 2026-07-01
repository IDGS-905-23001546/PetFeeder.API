-- =============================================================================
-- MIGRACIÓN ADITIVA para petfeeder_db (SQL Server / T-SQL)  -- 2026-07-01
-- Agrega mascotas.edad_meses + tablas horarios_agua y dispensaciones_agua.
-- Idempotente: no borra datos; si algo ya existe, lo omite.
-- =============================================================================
USE petfeeder_db;
GO

-- ============ 1) mascotas.edad_meses ============
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

-- ============ 2) TABLA: horarios_agua ============
IF OBJECT_ID('dbo.horarios_agua', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.horarios_agua (
        id              INT             IDENTITY(1,1) NOT NULL,
        usuario_id      INT             NOT NULL,
        mascota_id      INT                 NULL,
        dispensador_id  INT                 NULL,
        nombre          NVARCHAR(50)    NOT NULL,
        icono           NVARCHAR(20)    NOT NULL DEFAULT N'water',
        hora            NVARCHAR(10)    NOT NULL,
        lunes           BIT             NOT NULL DEFAULT 0,
        martes          BIT             NOT NULL DEFAULT 0,
        miercoles       BIT             NOT NULL DEFAULT 0,
        jueves          BIT             NOT NULL DEFAULT 0,
        viernes         BIT             NOT NULL DEFAULT 0,
        sabado          BIT             NOT NULL DEFAULT 0,
        domingo         BIT             NOT NULL DEFAULT 0,
        cantidad_ml     DECIMAL(7,1)    NOT NULL DEFAULT 200.0,
        activo          BIT             NOT NULL DEFAULT 1,
        created_at      DATETIME2       NOT NULL DEFAULT GETDATE(),
        updated_at      DATETIME2       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT pk_horarios_agua PRIMARY KEY (id),
        CONSTRAINT fk_horarios_agua_usuario FOREIGN KEY (usuario_id)
            REFERENCES dbo.usuarios (id) ON DELETE CASCADE,
        CONSTRAINT fk_horarios_agua_mascota FOREIGN KEY (mascota_id)
            REFERENCES dbo.mascotas (id),
        CONSTRAINT fk_horarios_agua_dispensador FOREIGN KEY (dispensador_id)
            REFERENCES dbo.dispensadores (id)
    );
    CREATE INDEX idx_horarios_agua_usuario ON dbo.horarios_agua (usuario_id);
    CREATE INDEX idx_horarios_agua_activo  ON dbo.horarios_agua (usuario_id, activo);
    PRINT 'Tabla horarios_agua creada.';
END
ELSE
    PRINT 'Tabla horarios_agua ya existia (omitida).';
GO

-- ============ 3) TABLA: dispensaciones_agua ============
IF OBJECT_ID('dbo.dispensaciones_agua', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.dispensaciones_agua (
        id                INT             IDENTITY(1,1) NOT NULL,
        usuario_id        INT             NOT NULL,
        mascota_id        INT                 NULL,
        dispensador_id    INT                 NULL,
        horario_agua_id   INT                 NULL,
        tipo              NVARCHAR(20)    NOT NULL DEFAULT N'manual',
        nombre            NVARCHAR(100)   NOT NULL DEFAULT N'Manual',
        cantidad_ml       DECIMAL(7,1)    NOT NULL,
        fecha_hora        DATETIME2       NOT NULL,
        estado            NVARCHAR(20)    NOT NULL DEFAULT N'ejecutada',
        created_at        DATETIME2       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT pk_dispensaciones_agua PRIMARY KEY (id),
        CONSTRAINT ck_disp_agua_tipo   CHECK (tipo   IN (N'programada', N'manual')),
        CONSTRAINT ck_disp_agua_estado CHECK (estado IN (N'ejecutada', N'fallida', N'pendiente')),
        CONSTRAINT fk_disp_agua_usuario FOREIGN KEY (usuario_id)
            REFERENCES dbo.usuarios (id) ON DELETE CASCADE,
        CONSTRAINT fk_disp_agua_mascota FOREIGN KEY (mascota_id)
            REFERENCES dbo.mascotas (id),
        CONSTRAINT fk_disp_agua_dispensador FOREIGN KEY (dispensador_id)
            REFERENCES dbo.dispensadores (id),
        CONSTRAINT fk_disp_agua_horario FOREIGN KEY (horario_agua_id)
            REFERENCES dbo.horarios_agua (id)
    );
    CREATE INDEX idx_disp_agua_usuario ON dbo.dispensaciones_agua (usuario_id);
    CREATE INDEX idx_disp_agua_fecha   ON dbo.dispensaciones_agua (usuario_id, fecha_hora);
    PRINT 'Tabla dispensaciones_agua creada.';
END
ELSE
    PRINT 'Tabla dispensaciones_agua ya existia (omitida).';
GO

PRINT '=== Migracion completada ===';
GO
