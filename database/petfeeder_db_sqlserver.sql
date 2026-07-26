-- =============================================================================
-- PetFeeder / PawFeeder - Base de Datos (SQL Server / T-SQL)
-- Schema completo sin modulo de horarios de agua
-- Incluye: registros_agua_semanal para guardar ml dispensados por semana
-- Motor objetivo: SQL Server 2016+ (DROP TABLE IF EXISTS, CREATE OR ALTER VIEW)
-- =============================================================================
-- COMO USAR:
--   1) Abre este script en SSMS (o ejecuta con sqlcmd).
--   2) Ejecutalo completo. Crea la base petfeeder_db, las tablas, vistas y datos seed.
--   3) Re-ejecutable: borra y recrea las tablas (DROP IF EXISTS) en cada corrida.
-- =============================================================================


-- ============ 1) CREAR BASE DE DATOS ============
IF DB_ID('petfeeder_db') IS NULL
    CREATE DATABASE petfeeder_db;
GO

USE petfeeder_db;
GO


-- ============ 2) LIMPIAR TABLAS PREVIAS (orden inverso por dependencias) ============
DROP TABLE IF EXISTS registros_agua_semanal;
DROP TABLE IF EXISTS notificaciones;
DROP TABLE IF EXISTS telemetria_dispensador;
DROP TABLE IF EXISTS dispensaciones;
DROP TABLE IF EXISTS horarios;
DROP TABLE IF EXISTS dispensadores;
DROP TABLE IF EXISTS mascotas;
DROP TABLE IF EXISTS sesiones;
DROP TABLE IF EXISTS otp_verificacion;
DROP TABLE IF EXISTS usuarios;
GO


-- ============ TABLA: usuarios ============
CREATE TABLE usuarios (
    id              INT             IDENTITY(1,1) NOT NULL,
    nombre          NVARCHAR(100)   NOT NULL,
    email           NVARCHAR(150)   NOT NULL,
    telefono        NVARCHAR(20)        NULL,
    password_hash   NVARCHAR(255)   NOT NULL,
    verificado      BIT             NOT NULL DEFAULT 0,   -- 0=pendiente, 1=verificado
    activo          BIT             NOT NULL DEFAULT 1,
    created_at      DATETIME2       NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT pk_usuarios PRIMARY KEY (id)
);
CREATE UNIQUE INDEX uq_usuarios_email ON usuarios (email);
GO


-- ============ TABLA: otp_verificacion ============
CREATE TABLE otp_verificacion (
    id              INT             IDENTITY(1,1) NOT NULL,
    usuario_id      INT             NOT NULL,
    codigo          CHAR(6)         NOT NULL,
    intentos        TINYINT         NOT NULL DEFAULT 0,
    max_intentos    TINYINT         NOT NULL DEFAULT 3,
    expira_en       DATETIME2       NOT NULL,
    usado           BIT             NOT NULL DEFAULT 0,
    created_at      DATETIME2       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT pk_otp PRIMARY KEY (id),
    CONSTRAINT fk_otp_usuario FOREIGN KEY (usuario_id)
        REFERENCES usuarios (id) ON DELETE CASCADE
);
CREATE INDEX idx_otp_usuario ON otp_verificacion (usuario_id);
CREATE INDEX idx_otp_codigo  ON otp_verificacion (codigo);
GO


-- ============ TABLA: sesiones ============
CREATE TABLE sesiones (
    id              INT             IDENTITY(1,1) NOT NULL,
    usuario_id      INT             NOT NULL,
    token           NVARCHAR(512)   NOT NULL,              -- JWT o token opaco
    dispositivo     NVARCHAR(200)       NULL,
    ip_origen       NVARCHAR(45)        NULL,
    activa          BIT             NOT NULL DEFAULT 1,
    expira_en       DATETIME2           NULL,
    created_at      DATETIME2       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT pk_sesiones PRIMARY KEY (id),
    CONSTRAINT fk_sesion_usuario FOREIGN KEY (usuario_id)
        REFERENCES usuarios (id) ON DELETE CASCADE
);
CREATE INDEX idx_sesion_usuario ON sesiones (usuario_id);
GO


