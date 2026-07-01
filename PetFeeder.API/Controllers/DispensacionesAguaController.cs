using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Models;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DispensacionesAguaController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DispensacionesAguaController(AppDbContext db) { _db = db; }

        // GET /api/dispensacionesagua/usuario/5  -> historial de agua (últimas 100)
        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> PorUsuario(int usuarioId)
        {
            var lista = await _db.DispensacionesAgua
                .Where(d => d.UsuarioId == usuarioId)
                .OrderByDescending(d => d.FechaHora)
                .Take(100)
                .ToListAsync();
            return Ok(lista);
        }

        // POST /api/dispensacionesagua  -> registra un dispensado de agua
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] DispensacionAgua dto)
        {
            dto.Id = 0;
            if (dto.FechaHora == default) dto.FechaHora = DateTime.Now;
            dto.CreatedAt = DateTime.Now;
            _db.DispensacionesAgua.Add(dto);
            await _db.SaveChangesAsync();
            return Ok(dto);
        }
    }
}
