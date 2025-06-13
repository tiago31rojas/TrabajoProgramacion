namespace PruebaFina.Models
{
    public class Mercado
    {
        public int Id { get; set; }
        public string Codigo { get; set; }

        public string Nombre { get; set; }

        public ICollection<Transaction> transactions{ get; set; } = new List<Transaction>();

    }
}
