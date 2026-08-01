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
    public class ProveedoresController : ControllerBase
    {
        private readonly DualWriteService _dual;
        private AppDbContext _db => _dual.Db;
        public ProveedoresController(DualWriteService dual) { _dual = dual; }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var lista = await _db.Proveedores
                .OrderByDescending(p => p.CreadoEn)
                .ToListAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var p = await _db.Proveedores.FindAsync(id);
            if (p == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Proveedor no encontrado." });
            return Ok(p);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Proveedor dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre del proveedor es obligatorio.");

            dto.Id = 0;
            dto.CreadoEn = DateTime.Now;
            _db.Proveedores.Add(dto);
            await _dual.SaveChangesAsync();
            return Ok(dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] Proveedor dto)
        {
            var p = await _db.Proveedores.FindAsync(id);
            if (p == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Proveedor no encontrado." });

            p.Nombre = dto.Nombre;
            p.Contacto = dto.Contacto;
            p.Telefono = dto.Telefono;
            p.Correo = dto.Correo;
            p.Direccion = dto.Direccion;
            p.Activo = dto.Activo;
            await _dual.SaveChangesAsync();
            return Ok(p);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Borrar(int id)
        {
            var p = await _db.Proveedores.FindAsync(id);
            if (p == null)
                return NotFound(new RespuestaDto { Exito = false, Mensaje = "Proveedor no encontrado." });

            _db.Proveedores.Remove(p);
            await _dual.SaveChangesAsync();
            return NoContent();
        }
    }
}
