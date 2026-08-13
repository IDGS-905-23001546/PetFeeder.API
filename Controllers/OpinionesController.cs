using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Models;
using PetFeeder.API.Services;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpinionesController : ControllerBase
    {
        private readonly DualWriteService _dual;
        private AppDbContext _db => _dual.Db;
        public OpinionesController(DualWriteService dual) { _dual = dual; }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var lista = await _db.Opiniones
                .OrderByDescending(o => o.Id)
                .ToListAsync();
            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Opinion dto)
        {
            dto.Id = 0;
            _db.Opiniones.Add(dto);
            await _dual.SaveChangesAsync();
            return Ok(dto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Opinion dto)
        {
            var opinion = await _db.Opiniones.FirstOrDefaultAsync(o => o.Id == id);
            if (opinion == null)
                return NotFound(new { mensaje = "La opinión no existe." });

            if (!string.IsNullOrWhiteSpace(dto.Estado))
                opinion.Estado = dto.Estado;

            if (dto.RespuestaAdmin != null)
            {
                opinion.RespuestaAdmin = dto.RespuestaAdmin.Trim();
                opinion.FechaRespuesta = string.IsNullOrWhiteSpace(opinion.RespuestaAdmin)
                    ? null
                    : DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            }

            await _dual.SaveChangesAsync();
            return Ok(opinion);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var opinion = await _db.Opiniones.FirstOrDefaultAsync(o => o.Id == id);
            if (opinion == null)
                return NotFound(new { mensaje = "La opinión no existe." });

            _db.Opiniones.Remove(opinion);
            await _dual.SaveChangesAsync();
            return Ok(new { mensaje = "Opinión eliminada." });
        }
    }
}
