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
    public class MascotasController : ControllerBase
    {
        private readonly DualWriteService _dual;
        private AppDbContext _db => _dual.Db;
        public MascotasController(DualWriteService dual) { _dual = dual; }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> PorUsuario(int usuarioId)
        {
            var lista = await _db.Mascotas
                .Where(m => m.UsuarioId == usuarioId)
                .OrderByDescending(m => m.Activa)
                .ThenByDescending(m => m.CreatedAt)
                .ToListAsync();
            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Mascota dto)
        {
            if (dto.Activa) await DesactivarOtras(dto.UsuarioId, 0);

            dto.Id = 0;
            dto.CreatedAt = DateTime.UtcNow;
            dto.UpdatedAt = DateTime.UtcNow;
            _db.Mascotas.Add(dto);
            await _dual.SaveChangesAsync();
            return Ok(dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] Mascota dto)
        {
            var m = await _db.Mascotas.FindAsync(id);
            if (m == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Mascota no encontrada." });

            if (dto.Activa) await DesactivarOtras(m.UsuarioId, id);

            m.Nombre = dto.Nombre;
            m.Raza = dto.Raza;
            m.EdadAnos = dto.EdadAnos;
            m.EdadMeses = dto.EdadMeses;
            m.PesoKg = dto.PesoKg;
            m.Tamano = dto.Tamano;
            m.Activa = dto.Activa;
            m.FotoUri = dto.FotoUri;
            m.UpdatedAt = DateTime.UtcNow;
            await _dual.SaveChangesAsync();
            return Ok(m);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Borrar(int id)
        {
            var m = await _db.Mascotas.FindAsync(id);
            if (m == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Mascota no encontrada." });

            _db.Mascotas.Remove(m);
            await _dual.SaveChangesAsync();
            return Ok(new RespuestaDto { Exito = true, Mensaje = "Mascota eliminada." });
        }

        private async Task DesactivarOtras(int usuarioId, int exceptoId)
        {
            var activas = await _db.Mascotas
                .Where(m => m.UsuarioId == usuarioId && m.Activa && m.Id != exceptoId)
                .ToListAsync();
            foreach (var a in activas) a.Activa = false;
        }
    }
}
