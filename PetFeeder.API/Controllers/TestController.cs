using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetFeeder.API.Data;

namespace PetFeeder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Aqui .NET nos "inyecta" el AppDbContext que registramos en Program.cs
        public TestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("conexion")]
        public async Task<IActionResult> ProbarConexion()
        {
            // 1. ¿Puede EF Core abrir la conexion a SQL Server?
            bool conectado = await _context.Database.CanConnectAsync();

            // 2. Contar y leer los usuarios reales de la BD
            int totalUsuarios = await _context.Usuarios.CountAsync();

            var usuarios = await _context.Usuarios
                .Select(u => new { u.Id, u.Nombre, u.Email, u.Verificado })
                .ToListAsync();

            return Ok(new { conectado, totalUsuarios, usuarios });
        }
    }
}