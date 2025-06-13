namespace PruebaFina.Models
{
    public class TransaccionDto
    {
        public int UsuarioId { get; set; }
        public int CriptoId { get; set; }
        public int MercadoId { get; set; }

        public string Operacion { get; set; }
        public decimal CantCripto { get; set; }
        public DateTime Fecha { get; set; }
    }
}
