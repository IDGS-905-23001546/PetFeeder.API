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
    }
}
