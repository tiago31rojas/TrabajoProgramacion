namespace PruebaFina.Models
{
    public class BilleteraPesos
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public decimal Importe { get; set; }

        public virtual Usuario Usuario { get; set; }

    }

}
