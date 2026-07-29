using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Models;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DispensacionesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private const decimal CAPACIDAD_KG = 5.0m;
        private const decimal ALERTA_KG = 1.0m;

        public DispensacionesController(AppDbContext db) { _db = db; }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> PorUsuario(int usuarioId)
        {
            var lista = await _db.Dispensaciones
                .Where(d => d.UsuarioId == usuarioId)
                .OrderByDescending(d => d.FechaHora)
                .Take(100)
                .ToListAsync();
            return Ok(lista);
        }

        [HttpGet("usuario/{usuarioId}/hoy")]
        public async Task<IActionResult> DeHoy(int usuarioId)
        {
            var hoy = DateTime.UtcNow.Date;
            var lista = await _db.Dispensaciones
                .Where(d => d.UsuarioId == usuarioId && d.FechaHora >= hoy)
                .OrderByDescending(d => d.FechaHora)
                .ToListAsync();
            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Dispensacion dto)
        {
            dto.Id = 0;
            if (dto.FechaHora == default) dto.FechaHora = DateTime.UtcNow;
            dto.CreatedAt = DateTime.UtcNow;
            _db.Dispensaciones.Add(dto);
            await _db.SaveChangesAsync();

            // Calcular restante y notificar si es necesario
            await VerificarTolva(dto.UsuarioId, dto.DispensadorId);

            return Ok(dto);
        }

        [HttpGet("tolva/{usuarioId}")]
        public async Task<IActionResult> EstadoTolva(int usuarioId)
        {
            var totalGramos = await _db.Dispensaciones
                .Where(d => d.UsuarioId == usuarioId && d.Estado == "ejecutada")
                .SumAsync(d => d.PorcionGramos);

            var usadoKg = totalGramos / 1000m;
            var restanteKg = Math.Max(0, CAPACIDAD_KG - usadoKg);
            var porcentaje = (int)((restanteKg / CAPACIDAD_KG) * 100);

            return Ok(new
            {
                capacidadKg = CAPACIDAD_KG,
                usadoKg,
                restanteKg,
                porcentaje,
                necesitaRelleno = restanteKg <= ALERTA_KG
            });
        }

        [HttpPost("rellenar/{usuarioId}")]
        public async Task<IActionResult> RellenarTolva(int usuarioId)
        {
            var notifPendientes = await _db.Notificaciones
                .Where(n => n.UsuarioId == usuarioId && n.Tipo == "tolva_baja")
                .ToListAsync();
            _db.Notificaciones.RemoveRange(notifPendientes);
            await _db.SaveChangesAsync();
            return Ok(new { exito = true, mensaje = "Tolva reiniciada a 5kg." });
        }

        private async Task VerificarTolva(int usuarioId, int? dispensadorId)
        {
            var totalGramos = await _db.Dispensaciones
                .Where(d => d.UsuarioId == usuarioId && d.Estado == "ejecutada")
                .SumAsync(d => d.PorcionGramos);

            var usadoKg = totalGramos / 1000m;
            var restanteKg = CAPACIDAD_KG - usadoKg;

            if (restanteKg <= ALERTA_KG)
            {
                var yaNotificado = await _db.Notificaciones
                    .AnyAsync(n => n.UsuarioId == usuarioId && n.Tipo == "tolva_baja");

                if (!yaNotificado)
                {
                    _db.Notificaciones.Add(new Notificacion
                    {
                        UsuarioId = usuarioId,
                        DispensadorId = dispensadorId,
                        Tipo = "tolva_baja",
                        Titulo = "¡Rellena el dispensador!",
                        Mensaje = $"Queda aproximadamente {restanteKg:F1} kg de alimento. Es hora de rellenar.",
                        Leida = false,
                        CreatedAt = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }
            }
        }
    }
}
