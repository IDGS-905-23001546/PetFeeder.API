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
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly PasswordService _passwordService;
        private readonly EmailService _emailService;

        // El framework nos inyecta la BD, el servicio de contraseñas y el de correo
        public AuthController(
            AppDbContext db,
            PasswordService passwordService,
            EmailService emailService)
        {
            _db = db;
            _passwordService = passwordService;
            _emailService = emailService;
        }

        // POST /api/auth/registro
        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroDto dto)
        {
            // 1. Validar que el correo no esté ya registrado
            bool existe = await _db.Usuarios.AnyAsync(u => u.Email == dto.Email);
            if (existe)
            {
                return BadRequest(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Ya existe una cuenta con ese correo."
                });
            }

            // 2. Crear el usuario con la contraseña ENCRIPTADA, sin verificar aún
            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Telefono = dto.Telefono,
                PasswordHash = _passwordService.Encriptar(dto.Password),
                Verificado = false,
                Activo = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync(); // aquí se le asigna el Id

            // 3. Generar un código OTP de 6 dígitos
            var codigo = Random.Shared.Next(100000, 1000000).ToString();

            // 4. Guardar el OTP en la tabla otp_verificacion (expira en 5 min)
            var otp = new OtpVerificacion
            {
                UsuarioId = usuario.Id,
                Codigo = codigo,
                Intentos = 0,
                MaxIntentos = 3,
                ExpiraEn = DateTime.Now.AddMinutes(10),
                Usado = false,
                CreatedAt = DateTime.Now
            };
            _db.OtpVerificaciones.Add(otp);
            await _db.SaveChangesAsync();

            // 5. Intentar enviar el código por correo (si falla, no bloquea el registro)
            try
            {
                await _emailService.EnviarOtpAsync(usuario.Email, usuario.Nombre, codigo);
            }
            catch (Exception ex)
            {
                // Log del error pero no retornar error al usuario
                Console.WriteLine($"[WARNING] No se pudo enviar el correo OTP: {ex.Message}");
            }

            return Ok(new RespuestaDto
            {
                Exito = true,
                Mensaje = "Cuenta creada. Te enviamos un código de verificación a tu correo."
            });
        }

        // POST /api/auth/reenviar
        [HttpPost("reenviar")]
        public async Task<IActionResult> Reenviar([FromBody] ReenviarOtpDto dto)
        {
            // 1. Buscar el usuario por email
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null)
            {
                return BadRequest(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "No existe una cuenta con ese correo."
                });
            }

            // 2. Si ya está verificada, no hay nada que reenviar
            if (usuario.Verificado)
            {
                return Ok(new RespuestaDto
                {
                    Exito = true,
                    Mensaje = "Tu cuenta ya está verificada. Inicia sesión."
                });
            }

            // 3. Invalidar los códigos anteriores no usados
            var previos = await _db.OtpVerificaciones
                .Where(o => o.UsuarioId == usuario.Id && !o.Usado)
                .ToListAsync();
            foreach (var p in previos) p.Usado = true;

            // 4. Generar y guardar un código nuevo (expira en 10 min)
            var codigo = Random.Shared.Next(100000, 1000000).ToString();
            var otp = new OtpVerificacion
            {
                UsuarioId = usuario.Id,
                Codigo = codigo,
                Intentos = 0,
                MaxIntentos = 3,
                ExpiraEn = DateTime.Now.AddMinutes(10),
                Usado = false,
                CreatedAt = DateTime.Now
            };
            _db.OtpVerificaciones.Add(otp);
            await _db.SaveChangesAsync();

            // 5. Intentar enviar el nuevo código por correo (si falla, no bloquea)
            try
            {
                await _emailService.EnviarOtpAsync(usuario.Email, usuario.Nombre, codigo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] No se pudo enviar el correo OTP: {ex.Message}");
            }

            return Ok(new RespuestaDto
            {
                Exito = true,
                Mensaje = "Te enviamos un nuevo código a tu correo."
            });
        }

        // POST /api/auth/verificar
        [HttpPost("verificar")]
        public async Task<IActionResult> Verificar([FromBody] VerificarOtpDto dto)
        {
            // 1. Buscar el usuario por email
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null)
            {
                return BadRequest(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "No existe una cuenta con ese correo."
                });
            }

            // 2. Si ya está verificada, no hay nada que hacer
            if (usuario.Verificado)
            {
                return Ok(new RespuestaDto
                {
                    Exito = true,
                    Mensaje = "Tu cuenta ya estaba verificada."
                });
            }

            // 3. Buscar el OTP más reciente de ese usuario que no se haya usado
            var otp = await _db.OtpVerificaciones
                .Where(o => o.UsuarioId == usuario.Id && !o.Usado)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null)
            {
                return BadRequest(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "No hay un código activo. Solicita uno nuevo."
                });
            }

            // 4. ¿Expiró?
            if (otp.ExpiraEn < DateTime.Now)
            {
                return BadRequest(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "El código expiró. Solicita uno nuevo."
                });
            }

            // 5. ¿Se agotaron los intentos?
            if (otp.Intentos >= otp.MaxIntentos)
            {
                return BadRequest(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Demasiados intentos fallidos. Solicita un código nuevo."
                });
            }

            // 6. ¿El código coincide?
            if (otp.Codigo != dto.Codigo)
            {
                otp.Intentos++;              // sumar un intento fallido
                await _db.SaveChangesAsync();
                return BadRequest(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Código incorrecto."
                });
            }

            // 7. ¡Correcto! Marcar OTP como usado y verificar la cuenta
            otp.Usado = true;
            usuario.Verificado = true;
            usuario.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            return Ok(new RespuestaDto
            {
                Exito = true,
                Mensaje = "¡Cuenta verificada! Ya puedes iniciar sesión."
            });
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // 1. Buscar el usuario por email
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            // 2. Si no existe, o la contraseña no coincide -> error genérico
            if (usuario == null ||
                !_passwordService.Verificar(dto.Password, usuario.PasswordHash))
            {
                return Unauthorized(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Correo o contraseña incorrectos."
                });
            }

            // 3. Si la cuenta no está verificada -> avisar
            if (!usuario.Verificado)
            {
                return BadRequest(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Tu cuenta aún no está verificada."
                });
            }

            // 4. Todo bien -> devolver datos del usuario (SIN el password)
            return Ok(new UsuarioRespuestaDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Verificado = usuario.Verificado
            });
        }

    // CAMBIAR CONTRASENA --------

    [HttpPut("cambiar-password")]
        public async Task<ActionResult<RespuestaDto>> CambiarPassword([FromBody] CambiarPasswordDto dto)
        {
            var usuario = await _db.Usuarios.FindAsync(dto.UsuarioId);
            if (usuario == null)
                return BadRequest(new RespuestaDto { Exito = false, Mensaje = "Usuario no encontrado." });

            if (!_passwordService.Verificar(dto.PasswordActual, usuario.PasswordHash))
                return BadRequest(new RespuestaDto { Exito = false, Mensaje = "Contraseña o Usuario incorrectos." });

            if (string.IsNullOrWhiteSpace(dto.PasswordNueva) || dto.PasswordNueva.Length < 6)
                return BadRequest(new RespuestaDto { Exito = false, Mensaje = "La nueva contraseña debe de tener minimo 6 caracteres." });

            usuario.PasswordHash = _passwordService.Encriptar(dto.PasswordNueva);
            usuario.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new RespuestaDto { Exito = true, Mensaje = "Contraseña actualizada correctamente." });
        }
    }
}