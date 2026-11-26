-- Migración: pedidos a proveedor para inventario
-- Idempotente. Ejecutar en la BD de PetFeeder (SQL Server/SOMEE).

IF OBJECT_ID('dbo.pedidos_proveedor', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.pedidos_proveedor (
        id                INT            IDENTITY(1,1) NOT NULL,
        folio             NVARCHAR(30)   NOT NULL,
        proveedor_id      INT            NOT NULL,
        proveedor_nombre  NVARCHAR(150)  NOT NULL,
        componente_id     INT            NOT NULL,
        componente_nombre NVARCHAR(150)  NOT NULL,
        cantidad          INT            NOT NULL,
        unidad_medida     NVARCHAR(20)   NOT NULL DEFAULT N'pza',
        fecha             DATETIME2      NOT NULL DEFAULT GETDATE(),
        estado            NVARCHAR(20)   NOT NULL DEFAULT N'pendiente',
        fecha_recibido    DATETIME2      NULL,
        CONSTRAINT pk_pedidos_proveedor PRIMARY KEY (id)
    );

    CREATE INDEX idx_pedidos_proveedor_estado ON dbo.pedidos_proveedor (estado);
    CREATE INDEX idx_pedidos_proveedor_fecha  ON dbo.pedidos_proveedor (fecha DESC);
    PRINT 'Tabla pedidos_proveedor creada.';
END
ELSE
    PRINT 'Tabla pedidos_proveedor ya existía (omitida).';
GO
