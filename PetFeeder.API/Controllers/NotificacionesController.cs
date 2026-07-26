using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Models;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificacionesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public NotificacionesController(AppDbContext db) { _db = db; }

        // GET /api/notificaciones/usuario/5
        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> PorUsuario(int usuarioId)
        {
            var lista = await _db.Notificaciones
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();
            return Ok(lista);
        }

        // PUT /api/notificaciones/3/leida
        [HttpPut("{id}/leida")]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            var notif = await _db.Notificaciones.FindAsync(id);
            if (notif == null) return NotFound();
            notif.Leida = true;
            await _db.SaveChangesAsync();
            return Ok(notif);
        }

        // PUT /api/notificaciones/usuario/5/marcar-todas
        [HttpPut("usuario/{usuarioId}/marcar-todas")]
        public async Task<IActionResult> MarcarTodasLeidas(int usuarioId)
        {
            var lista = await _db.Notificaciones
                .Where(n => n.UsuarioId == usuarioId && !n.Leida)
                .ToListAsync();
            lista.ForEach(n => n.Leida = true);
            await _db.SaveChangesAsync();
            return Ok(new { exito = true, mensaje = "Todas marcadas como leídas" });
        }

        // DELETE /api/notificaciones/3
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var notif = await _db.Notificaciones.FindAsync(id);
            if (notif == null) return NotFound();
            _db.Notificaciones.Remove(notif);
            await _db.SaveChangesAsync();
            return Ok(new { exito = true, mensaje = "Eliminada" });
        }
    }
}
