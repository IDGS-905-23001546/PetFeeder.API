using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Models;
using PetFeeder.API.Services;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComponentesController : ControllerBase
    {
        private readonly DualWriteService _dual;
        private AppDbContext _db => _dual.Db;
        public ComponentesController(DualWriteService dual) { _dual = dual; }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var lista = await _db.Componentes
                .OrderBy(c => c.Nombre)
                .ToListAsync();
            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Componente dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre del componente es obligatorio.");
            if (dto.Stock <= 0)
                return BadRequest("El stock inicial debe ser mayor a 0.");

            dto.Id = 0;
            dto.Nombre = dto.Nombre.Trim();
            dto.UnidadMedida = "pza";
            _db.Componentes.Add(dto);
            await _dual.SaveChangesAsync();
            return Ok(dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] Componente dto)
        {
            if (id != dto.Id)
                return BadRequest("El id no coincide.");
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre del componente es obligatorio.");
            if (dto.Stock < 0)
                return BadRequest("El stock no puede ser negativo.");

            var c = await _db.Componentes.FindAsync(id);
            if (c == null)
                return NotFound("El componente no existe.");

            c.Nombre = dto.Nombre.Trim();
            c.Stock = dto.Stock;
            c.UnidadMedida = "pza";
            await _dual.SaveChangesAsync();
            return Ok(c);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Borrar(int id)
        {
            var c = await _db.Componentes.FindAsync(id);
            if (c == null)
                return NotFound("El componente no existe.");

            bool enReceta = await _db.RecetasProducto.AnyAsync(r => r.ComponenteId == id);
            if (enReceta)
                return BadRequest("El componente esta asociado a una receta de produccion y no puede eliminarse.");

            _db.Componentes.Remove(c);
            await _dual.SaveChangesAsync();
            return Ok(new { mensaje = $"Componente '{c.Nombre}' eliminado correctamente." });
        }
    }
}
