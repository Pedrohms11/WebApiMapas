using Microsoft.EntityFrameworkCore;
using ConsoleLog.Models;

namespace ConsoleLog.Data
{
    public class AppDbContext : DbContext
    {
        // Construtor sem parâmetros para o EF Tools (design time)
        public AppDbContext()
        {
        }

        // Construtor com parâmetros para sua aplicação (runtime)
        public AppDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        private string? _connectionString;

        public DbSet<Localizacao> Localizacoes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Se uma connection string foi fornecida (runtime), usa ela
            if (!string.IsNullOrEmpty(_connectionString))
            {
                optionsBuilder.UseSqlite(_connectionString);
            }
            // Se não (design time), usa uma connection string padrão para migrações
            else
            {
                optionsBuilder.UseSqlite("Data Source=localizacao.db");
            }

            optionsBuilder.EnableSensitiveDataLogging(false);
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
                entity.Property(e => e.DataHash).HasMaxLength(64);
                entity.Property(e => e.LastSyncAt);

                entity.HasIndex(e => e.Cep).HasDatabaseName("IX_Localizacoes_Cep");
                entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_Localizacoes_Timestamp");
                entity.HasIndex(e => e.LastSyncAt).HasDatabaseName("IX_Localizacoes_LastSyncAt");
            });
        }

        public void ApplyMigrations()
        {
            Database.Migrate();
        }
    }
}