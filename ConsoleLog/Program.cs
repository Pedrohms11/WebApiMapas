using ConsoleLog.Data;
using ConsoleLog.Services;
using ConsoleLog.Services.Sync;
using ConsoleLog.View;
using ConsoleLog.ViewModel;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleLog
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Sistema de Localizações - READ ONLY";

            // Configuração
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Configurar DI
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<LogService>();

            // ✅ Garantir que connectionString não é nula
            var connectionString = configuration["Database:SqliteConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("ERRO: Connection string do SQLite não configurada no appsettings.json");
                Console.WriteLine("Pressione qualquer tecla para sair...");
                Console.ReadKey();
                return;
            }

            var dbContext = new AppDbContext(connectionString);
            dbContext.ApplyMigrations();
            services.AddSingleton(dbContext);

            // Configurar Firestore
            services.AddSingleton<FirestoreService>();
            services.AddSingleton<DataSyncService>();
            services.AddSingleton<LocalizacaoViewModel>();
            services.AddSingleton<LocalizacaoView>();

            var serviceProvider = services.BuildServiceProvider();

            // Verificar conexão com Firestore
            var firestoreService = serviceProvider.GetRequiredService<FirestoreService>();
            var firestoreConnected = await firestoreService.VerificarConexao();

            if (!firestoreConnected)
            {
                Console.WriteLine("\n⚠ ATENÇÃO: Não foi possível conectar ao Firestore!");
                Console.WriteLine("Verifique suas credenciais e conexão com internet.");
                Console.WriteLine("\nPressione qualquer tecla para sair...");
                Console.ReadKey();
                return;
            }

            var view = serviceProvider.GetRequiredService<LocalizacaoView>();
            await view.RunAsync();
        }
    }
}