using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Models;

namespace PetFeeder.API.Services
{
    public static class DbInitializer
    {
        // Crea las tablas que faltan y agrega la columna "rol" sin borrar datos.
        // Es seguro ejecutarlo en cada arranque (todo es IF NOT EXISTS).
        public static void AsegurarEsquema(AppDbContext db)
        {
            db.Database.ExecuteSqlRaw(@"
                -- 1. Tablas de inventario / proveedores / opiniones (solo si no existen)
                IF OBJECT_ID('opiniones', 'U') IS NULL
                BEGIN
                    CREATE TABLE [opiniones] (
                        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [nombre_usuario] NVARCHAR(100) NOT NULL,
                        [detalles_mascota] NVARCHAR(100) NOT NULL,
                        [calificacion] INT NOT NULL,
                        [comentario] NVARCHAR(MAX) NOT NULL,
                        [fecha] NVARCHAR(20) NOT NULL
                    );
                END;

                IF OBJECT_ID('inventario_componentes', 'U') IS NULL
                BEGIN
                    CREATE TABLE [inventario_componentes] (
                        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [nombre] NVARCHAR(MAX) NOT NULL,
                        [stock] INT NOT NULL,
                        [unidad_medida] NVARCHAR(MAX) NOT NULL
                    );
                END;

                IF OBJECT_ID('inventario_productos', 'U') IS NULL
                BEGIN
                    CREATE TABLE [inventario_productos] (
                        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [nombre] NVARCHAR(MAX) NOT NULL,
                        [stock] INT NOT NULL,
                        [estado] NVARCHAR(MAX) NULL
                    );
                END;

                IF OBJECT_ID('recetas_producto', 'U') IS NULL
                BEGIN
                    CREATE TABLE [recetas_producto] (
                        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [producto_id] INT NOT NULL,
                        [componente_id] INT NOT NULL,
                        [cantidad_requerida] INT NOT NULL,
                        [dispensador] NVARCHAR(MAX) NULL
                    );
                END;

                IF OBJECT_ID('dispensadores_inventario', 'U') IS NULL
                BEGIN
                    CREATE TABLE [dispensadores_inventario] (
                        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [producto_id] INT NOT NULL,
                        [codigo_unico] NVARCHAR(MAX) NOT NULL,
                        [estado] NVARCHAR(MAX) NOT NULL,
                        [creado_en] DATETIME2 NOT NULL
                    );
                END;

                IF OBJECT_ID('proveedores', 'U') IS NULL
                BEGIN
                    CREATE TABLE [proveedores] (
                        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [nombre] NVARCHAR(MAX) NOT NULL,
                        [contacto] NVARCHAR(MAX) NULL,
                        [telefono] NVARCHAR(MAX) NULL,
                        [correo] NVARCHAR(MAX) NULL,
                        [direccion] NVARCHAR(MAX) NULL,
                        [activo] BIT NOT NULL,
                        [creado_en] DATETIME2 NOT NULL
                    );
                END;

                -- 2. Columna 'rol' en usuarios (solo si falta)
                IF COL_LENGTH('usuarios', 'rol') IS NULL
                BEGIN
                    ALTER TABLE [usuarios] ADD [rol] NVARCHAR(50) NOT NULL DEFAULT 'cliente';
                END;
            ");
        }

        // Crea el usuario administrador inicial (si no existe)
        public static void SembrarAdmin(AppDbContext db, string hashAdmin)
        {
            bool existe = db.Usuarios.Any(u => u.Email == "admin@pawfeeder.com");
            if (!existe)
            {
                db.Usuarios.Add(new Usuario
                {
                    Nombre = "Administrador",
                    Email = "admin@pawfeeder.com",
                    PasswordHash = hashAdmin,
                    Verificado = true,
                    Rol = "admin",
                    Activo = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                db.SaveChanges();
            }
        }
    }
}
