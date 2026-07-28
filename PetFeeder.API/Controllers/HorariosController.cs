using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.DTOs;
using PetFeeder.API.Models;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HorariosController : ControllerBase
    {
        private readonly AppDbContext _db;
        public HorariosController(AppDbContext db) { _db = db; }

        // GET /api/horarios/usuario/5
        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> PorUsuario(int usuarioId)
        {
            var lista = await _db.Horarios
                .Where(h => h.UsuarioId == usuarioId)
                .OrderBy(h => h.Hora)
                .ToListAsync();
            return Ok(lista);
        }

        // POST /api/horarios
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Horario dto)
        {
            dto.Id = 0;
            dto.CreatedAt = DateTime.UtcNow;
            dto.UpdatedAt = DateTime.UtcNow;
            _db.Horarios.Add(dto);
            await _db.SaveChangesAsync();
            return Ok(dto);
        }

        // PUT /api/horarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] Horario dto)
        {
            var h = await _db.Horarios.FindAsync(id);
            if (h == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Horario no encontrado." });

            h.Nombre = dto.Nombre;
            h.Icono = dto.Icono;
            h.Hora = dto.Hora;
            h.Lunes = dto.Lunes; h.Martes = dto.Martes; h.Miercoles = dto.Miercoles;
            h.Jueves = dto.Jueves; h.Viernes = dto.Viernes; h.Sabado = dto.Sabado; h.Domingo = dto.Domingo;
            h.PorcionGramos = dto.PorcionGramos;
            h.Activo = dto.Activo;
            h.MascotaId = dto.MascotaId;
            h.DispensadorId = dto.DispensadorId;
            h.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(h);
        }

        // PUT /api/horarios/5/activo/true  -> encender/apagar
        [HttpPut("{id}/activo/{valor:bool}")]
        public async Task<IActionResult> CambiarActivo(int id, bool valor)
        {
            var h = await _db.Horarios.FindAsync(id);
            if (h == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Horario no encontrado." });
            h.Activo = valor;
            h.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(h);
        }

        // DELETE /api/horarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Borrar(int id)
        {
            var h = await _db.Horarios.FindAsync(id);
            if (h == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Horario no encontrado." });
            _db.Horarios.Remove(h);
            await _db.SaveChangesAsync();
            return Ok(new RespuestaDto { Exito = true, Mensaje = "Horario eliminado." });
        }
    }
}
