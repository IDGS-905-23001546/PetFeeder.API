-- =============================================================================
-- Seed para PostgreSQL (Render)
-- Las tablas las crea EF Core automaticamente con EnsureCreated().
-- Este script solo inserta datos de prueba.
-- =============================================================================

INSERT INTO usuarios (nombre, email, password_hash, verificado, activo) VALUES
('Carlos', 'carlosriosrmz17@gmail.com',
 '$2a$11$xRDdvigbrklRx7Mlfmz5NObDQunI71SBzSwDeBT0Ar0nYocjEZZCe', true, true);

INSERT INTO mascotas (usuario_id, nombre, raza, edad_anos, edad_meses, peso_kg, tamano, activa) VALUES
(1, 'Max', 'Golden Retriever', 2, 24, 28.50, 'grande', true);

INSERT INTO dispensadores (usuario_id, nombre, codigo_unico, firmware_version, estado, bateria_percent, nivel_tolva_pct, ssid_wifi) VALUES
(1, 'PawFeeder Casa', 'PF-2024-001', 'v1.0.0', 'activo', 85, 65, 'MiWifi_Casa');

INSERT INTO horarios (usuario_id, mascota_id, dispensador_id, nombre, icono, hora, lunes, martes, miercoles, jueves, viernes, sabado, domingo, porcion_gramos) VALUES
(1, 1, 1, 'Desayuno', 'sun',  '07:30 AM', true, true, true, true, true, true, true, 140.0),
(1, 1, 1, 'Cena',     'moon', '06:00 PM', true, true, true, true, true, true, true, 140.0);