-- ============ TABLA: mascotas ============
CREATE TABLE mascotas (
    id              INT             IDENTITY(1,1) NOT NULL,
    usuario_id      INT             NOT NULL,
    nombre          NVARCHAR(100)   NOT NULL,
    raza            NVARCHAR(100)   NOT NULL,
    edad_anos       TINYINT         NOT NULL DEFAULT 0,
    edad_meses      SMALLINT        NOT NULL DEFAULT 0,    -- Edad en meses para calculo de porciones
    peso_kg         DECIMAL(5,2)    NOT NULL DEFAULT 0.00,
    tamano          NVARCHAR(20)    NOT NULL DEFAULT N'mediano',
    activa          BIT             NOT NULL DEFAULT 0,    -- solo 1 activa por usuario
    foto_uri        NVARCHAR(500)       NULL,
    created_at      DATETIME2       NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT pk_mascotas PRIMARY KEY (id),
    CONSTRAINT ck_mascotas_tamano CHECK (tamano IN (N'pequeño', N'mediano', N'grande', N'gigante')),
    CONSTRAINT fk_mascotas_usuario FOREIGN KEY (usuario_id)
        REFERENCES usuarios (id) ON DELETE CASCADE
);
CREATE INDEX idx_mascotas_usuario ON mascotas (usuario_id);
CREATE INDEX idx_mascotas_activa  ON mascotas (usuario_id, activa);
GO


