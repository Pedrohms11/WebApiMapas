using ConsoleLog.Data;
using ConsoleLog.Services;
using ConsoleLog.Services.Sync;
using ConsoleLog.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleLog
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Sistema de Localizações - READ ONLY";

            Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════╗
║     SISTEMA DE CONSULTA DE LOCALIZAÇÕES - READ ONLY MODE          ║
║                    Firebase Firestore + SQLite                    ║
╚═══════════════════════════════════════════════════════════════════╝
");

            // Configuração
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Configurar DI (Dependency Injection)
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<LogService>();

            // Configurar SQLite
            var connectionString = configuration["Database:SqliteConnectionString"];
            var dbContext = new AppDbContext(connectionString);
            dbContext.ApplyMigrations(); // Migração automática
            services.AddSingleton(dbContext);

            // Configurar Firestore (APENAS LEITURA)
            services.AddSingleton<FirestoreService>();

            // Configurar Serviço de Sincronização
            services.AddSingleton<DataSyncService>();

            // Configurar ViewModel
            services.AddSingleton<LocalizacaoViewModel>();

            // Configurar View
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

            // Executar a aplicação
            var view = serviceProvider.GetRequiredService<LocalizacaoView>();
            await view.RunAsync();
        }
    }
}