using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.DTOs;
using PetFeeder.API.Models;
using PetFeeder.API.Services;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DispensadoresController : ControllerBase
    {
        private readonly DualWriteService _dual;
        private AppDbContext _db => _dual.Db;
        public DispensadoresController(DualWriteService dual) { _dual = dual; }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var lista = await _db.Dispensadores
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var d = await _db.Dispensadores.FindAsync(id);
            if (d == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Dispensador no encontrado." });
            return Ok(d);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Dispensador dto)
        {
            dto.Id = 0;
            dto.CreatedAt = DateTime.UtcNow;
            dto.UpdatedAt = DateTime.UtcNow;
            _db.Dispensadores.Add(dto);
            await _dual.SaveChangesAsync();
            return Ok(dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] Dispensador dto)
        {
            var d = await _db.Dispensadores.FindAsync(id);
            if (d == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Dispensador no encontrado." });

            d.UsuarioId = dto.UsuarioId;
            d.Nombre = dto.Nombre;
            d.CodigoUnico = dto.CodigoUnico;
            d.FirmwareVersion = dto.FirmwareVersion;
            d.Estado = dto.Estado;
            d.BateriaPercent = dto.BateriaPercent;
            d.NivelTolvaPct = dto.NivelTolvaPct;
            d.SsidWifi = dto.SsidWifi;
            d.Activo = dto.Activo;
            d.LastPingAt = dto.LastPingAt;
            d.UpdatedAt = DateTime.UtcNow;
            await _dual.SaveChangesAsync();
            return Ok(d);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Borrar(int id)
        {
            var d = await _db.Dispensadores.FindAsync(id);
            if (d == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Dispensador no encontrado." });

            _db.Dispensadores.Remove(d);
            await _dual.SaveChangesAsync();
            return Ok(new RespuestaDto { Exito = true, Mensaje = "Dispensador eliminado." });
        }
    }
}
