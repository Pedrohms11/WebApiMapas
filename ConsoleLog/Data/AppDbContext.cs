using Microsoft.EntityFrameworkCore;
using ConsoleLog.Models;

namespace ConsoleLog.Data
{
    public class AppDbContext : DbContext
    {
        private readonly string _connectionString;

        public AppDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<Localizacao> Localizacoes { get; set; }
        public DbSet<Auditoria> Auditoria { get; set; } // NOVO

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
            optionsBuilder.EnableSensitiveDataLogging(true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Localizacao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Logradouro).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Numero).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Bairro).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Cep).IsRequired().HasMaxLength(8);
                entity.Property(e => e.Latitude).IsRequired();
                entity.Property(e => e.Longitude).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
            });

            // Configuração da tabela de Auditoria
            modelBuilder.Entity<Auditoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Tabela).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RegistroId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Acao).IsRequired().HasMaxLength(20);
                entity.Property(e => e.DadosAntigos).IsRequired();
                entity.Property(e => e.DadosNovos).IsRequired();
                entity.Property(e => e.Usuario).HasMaxLength(100);
                entity.Property(e => e.EmailUsuario).HasMaxLength(100);
                entity.Property(e => e.PerfilUsuario).HasMaxLength(50);
                entity.Property(e => e.Maquina).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.Detalhes).HasMaxLength(500);
                entity.Property(e => e.Origem).HasMaxLength(50);

                entity.HasIndex(e => e.Tabela);
                entity.HasIndex(e => e.RegistroId);
                entity.HasIndex(e => e.Acao);
                entity.HasIndex(e => e.DataHora);
                entity.HasIndex(e => e.Usuario);
                entity.HasIndex(e => e.EmailUsuario);
                entity.HasIndex(e => e.Origem);
            });
        }

        public void ApplyMigrations()
        {
            Database.Migrate();
        }
    }
}