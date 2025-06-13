using System.Text.Json.Serialization;
namespace PruebaFina.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int CriptoId { get; set; }
        public int MercadoId { get; set; }
        public string Operacion { get; set; }
        public decimal CantCripto { get; set; }
        public decimal CantPesos { get; set; }
        public DateTime Fecha { get; set; }

        [JsonIgnore] public virtual Usuario Usuario { get; set; }
        [JsonIgnore] public virtual Cripto Cripto { get; set; }
        [JsonIgnore] public virtual Mercado Mercado { get; set; }

    }
}
