using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.DTOs;
using PetFeeder.API.Models;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HorariosAguaController : ControllerBase
    {
        private readonly AppDbContext _db;
        public HorariosAguaController(AppDbContext db) { _db = db; }

        // GET /api/horariosagua/usuario/5
        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> PorUsuario(int usuarioId)
        {
            var lista = await _db.HorariosAgua
                .Where(h => h.UsuarioId == usuarioId)
                .OrderBy(h => h.Hora)
                .ToListAsync();
            return Ok(lista);
        }

        // POST /api/horariosagua
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] HorarioAgua dto)
        {
            dto.Id = 0;
            dto.CreatedAt = DateTime.Now;
            dto.UpdatedAt = DateTime.Now;
            _db.HorariosAgua.Add(dto);
            await _db.SaveChangesAsync();
            return Ok(dto);
        }

        // PUT /api/horariosagua/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] HorarioAgua dto)
        {
            var h = await _db.HorariosAgua.FindAsync(id);
            if (h == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Horario de agua no encontrado." });

            h.Nombre = dto.Nombre;
            h.Icono = dto.Icono;
            h.Hora = dto.Hora;
            h.Lunes = dto.Lunes; h.Martes = dto.Martes; h.Miercoles = dto.Miercoles;
            h.Jueves = dto.Jueves; h.Viernes = dto.Viernes; h.Sabado = dto.Sabado; h.Domingo = dto.Domingo;
            h.CantidadMl = dto.CantidadMl;
            h.Activo = dto.Activo;
            h.MascotaId = dto.MascotaId;
            h.DispensadorId = dto.DispensadorId;
            h.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(h);
        }

        // PUT /api/horariosagua/5/activo/true
        [HttpPut("{id}/activo/{valor:bool}")]
        public async Task<IActionResult> CambiarActivo(int id, bool valor)
        {
            var h = await _db.HorariosAgua.FindAsync(id);
            if (h == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Horario de agua no encontrado." });
            h.Activo = valor;
            h.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(h);
        }

        // DELETE /api/horariosagua/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Borrar(int id)
        {
            var h = await _db.HorariosAgua.FindAsync(id);
            if (h == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Horario de agua no encontrado." });
            _db.HorariosAgua.Remove(h);
            await _db.SaveChangesAsync();
            return Ok(new RespuestaDto { Exito = true, Mensaje = "Horario de agua eliminado." });
        }
    }
}
