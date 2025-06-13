using Microsoft.EntityFrameworkCore;
using PruebaFina.Models;

namespace PruebaFina.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> UsuariosRegistrados { get; set; }
        public DbSet<BilleteraPesos> billeteraPesos { get; set; }
        public DbSet<Cripto> Criptos { get; set; }
        public DbSet<Mercado> CriptoMarket { get; set; }
        public DbSet<Transaction> Transacciones { get; set; }
        public DbSet<BilleteraCripto> billeteraCripto { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().ToTable("UsuariosRegistrados");
            modelBuilder.Entity<BilleteraPesos>().ToTable("billeteraPesos");
            modelBuilder.Entity<BilleteraCripto>().HasKey(b => b.Id);
            modelBuilder.Entity<Cripto>().ToTable("Criptos");
            modelBuilder.Entity<Mercado>().ToTable("CriptoMarket");
            modelBuilder.Entity<Transaction>().ToTable("Transacciones");

            // billeteraPesos -> UsuariosRegistrados
            modelBuilder.Entity<BilleteraPesos>()
                .HasOne(b => b.Usuario)
                .WithMany()
                .HasForeignKey(b => b.UsuarioId);

            // Transacciones -> UsuariosRegistrados
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Usuario)
                .WithMany(u => u.transactions)
                .HasForeignKey(t => t.UsuarioId);

            // Transacciones -> Criptos
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Cripto)
                .WithMany(c => c.transactions)
                .HasForeignKey(t => t.CriptoId);

            // Transacciones -> CriptoMarket
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Mercado)
                .WithMany(m => m.transactions)
                .HasForeignKey(t => t.MercadoId);
        }
    }
}


