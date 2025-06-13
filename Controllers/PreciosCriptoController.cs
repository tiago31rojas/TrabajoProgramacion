using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PruebaFina.Controllers;
using PruebaFina.Data;
using System;
using System.Net.Http;
using System.Text.Json;

namespace trabajoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreciosCriptoController : ControllerBase
    {

        
        private readonly HttpClient _httpClient;
        private readonly ILogger<TransaccionController> _logger;
        private readonly AppDbContext _context;

        public PreciosCriptoController(IHttpClientFactory httpClientFactory, AppDbContext appContext, ILogger<TransaccionController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _context = appContext;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("https://criptoya.com/api/");
        }

        [HttpGet("precios")]
        public async Task<IActionResult> GetPreciosCripto()
        {
            var criptos = new[] { "BTC", "ETH", "USDC" };
            var mercados = new[] { "Binance", "Buenbit", "LemonCash" };

            var resultados = new List<object>();

            foreach (var mercado in mercados)
            {
                foreach (var cripto in criptos)
                {
                    decimal? precioCompra = null;
                    decimal? precioVenta = null;
                    try
                    {
              
                        precioCompra = await GetBuyPriceAsync(mercado, cripto);
                        precioVenta = await GetSellPriceAsync(mercado, cripto);
                    }
                    catch
                    {
                        // Si hay error, los valores quedan en null
                    }

                    resultados.Add(new
                    {
                        Mercado = mercado,
                        Cripto = cripto,
                        PrecioCompra = precioCompra,
                        PrecioVenta = precioVenta
                    });
                }
            }

            return Ok(resultados);
        }

        private async Task<decimal?> GetBuyPriceAsync(string Mercado, string Criptos)
        {
            try
            {
                var url = $"{Mercado.ToLower()}/{Criptos.ToUpper()}/ARS/1";
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                // Verifica si la respuesta parece JSON (empieza con { o [ )
                string trimmed = content.TrimStart();
                if (!(trimmed.StartsWith("{") || trimmed.StartsWith("[")))
                {
                    _logger.LogWarning("Respuesta no JSON para {Cripto} en {Mercado}: {Content}", Criptos, Mercado, content);
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

        private async Task<decimal?> GetSellPriceAsync(string Mercado, string Criptos)
        {
            try
            {
                var url = $"{Mercado.ToLower()}/{Criptos.ToUpper()}/ARS/1";
                var response = await _httpClient.GetAsync(url);

                var content = await response.Content.ReadAsStringAsync();

                var dict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(content);
                return dict != null && dict.ContainsKey("bid") ? dict["bid"] : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener precio de compra para {Cripto} en {Mercado}", Criptos, Mercado);
                throw;
            }

        }
    }
}
