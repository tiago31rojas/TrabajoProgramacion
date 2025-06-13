namespace PruebaFina.Models
{
    public class Cripto
    {

        public int Id { get; set; }
        public string CriptoCodigo { get; set; }
        public string Nombre { get; set; }

        public ICollection<Transaction> transactions{ get; set; } = new List<Transaction>();


    }
}
