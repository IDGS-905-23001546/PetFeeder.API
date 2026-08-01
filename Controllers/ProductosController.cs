using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;
using PetFeeder.API.Models;
using PetFeeder.API.Services;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly DualWriteService _dual;
        private AppDbContext _db => _dual.Db;
        public ProductosController(DualWriteService dual) { _dual = dual; }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var lista = await _db.InventarioProductos
                .OrderBy(p => p.Nombre)
                .ToListAsync();
            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ProductoTerminado dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre del producto es obligatorio.");

            bool duplicado = await _db.InventarioProductos
                .AnyAsync(p => p.Nombre.ToLower() == dto.Nombre.ToLower());
            if (duplicado)
                return BadRequest($"Ya existe un producto con el nombre '{dto.Nombre}'.");

            dto.Id = 0;
            dto.Stock = 0;
            dto.Estado = "En proceso";
            _db.InventarioProductos.Add(dto);
            await _dual.SaveChangesAsync();
            return Ok(dto);
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoProductoRequest request)
        {
            var p = await _db.InventarioProductos.FindAsync(id);
            if (p == null)
                return NotFound("Producto no encontrado.");

            p.Estado = request.Estado;
            await _dual.SaveChangesAsync();
            return Ok(new { mensaje = $"Estado actualizado a '{request.Estado}'." });
        }
    }

    public class EstadoProductoRequest
    {
        public string Estado { get; set; } = string.Empty;
    }
}
