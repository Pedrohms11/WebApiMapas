using Microsoft.EntityFrameworkCore;
using ConsoleLog.Models;

namespace ConsoleLog.Data
{
    public class AppDbContext : DbContext
    {
        private readonly string _connectionString;

        // Construtor com validação de null
        public AppDbContext(string connectionString)
        {
            // ✅ Adicionar validação para evitar null
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Connection string não pode ser nula ou vazia");

            _connectionString = connectionString;
        }

        public DbSet<Localizacao> Localizacoes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
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