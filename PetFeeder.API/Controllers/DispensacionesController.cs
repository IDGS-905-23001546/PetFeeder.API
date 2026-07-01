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
        public DispensacionesController(AppDbContext db) { _db = db; }

        // GET /api/dispensaciones/usuario/5  -> historial (últimas 100)
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

        // GET /api/dispensaciones/usuario/5/hoy  -> las de hoy
        [HttpGet("usuario/{usuarioId}/hoy")]
        public async Task<IActionResult> DeHoy(int usuarioId)
        {
            var hoy = DateTime.Today;
            var lista = await _db.Dispensaciones
                .Where(d => d.UsuarioId == usuarioId && d.FechaHora >= hoy)
                .OrderByDescending(d => d.FechaHora)
                .ToListAsync();
            return Ok(lista);
        }

        // POST /api/dispensaciones  -> registra un evento de dispensado
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Dispensacion dto)
        {
            dto.Id = 0;
            if (dto.FechaHora == default) dto.FechaHora = DateTime.Now;
            dto.CreatedAt = DateTime.Now;
            _db.Dispensaciones.Add(dto);
            await _db.SaveChangesAsync();
            return Ok(dto);
        }
    }
}
