using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entities;

namespace minimal_api.Infraestructure.DB
{
    public class AppDbContext : DbContext
    {

        private readonly IConfiguration _configuration;


        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<Administrador> Administradores { get; set; } = default!;
        public DbSet<Veiculo> Veiculos { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Administrador>().HasData(
                new Administrador
                {
                    Id = 1,
                    Email = "admin@teste.com",
                    Senha = "123456",
                    Perfil = "Admin"
                }
            );
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var stringConnection = _configuration.GetConnectionString("SqlServer")?.ToString();
                if (!string.IsNullOrEmpty(stringConnection))
                {
                    optionsBuilder.UseSqlServer(stringConnection);
                }
            }
        }

    }
}
