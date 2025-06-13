namespace PruebaFina.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Gmail { get; set; }
        public string Contrasenia { get; set; }

        public ICollection<Transaction> transactions { get; set; } = new List<Transaction>();

    }
}
