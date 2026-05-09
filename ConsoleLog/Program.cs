using ConsoleLog.Data;
using ConsoleLog.Services;
using ConsoleLog.Services.Sync;
using ConsoleLog.View;
using ConsoleLog.ViewModel;
using ConsoleLog.ViewModel;
using ConsoleLog.View;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleLog
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Sistema de Monitoramento Firebase - Auditoria em Tempo Real";

            Console.Clear();
            Console.WriteLine(@"
╔══════════════════════════════════════════════════════════════════════════════╗
║                    SISTEMA DE MONITORAMENTO FIREBASE                         ║
║                         AUDITORIA EM TEMPO REAL                              ║
║                                                                              ║
║  Este sistema monitora TODAS as alterações no Firebase Firestore            ║
║  e registra localmente QUEM e QUANDO realizou cada ação                      ║
╚══════════════════════════════════════════════════════════════════════════════╝
");

            // Configuração
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Configurar DI
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<LogService>();

            // Configurar SQLite
            var connectionString = configuration["Database:SqliteConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("ERRO: Connection string do SQLite não configurada");
                Console.ReadKey();
                return;
            }

            var dbContext = new AppDbContext(connectionString);
            dbContext.ApplyMigrations();
            services.AddSingleton(dbContext);

            // Configurar serviços
            services.AddSingleton<FirestoreService>();
            services.AddSingleton<AuditoriaService>();
            services.AddSingleton<RealtimeMonitorService>();
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

            // Iniciar monitoramento em tempo real
            Console.WriteLine("\n🔄 Iniciando monitoramento em tempo real...");
            var monitorService = serviceProvider.GetRequiredService<RealtimeMonitorService>();
            await monitorService.IniciarMonitoramento();

            Console.WriteLine("\n✅ Sistema de auditoria ativo!");
            Console.WriteLine("📝 Todas as alterações no Firebase serão registradas localmente.");
            Console.WriteLine("🔍 Pressione qualquer tecla para continuar...");
            Console.ReadKey();

            // Executar a aplicação
            var view = serviceProvider.GetRequiredService<LocalizacaoView>();
            await view.RunAsync();
        }
    }
}