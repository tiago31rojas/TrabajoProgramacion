using Azure.Identity;

namespace PruebaFina.Models
{
    public class BilleteraCripto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string CodigoCriptoId { get; set; }
        public decimal  CantidadCripto { get; set; }
        public decimal PrecioCripto { get; set; }
        public decimal TotalPesos { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual Cripto Cripto { get; set; }

    }
}
