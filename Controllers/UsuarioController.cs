using PruebaFina.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using PruebaFina.Data;
using trabajoFinal.Models;

namespace PruebaFina.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistroController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RegistroController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto user)
        {
            var usuario = await _context.UsuariosRegistrados
                .FirstOrDefaultAsync(u => u.Gmail == user.Gmail && u.Contrasenia == user.Contrasenia);

            if (usuario == null)
                return Unauthorized("Usuario no encontrado chaval ");

            return Ok(new { Id = usuario.Id });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto user)
        {
    

            var existe = await _context.UsuariosRegistrados
                .AnyAsync(u => u.Gmail == user.Gmail);

            if (existe)
                return BadRequest("El usuario ya existe");

            var nuevoUsuario = new Usuario
            {
                Nombre = user.Nombre,
                Gmail = user.Gmail,
                Contrasenia = user.Contrasenia
            };

            try
            {
                _context.UsuariosRegistrados.Add(nuevoUsuario);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }

            return Ok(new
            {
                Gmail = nuevoUsuario.Gmail,
                Nombre = nuevoUsuario.Nombre,
            });
        }
    }
}