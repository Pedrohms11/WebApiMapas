using ConsoleLog.Models;


namespace ConsoleLog.Data
{
    public class AppDbContext : DbContext
    /// <summary>
    /// Contexto de banco de dados para a aplicação, utilizando Entity Framework Core
    /// </summary>
    {
        private readonly string _connectionString;

        public AppDbContext(string connectionString)
        {
            _connectionString = connectionString;

        } /// <summary>
          /// Tabela de Localizações
          /// </summary>
        public DbSet<Localizacao> Localizacoes { get; set; }

        /// <summary>
        /// Configuração do banco de dados SQLite
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
            optionsBuilder.EnableSensitiveDataLogging(false);
        }

        /// <summary>
        /// Configuração do modelo de dados
        /// </summary>
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

                // Índices para melhor performance
                entity.HasIndex(e => e.Cep).HasDatabaseName("IX_Localizacoes_Cep");
                entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_Localizacoes_Timestamp");
            });
        }

        /// <summary>
        /// Aplica migrações automaticamente
        /// </summary>
        public void ApplyMigrations()
        {
            Database.Migrate();
        }
    }



}

