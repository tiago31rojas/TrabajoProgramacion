using Microsoft.EntityFrameworkCore;
using PruebaFina.Models;

namespace PruebaFina.DBcontext
{
    public class UsuarioDbContext : DbContext
    {
        public UsuarioDbContext(DbContextOptions<UsuarioDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> UsuariosRegistrados { get; set; }

        public DbSet<BilleteraPesos> BilleteraPesos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().ToTable("UsuariosRegistrados");
        }

      
    }
}

