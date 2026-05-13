using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConsoleLog.Data;
using ConsoleLog.Services;
using ConsoleLog.Services.Sync;
using ConsoleLog.ViewModels;
using ConsoleLog.Views;

namespace ConsoleLog
{
    /// <summary>
    /// Classe principal do programa que configura e inicializa o sistema de monitoramento
    /// </summary>
    class Program
    {
        /// <summary>
        /// Método principal de entrada do programa
        /// </summary>
        /// <param name="args">Argumentos de linha de comando</param>
        static async Task Main(string[] args)
        {
            Console.Title = "Monitor Firebase - Auditoria";

            // Configuração do arquivo de configurações
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var conn = config["Database:SqliteConnectionString"];

            // Configuração do container de injeção de dependência
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);
            services.AddSingleton<LogService>();
            services.AddSingleton(new AppDbContext(conn!));
            services.AddSingleton<FirestoreService>();
            services.AddSingleton<AuditoriaService>();
            services.AddSingleton<RequisicaoLoggerService>();
            services.AddSingleton<RealtimeMonitorService>();
            services.AddSingleton<DataSyncService>();
            services.AddSingleton<LocalizacaoViewModel>();
            services.AddSingleton<LocalizacaoView>();

            // Construção do provedor de serviços
            var sp = services.BuildServiceProvider();

            // Aplicação das migrações do banco de dados
            var db = sp.GetRequiredService<AppDbContext>();
            db.ApplyMigrations();

            // Inicialização do monitoramento em tempo real
            var monitor = sp.GetRequiredService<RealtimeMonitorService>();
            await monitor.IniciarMonitoramento();

            // Execução da interface principal
            var view = sp.GetRequiredService<LocalizacaoView>();
            await view.RunAsync();

            // Parada do monitoramento ao encerrar
            monitor.PararMonitoramento();
        }
    }
}