using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PruebaFina.Data;
using PruebaFina.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PruebaFina.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransaccionController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TransaccionController> _logger;
        private readonly AppDbContext _context;
        

        public TransaccionController(IHttpClientFactory httpClientFactory, AppDbContext appContext, ILogger<TransaccionController> logger )
        {
            _httpClient = httpClientFactory.CreateClient();
           _context = appContext;
            _logger = logger;
           _httpClient.BaseAddress = new Uri("https://criptoya.com/api/");
        }

        [HttpGet("Historial")]
        public async Task<IActionResult> GetHistorial(int usuarioid)
        {
            var usuario = await _context.UsuariosRegistrados.FindAsync(usuarioid);
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            var historial = await _context.Transacciones
                .Where(t => t.UsuarioId == usuarioid)
                .OrderByDescending(t => t.Fecha)
                .Include(t => t.Cripto)
                .Include(t => t.Mercado)
                .Select(t => new
                {
                    t.Id,
                    t.Operacion, 
                    t.CantCripto,
                    t.CantPesos,
                    t.Fecha,
                    CriptoNombre = t.Cripto.Nombre,
                    MercadoNombre = t.Mercado.Nombre
                })
                .ToListAsync();

            return Ok(historial);
        }

        [HttpPost("Transaccion")]
        public async Task<IActionResult> PostTransaccion([FromBody] TransaccionDto dto)
        {
            try
            {
                if (dto.CantCripto <= 0)
                    return BadRequest("La cantidad de cripto debe ser mayor a cero.");

                var usuario = await _context.UsuariosRegistrados.FindAsync(dto.UsuarioId);
                var cripto = await _context.Criptos.FindAsync(dto.CriptoId);
                var mercado = await _context.CriptoMarket.FindAsync(dto.MercadoId);

                if (usuario == null) return NotFound("Usuario no encontrado");
                if (cripto == null) return NotFound("Cripto no encontrada");
                if (mercado == null) return NotFound("Mercado no encontrado");

                decimal? totalPesos = 0;

                if (dto.Operacion.ToLower() == "compra")
                {
                    decimal? precio = await GetBuyPriceAsync(mercado.Nombre, cripto.CriptoCodigo);
                    if (precio == null)
                        return BadRequest("No se pudo obtener el precio de compra.");

                    totalPesos = dto.CantCripto * precio.Value;

                    var billetera = await _context.billeteraPesos.FirstOrDefaultAsync(b => b.UsuarioId == dto.UsuarioId);
                    if (billetera == null)
                        return BadRequest("El usuario no tiene billetera de pesos.");

                    if (billetera.Importe < totalPesos)
                        return BadRequest("Saldo insuficiente en la billetera de pesos.");

                    billetera.Importe -= totalPesos.Value;
                    _context.billeteraPesos.Update(billetera);
                    await _context.SaveChangesAsync();
                }
                else if (dto.Operacion.ToLower() == "venta")
                {
                    decimal? precio = await GetSellPriceAsync(mercado.Nombre, cripto.CriptoCodigo);
                    if (precio == null)
                        return BadRequest("No se pudo obtener el precio de venta.");

                    totalPesos = dto.CantCripto * precio.Value;

                    decimal saldoBilleteraCripto = await BalanceBilleteraCripto(dto.UsuarioId, dto.CriptoId);
                    if (saldoBilleteraCripto < dto.CantCripto)
                        return BadRequest("Saldo insuficiente en la billetera de cripto.");
                }
                else
                {
                    return BadRequest("Operación no válida. Debe ser 'compra' o 'venta'.");
                }

                var transaccion = new Transaction
                {
                    UsuarioId = dto.UsuarioId,
                    CriptoId = dto.CriptoId,
                    MercadoId = dto.MercadoId,
                    Operacion = dto.Operacion,
                    CantCripto = dto.CantCripto,
                    CantPesos = totalPesos ?? 0,
                    Fecha = dto.Fecha == default ? DateTime.Now : dto.Fecha
                };

                _context.Transacciones.Add(transaccion);
                await _context.SaveChangesAsync();

                var resultado = await _context.Transacciones
                    .Include(t => t.Cripto)
                    .Include(t => t.Mercado)
                    .Where(t => t.Id == transaccion.Id)
                    .Select(t => new
                    {
                        t.Id,
                        t.Operacion,
                        t.CantCripto,
                        t.CantPesos,
                        t.Fecha,
                        CriptoNombre = t.Cripto.Nombre,
                        MercadoNombre = t.Mercado.Nombre
                    })
                    .FirstOrDefaultAsync();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }




        private async Task<decimal?> GetBuyPriceAsync(string Mercado, string Criptos)
        {
            try
            {
                var url = $"{Mercado.ToLower()}/{Criptos.ToUpper()}/ARS/1";
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No se pudo obtener precio de compra para {Cripto} en {Mercado}. Status: {Status}. Respuesta: {Respuesta}", Criptos, Mercado, response.StatusCode, content);
                    return null;
                }

                var dict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(content);
                return dict != null && dict.ContainsKey("ask") ? dict["ask"] : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener precio de compra para {Cripto} en {Mercado}", Criptos, Mercado);
                return null;
            }
        }

        private async Task<decimal?> GetSellPriceWithRetry(string mercado, string cripto, int maxRetries = 3)
        {
            int retryCount = 0;
            while (retryCount < maxRetries)
            {
                try
                {
                    var precio = await GetSellPriceAsync(mercado, cripto);
                    if (precio != null) return precio;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Intento {Retry} fallido para obtener precio de {Cripto}", retryCount + 1, cripto);
                }
                retryCount++;
                await Task.Delay(1000 * retryCount);
            }
            return null;
        }
        private async Task<decimal?> GetSellPriceAsync(string mercado, string cripto)
        {
            try
            {
                var url = $"{mercado.ToLower()}/{cripto.ToUpper()}/ARS/1";

                // Log de la solicitud que se va a realizar
                _logger.LogDebug("Solicitando precio de venta a {Url}", url);

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No se pudo obtener precio de venta para {Cripto} en {Mercado}. Status: {Status}. Respuesta: {Respuesta}",
                        cripto, mercado, response.StatusCode, content);
                    return null;
                }

                // Verifica si la respuesta es JSON válido
                string trimmed = content.TrimStart();
                if (!(trimmed.StartsWith("{") || trimmed.StartsWith("[")))
                {
                    _logger.LogWarning("Respuesta no JSON para {Cripto} en {Mercado}: {Content}",
                        cripto, mercado, content);
                    return null;
                }

                // Intenta deserializar la respuesta
                try
                {
                    var dict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(content);

                    if (dict == null)
                    {
                        _logger.LogWarning("Respuesta JSON inválida (null) para {Cripto} en {Mercado}", cripto, mercado);
                        return null;
                    }

                    if (!dict.ContainsKey("bid"))
                    {
                        _logger.LogWarning("La respuesta no contiene el campo 'bid' para {Cripto} en {Mercado}. Datos: {Data}",
                            cripto, mercado, content);
                        return null;
                    }

                    // Log exitoso
                    _logger.LogDebug("Precio de venta obtenido para {Cripto} en {Mercado}: {Precio}",
                        cripto, mercado, dict["bid"]);

                    return dict["bid"];
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Error al deserializar respuesta JSON para {Cripto} en {Mercado}",
                        cripto, mercado);
                    return null;
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Error de conexión al obtener precio para {Cripto} en {Mercado}",
                    cripto, mercado);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener precio para {Cripto} en {Mercado}",
                    cripto, mercado);
                return null;
            }
        }

        private async Task<BilleteraPesos> BalanceBilleteraPesos(int usuarioId)
        {
            var billetera = await _context.billeteraPesos.AsNoTracking().FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

            decimal saldoBilletera = billetera.Importe;


            var transacciones = await _context.Transacciones
                .Where(t => t.UsuarioId == usuarioId && (t.Operacion == "compra" || t.Operacion == "venta"))
                .ToListAsync();
            foreach (var transaccion in transacciones)
            {
                if (transaccion.Operacion == "compra")
                {
                    saldoBilletera -= transaccion.CantPesos;
                }
                else if (transaccion.Operacion == "venta")
                {
                    saldoBilletera += transaccion.CantPesos;
                }
            }
            return new BilleteraPesos
            {
                UsuarioId = usuarioId,
                Importe = saldoBilletera
            };
        }
        private async Task<decimal> BalanceBilleteraCripto(int usuarioId, int criptoId)
        {
            var billetera = await _context.billeteraCripto.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.Id == criptoId);
            if (billetera == null)
            {
                return 0;
            }
            return billetera.CantidadCripto;
        }

        
    }

}