-- ============ TABLA: dispensadores ============
CREATE TABLE dispensadores (
    id               INT            IDENTITY(1,1) NOT NULL,
    usuario_id       INT            NOT NULL,
    nombre           NVARCHAR(100)  NOT NULL,
    codigo_unico     NVARCHAR(50)   NOT NULL,              -- impreso en el hardware
    firmware_version NVARCHAR(20)   NOT NULL DEFAULT N'v1.0.0',
    estado           NVARCHAR(20)   NOT NULL DEFAULT N'offline',
    bateria_percent  TINYINT        NOT NULL DEFAULT 100,  -- 0-100%
    nivel_tolva_pct  TINYINT        NOT NULL DEFAULT 60,   -- 0-100%
    ssid_wifi        NVARCHAR(100)      NULL,
    activo           BIT            NOT NULL DEFAULT 1,
    last_ping_at     DATETIME2          NULL,
    created_at       DATETIME2      NOT NULL DEFAULT GETDATE(),
    updated_at       DATETIME2      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT pk_dispensadores PRIMARY KEY (id),
    CONSTRAINT ck_dispensador_estado CHECK (estado IN (N'activo', N'offline', N'emparejando')),
    CONSTRAINT fk_dispensador_usuario FOREIGN KEY (usuario_id)
        REFERENCES usuarios (id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX uq_dispensador_codigo ON dispensadores (codigo_unico);
CREATE INDEX idx_dispensador_usuario ON dispensadores (usuario_id);
GO


-- ============ TABLA: horarios ============
CREATE TABLE horarios (
    id              INT             IDENTITY(1,1) NOT NULL,
    usuario_id      INT             NOT NULL,
    mascota_id      INT                 NULL,              -- NULL = mascota activa
    dispensador_id  INT                 NULL,              -- NULL = dispensador activo
    nombre          NVARCHAR(50)    NOT NULL,
    icono           NVARCHAR(20)    NOT NULL DEFAULT N'sun',
    hora            NVARCHAR(10)    NOT NULL,              -- "07:30 AM"
    lunes           BIT             NOT NULL DEFAULT 0,
    martes          BIT             NOT NULL DEFAULT 0,
    miercoles       BIT             NOT NULL DEFAULT 0,
    jueves          BIT             NOT NULL DEFAULT 0,
    viernes         BIT             NOT NULL DEFAULT 0,
    sabado          BIT             NOT NULL DEFAULT 0,
    domingo         BIT             NOT NULL DEFAULT 0,
    porcion_gramos  DECIMAL(6,1)    NOT NULL DEFAULT 100.0,
    activo          BIT             NOT NULL DEFAULT 1,
    created_at      DATETIME2       NOT NULL DEFAULT GETDATE(),
    updated_at      DATETIME2       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT pk_horarios PRIMARY KEY (id),
    CONSTRAINT fk_horarios_usuario FOREIGN KEY (usuario_id)
        REFERENCES usuarios (id) ON DELETE CASCADE,
    CONSTRAINT fk_horarios_mascota FOREIGN KEY (mascota_id)
        REFERENCES mascotas (id),
    CONSTRAINT fk_horarios_dispensador FOREIGN KEY (dispensador_id)
        REFERENCES dispensadores (id)
);
CREATE INDEX idx_horarios_usuario ON horarios (usuario_id);
CREATE INDEX idx_horarios_activo  ON horarios (usuario_id, activo);
GO


-- ============ TABLA: dispensaciones ============
CREATE TABLE dispensaciones (
    id              INT             IDENTITY(1,1) NOT NULL,
    usuario_id      INT             NOT NULL,
    mascota_id      INT                 NULL,
    dispensador_id  INT                 NULL,
    horario_id      INT                 NULL,              -- NULL si fue manual
    tipo            NVARCHAR(20)    NOT NULL DEFAULT N'manual',
    nombre          NVARCHAR(100)   NOT NULL DEFAULT N'Manual',
    porcion_gramos  DECIMAL(6,1)    NOT NULL,
    fecha_hora      DATETIME2       NOT NULL,
    estado          NVARCHAR(20)    NOT NULL DEFAULT N'ejecutada',
    created_at      DATETIME2       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT pk_dispensaciones PRIMARY KEY (id),
    CONSTRAINT ck_disp_tipo   CHECK (tipo   IN (N'programada', N'manual')),
    CONSTRAINT ck_disp_estado CHECK (estado IN (N'ejecutada', N'fallida', N'pendiente')),
    CONSTRAINT fk_disp_usuario FOREIGN KEY (usuario_id)
        REFERENCES usuarios (id) ON DELETE CASCADE,
    CONSTRAINT fk_disp_mascota FOREIGN KEY (mascota_id)
        REFERENCES mascotas (id),
    CONSTRAINT fk_disp_dispensador FOREIGN KEY (dispensador_id)
        REFERENCES dispensadores (id),
    CONSTRAINT fk_disp_horario FOREIGN KEY (horario_id)
        REFERENCES horarios (id)
);
CREATE INDEX idx_disp_usuario ON dispensaciones (usuario_id);
CREATE INDEX idx_disp_fecha   ON dispensaciones (usuario_id, fecha_hora);
CREATE INDEX idx_disp_mascota ON dispensaciones (mascota_id);
CREATE INDEX idx_disp_horario ON dispensaciones (horario_id);
GO


-- ============ TABLA: telemetria_dispensador ============
CREATE TABLE telemetria_dispensador (
    id              BIGINT          IDENTITY(1,1) NOT NULL,
    dispensador_id  INT             NOT NULL,
    bateria_percent TINYINT         NOT NULL,
    nivel_tolva_pct TINYINT         NOT NULL,
    estado          NVARCHAR(20)    NOT NULL DEFAULT N'activo',
    registrado_en   DATETIME2       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT pk_telemetria PRIMARY KEY (id),
    CONSTRAINT fk_telemetria_dispensador FOREIGN KEY (dispensador_id)
        REFERENCES dispensadores (id) ON DELETE CASCADE
);
CREATE INDEX idx_telemetria_disp ON telemetria_dispensador (dispensador_id, registrado_en);
GO


-- ============ TABLA: notificaciones ============
CREATE TABLE notificaciones (
    id              INT             IDENTITY(1,1) NOT NULL,
    usuario_id      INT             NOT NULL,
    dispensador_id  INT                 NULL,
    tipo            NVARCHAR(30)    NOT NULL DEFAULT N'otro',
    titulo          NVARCHAR(200)   NOT NULL,
    mensaje         NVARCHAR(MAX)       NULL,
    leida           BIT             NOT NULL DEFAULT 0,
    created_at      DATETIME2       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT pk_notificaciones PRIMARY KEY (id),
    CONSTRAINT ck_notif_tipo CHECK (tipo IN (N'tolva_baja', N'bateria_critica', N'dispensa_ok',
                                             N'dispensa_fallida', N'dispositivo_offline', N'otro')),
    CONSTRAINT fk_notif_usuario FOREIGN KEY (usuario_id)
        REFERENCES usuarios (id) ON DELETE CASCADE,
    CONSTRAINT fk_notif_dispensador FOREIGN KEY (dispensador_id)
        REFERENCES dispensadores (id)
);
CREATE INDEX idx_notif_usuario ON notificaciones (usuario_id, leida);
GO


-- ============ TABLA: registros_agua_semanal ============
-- Guarda cuantos ml de agua dispensa el prototipo por semana.
-- Cada registro es un evento de agua (el ESP32 dispensa y reporta).
CREATE TABLE registros_agua_semanal (
    id              INT             IDENTITY(1,1) NOT NULL,
    usuario_id      INT             NOT NULL,
    dispensador_id  INT                 NULL,
    cantidad_ml     DECIMAL(7,1)    NOT NULL,              -- ml dispensados en este evento
    semana_anio     TINYINT         NOT NULL,              -- numero de semana (1-53)
    anio            SMALLINT        NOT NULL,              -- anio del registro
    fecha_registro  DATETIME2       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT pk_registros_agua PRIMARY KEY (id),
    CONSTRAINT fk_regagua_usuario FOREIGN KEY (usuario_id)
        REFERENCES usuarios (id) ON DELETE CASCADE,
    CONSTRAINT fk_regagua_dispensador FOREIGN KEY (dispensador_id)
        REFERENCES dispensadores (id)
);
CREATE INDEX idx_regagua_usuario ON registros_agua_semanal (usuario_id);
CREATE INDEX idx_regagua_semana  ON registros_agua_semanal (usuario_id, anio, semana_anio);
GO


-- =============================================================================
-- DATOS DE PRUEBA (SEED)
-- =============================================================================

-- Usuario admin de prueba.
--   email:      carlosriosrmz17@gmail.com
--   contrasena: CarlosRMZ17   (hash BCrypt real; ya queda verificado para hacer login)
INSERT INTO usuarios (nombre, email, password_hash, verificado, activo) VALUES
(N'Carlos', N'carlosriosrmz17@gmail.com',
 '$2a$11$xRDdvigbrklRx7Mlfmz5NObDQunI71SBzSwDeBT0Ar0nYocjEZZCe', 1, 1);

-- Mascota de prueba (Golden Retriever, grande, 2 anos = 24 meses)
INSERT INTO mascotas (usuario_id, nombre, raza, edad_anos, edad_meses, peso_kg, tamano, activa) VALUES
(1, N'Max', N'Golden Retriever', 2, 24, 28.50, N'grande', 1);

-- Dispensador de prueba
INSERT INTO dispensadores (usuario_id, nombre, codigo_unico, firmware_version, estado, bateria_percent, nivel_tolva_pct, ssid_wifi) VALUES
(1, N'PawFeeder Casa', N'PF-2024-001', N'v1.0.0', N'activo', 85, 65, N'MiWifi_Casa');

-- Horarios de prueba (Desayuno y Cena, todos los dias)
INSERT INTO horarios (usuario_id, mascota_id, dispensador_id, nombre, icono, hora, lunes, martes, miercoles, jueves, viernes, sabado, domingo, porcion_gramos) VALUES
(1, 1, 1, N'Desayuno', N'sun',  N'07:30 AM', 1, 1, 1, 1, 1, 1, 1, 140.0),
(1, 1, 1, N'Cena',     N'moon', N'06:00 PM', 1, 1, 1, 1, 1, 1, 1, 140.0);
GO


-- =============================================================================
-- VISTAS UTILES
-- =============================================================================

-- Resumen del dashboard para el home (Principal.kt)
CREATE OR ALTER VIEW v_dashboard_usuario AS
SELECT
    u.id            AS usuario_id,
    u.nombre        AS usuario_nombre,
    m.id            AS mascota_id,
    m.nombre        AS mascota_nombre,
    m.raza,
    m.tamano,
    m.peso_kg,
    m.edad_anos,
    m.edad_meses,
    d.id            AS dispensador_id,
    d.estado        AS dispensador_estado,
    d.bateria_percent,
    d.nivel_tolva_pct,
    ROUND(d.nivel_tolva_pct / 100.0 * 4, 2) AS tolva_kg,
    CASE m.tamano
        WHEN N'pequeño' THEN 80
        WHEN N'mediano' THEN 180
        WHEN N'grande'  THEN 280
        WHEN N'gigante' THEN 450
        ELSE 180
    END             AS porcion_recomendada_g
FROM usuarios u
LEFT JOIN mascotas m      ON m.usuario_id = u.id AND m.activa = 1
LEFT JOIN dispensadores d ON d.usuario_id = u.id AND d.activo = 1;
GO

-- Resumen de dispensaciones del dia actual
CREATE OR ALTER VIEW v_dispensaciones_hoy AS
SELECT
    usuario_id,
    COUNT(*)            AS total_dispensaciones,
    SUM(porcion_gramos) AS total_gramos_hoy,
    MIN(fecha_hora)     AS primera_dispensa,
    MAX(fecha_hora)     AS ultima_dispensa
FROM dispensaciones
WHERE CAST(fecha_hora AS DATE) = CAST(GETDATE() AS DATE)
  AND estado = N'ejecutada'
GROUP BY usuario_id;
GO

-- Proxima dispensa programada por usuario
CREATE OR ALTER VIEW v_proxima_dispensa AS
SELECT
    h.usuario_id,
    h.id        AS horario_id,
    h.nombre    AS nombre_horario,
    h.hora,
    h.porcion_gramos,
    CASE DATEPART(WEEKDAY, GETDATE())
        WHEN 1 THEN h.domingo
        WHEN 2 THEN h.lunes
        WHEN 3 THEN h.martes
        WHEN 4 THEN h.miercoles
        WHEN 5 THEN h.jueves
        WHEN 6 THEN h.viernes
        WHEN 7 THEN h.sabado
    END         AS activo_hoy
FROM horarios h
WHERE h.activo = 1;
GO

-- =============================================================================
-- FIN DEL ESQUEMA
-- =============================================================================
select * from usuarios;