using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Models;

namespace PetFeeder.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Una propiedad DbSet por cada tabla de la BD
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<OtpVerificacion> OtpVerificaciones { get; set; }
        public DbSet<Sesion> Sesiones { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Dispensador> Dispensadores { get; set; }
        public DbSet<Horario> Horarios { get; set; }
        public DbSet<Dispensacion> Dispensaciones { get; set; }
        public DbSet<TelemetriaDispensador> Telemetria { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }

        // Tablas nuevas del módulo de agua
        public DbSet<HorarioAgua> HorariosAgua { get; set; }
        public DbSet<DispensacionAgua> DispensacionesAgua { get; set; }
    }
}
