using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.DTOs;
using PetFeeder.API.Services;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DualWriteService _dual;
        private AppDbContext _db => _dual.Db;

        public DashboardController(DualWriteService dual)
        {
            _dual = dual;
        }

        // GET /api/Dashboard/cliente/{usuarioId}
        [HttpGet("cliente/{usuarioId}")]
        public async Task<IActionResult> Cliente(int usuarioId)
        {
            var usuario = await _db.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Usuario no encontrado." });

            var hace7Dias = DateTime.UtcNow.AddDays(-7);
            var inicioHoy = DateTime.UtcNow.Date;

            var mascotas = await _db.Mascotas
                .Where(m => m.UsuarioId == usuarioId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var horarios = await _db.Horarios
                .Where(h => h.UsuarioId == usuarioId)
                .OrderBy(h => h.Hora)
                .ToListAsync();

            var dispensaciones = await _db.Dispensaciones
                .Where(d => d.UsuarioId == usuarioId)
                .OrderByDescending(d => d.FechaHora)
                .Take(100)
                .ToListAsync();

            var dispensadores = await _db.Dispensadores
                .Where(d => d.UsuarioId == usuarioId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            var sesiones = await _db.Sesiones
                .Where(s => s.UsuarioId == usuarioId)
                .ToListAsync();

            var notificaciones = await _db.Notificaciones
                .Where(n => n.UsuarioId == usuarioId)
                .ToListAsync();

            var semana = dispensaciones.Where(d => d.FechaHora >= hace7Dias && d.Estado == "ejecutada");
            var hoy = dispensaciones.Where(d => d.FechaHora >= inicioHoy && d.Estado == "ejecutada");

            return Ok(new
            {
                mascotas = new
                {
                    total = mascotas.Count,
                    activas = mascotas.Count(m => m.Activa),
                    lista = mascotas.Select(m => new { m.Id, m.Nombre, m.Raza, m.Tamano, m.Activa })
                },
                horarios = new
                {
                    total = horarios.Count,
                    activos = horarios.Count(h => h.Activo),
                    lista = horarios.Select(h => new { h.Id, h.Nombre, h.Hora, h.PorcionGramos, h.Activo })
                },
                comida = new
                {
                    semanaGramos = semana.Sum(d => d.PorcionGramos),
                    hoyGramos = hoy.Sum(d => d.PorcionGramos),
                    totalDispensaciones = dispensaciones.Count,
                    dispensacionesHoy = hoy.Count()
                },
                dispensadores = dispensadores.Select(d => new
                {
                    d.Id,
                    d.Nombre,
                    d.CodigoUnico,
                    d.Estado,
                    d.BateriaPercent,
                    d.NivelTolvaPct,
                    d.Activo
                }),
                sesiones = new
                {
                    total = sesiones.Count,
                    web = sesiones.Count(s => s.Dispositivo == "web"),
                    app = sesiones.Count(s => s.Dispositivo == "app")
                },
                notificaciones = new
                {
                    noLeidas = notificaciones.Count(n => !n.Leida)
                }
            });
        }

        // GET /api/Dashboard/admin
        [HttpGet("admin")]
        public async Task<IActionResult> Admin()
        {
            var usuarios = await _db.Usuarios.OrderBy(u => u.CreatedAt).ToListAsync();
            var sesiones = await _db.Sesiones.ToListAsync();
            var productos = await _db.InventarioProductos.OrderBy(p => p.Nombre).ToListAsync();
            var componentes = await _db.Componentes.OrderBy(c => c.Nombre).ToListAsync();
            var dispensadoresInv = await _db.DispensadoresInventario.ToListAsync();
            var dispensadores = await _db.Dispensadores.ToListAsync();
            var dispensaciones = await _db.Dispensaciones.ToListAsync();
            var mascotas = await _db.Mascotas.ToListAsync();
            var horarios = await _db.Horarios.ToListAsync();
            var opiniones = await _db.Opiniones.ToListAsync();

            var loginsPorUsuario = sesiones
                .GroupBy(s => s.UsuarioId)
                .Select(g => new
                {
                    usuarioId = g.Key,
                    nombre = usuarios.FirstOrDefault(u => u.Id == g.Key)?.Nombre ?? "Desconocido",
                    email = usuarios.FirstOrDefault(u => u.Id == g.Key)?.Email ?? "",
                    total = g.Count(),
                    web = g.Count(s => s.Dispositivo == "web"),
                    app = g.Count(s => s.Dispositivo == "app")
                })
                .OrderByDescending(x => x.total)
                .ToList();

            return Ok(new
            {
                usuarios = new
                {
                    total = usuarios.Count,
                    admins = usuarios.Count(u => u.Rol == "admin"),
                    clientes = usuarios.Count(u => u.Rol == "cliente"),
                    verificados = usuarios.Count(u => u.Verificado),
                    activos = usuarios.Count(u => u.Activo)
                },
                sesiones = new
                {
                    total = sesiones.Count,
                    porUsuario = loginsPorUsuario
                },
                inventario = new
                {
                    productos = new
                    {
                        total = productos.Count,
                        stockTotal = productos.Sum(p => p.Stock),
                        enProceso = productos.Count(p => p.Estado == "En proceso"),
                        terminados = productos.Count(p => p.Estado == "Terminado")
                    },
                    componentes = new
                    {
                        total = componentes.Count,
                        stockTotal = componentes.Sum(c => c.Stock)
                    },
                    unidadesFabricadas = dispensadoresInv.Count,
                    unidadesTerminadas = dispensadoresInv.Count(d => d.Estado == "Terminado"),
                    unidadesPendientes = dispensadoresInv.Count(d => d.Estado == "Pendiente")
                },
                ventas = new
                {
                    dispositivosRegistrados = dispensadores.Count,
                    dispositivosActivos = dispensadores.Count(d => d.Activo),
                    dispensacionesEjecutadas = dispensaciones.Count(d => d.Estado == "ejecutada"),
                    comidaTotalGramos = dispensaciones.Where(d => d.Estado == "ejecutada").Sum(d => d.PorcionGramos)
                },
                general = new
                {
                    mascotas = mascotas.Count,
                    horarios = horarios.Count,
                    opiniones = opiniones.Count,
                    calificacionPromedio = opiniones.Count > 0 ? opiniones.Average(o => o.Calificacion) : 0
                }
            });
        }
    }
}
