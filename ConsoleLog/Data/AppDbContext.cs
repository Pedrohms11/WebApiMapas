using Microsoft.EntityFrameworkCore;
using ConsoleLog.Models;

namespace ConsoleLog.Data
{
    public class AppDbContext : DbContext
    {
        private readonly string? _connectionString;

        // Construtor sem parâmetros para o EF Tools (design time)
        public AppDbContext()
        {
        }

        // Construtor com parâmetros para sua aplicação (runtime)
        public AppDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<Localizacao> Localizacoes { get; set; }
        public DbSet<Auditoria> Auditoria { get; set; }
        public DbSet<LogRequisicao> LogsRequisicao { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connection = !string.IsNullOrEmpty(_connectionString)
                    ? _connectionString
                    : "Data Source=localizacao.db";

                optionsBuilder.UseSqlite(connection);
                optionsBuilder.EnableSensitiveDataLogging(true);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração Localizacao
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

                entity.HasIndex(e => e.Cep);
                entity.HasIndex(e => e.Timestamp);
            });

            // Configuração Auditoria
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
            });

            // Configuração LogRequisicao
            modelBuilder.Entity<LogRequisicao>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Operacao).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Endpoint).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Parametros).HasMaxLength(200);
                entity.Property(e => e.RequestBody).HasMaxLength(500);
                entity.Property(e => e.ResponseBody).HasMaxLength(500);
                entity.Property(e => e.Usuario).HasMaxLength(100);
                entity.Property(e => e.EmailUsuario).HasMaxLength(100);
                entity.Property(e => e.PerfilUsuario).HasMaxLength(50);
                entity.Property(e => e.Maquina).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.MensagemErro).HasMaxLength(500);
                entity.Property(e => e.Origem).HasMaxLength(50);
                entity.Property(e => e.Categoria).HasMaxLength(50);

                entity.HasIndex(e => e.Operacao);
                entity.HasIndex(e => e.Endpoint);
                entity.HasIndex(e => e.DataHora);
                entity.HasIndex(e => e.Usuario);
                entity.HasIndex(e => e.Sucesso);
            });
        }

        public void ApplyMigrations()
        {
            try
            {
                // Verificar se o banco existe
                var dbExists = File.Exists("localizacao.db");

                if (!dbExists)
                {
                    Database.Migrate();
                }
                else
                {
                    // Verificar se a tabela __EFMigrationsHistory existe
                    var hasHistoryTable = Database.SqlQueryRaw<int>(
                        "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory'")
                        .ToList()
                        .FirstOrDefault() > 0;

                    if (!hasHistoryTable)
                    {
                        // Recriar o banco se a tabela de migrações não existir
                        Database.EnsureDeleted();
                        Database.Migrate();
                    }
                    else if (Database.GetPendingMigrations().Any())
                    {
                        Database.Migrate();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao aplicar migrações: {ex.Message}");
                // Tentar recriar o banco
                try
                {
                    Database.EnsureDeleted();
                    Database.Migrate();
                    Console.WriteLine("Banco de dados recriado com sucesso!");
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"Erro fatal ao recriar banco: {ex2.Message}");
                    throw;
                }
            }
        }
    }
}