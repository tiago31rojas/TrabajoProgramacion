using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PruebaFina.Data;
using PruebaFina.Models;
using System.Threading.Tasks;
using System.Linq;

namespace PruebaFina.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BilleteraController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BilleteraController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("transferir")]
        public async Task<IActionResult> TransferirPesos([FromBody] TransferenciaPesosDto dto)
        {
            var usuario = await _context.UsuariosRegistrados.FindAsync(dto.UsuarioId);
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            var billetera = await _context.billeteraPesos
                .FirstOrDefaultAsync(b => b.UsuarioId == dto.UsuarioId);

            if (billetera == null)
            {
                billetera = new BilleteraPesos
                {
                    UsuarioId = dto.UsuarioId,
                    Importe = dto.Pesos,
                    Usuario = usuario
                };
                _context.billeteraPesos.Add(billetera);
            }
            else
            {
                billetera.Importe += dto.Pesos;
                _context.billeteraPesos.Update(billetera);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                UsuarioId = dto.UsuarioId,
                SaldoActual = billetera.Importe
            });
        }

        [HttpGet("saldos/{usuarioId}")]
        public async Task<IActionResult> GetSaldos(int usuarioId)
        {
            var billetera = await _context.billeteraPesos.FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);
            var saldoPesos = billetera?.Importe ?? 0;

            var saldosCripto = await _context.billeteraCripto
            .Where(bc => bc.UsuarioId == usuarioId)
            .Select(bc => new
            {
                 nombre = bc.CodigoCriptoId, 
                 cantidad = bc.CantidadCripto
            })
               .ToListAsync();

            return Ok(new {
                saldoPesos,
                saldosCripto
            });
        }
    }

    public class TransferenciaPesosDto
    {
        public int UsuarioId { get; set; }
        public decimal Pesos { get; set; }
    }


}