using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConsoleLog.Data;
using ConsoleLog.Services;
using ConsoleLog.Services.Sync;
using ConsoleLog.ViewModels;
using ConsoleLog.Views;

namespace ConsoleLog
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Monitor Firebase - Auditoria";

            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            var conn = config["Database:SqliteConnectionString"];

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

            var sp = services.BuildServiceProvider();
            var db = sp.GetRequiredService<AppDbContext>();
            db.ApplyMigrations();

            var monitor = sp.GetRequiredService<RealtimeMonitorService>();
            await monitor.IniciarMonitoramento();

            var view = sp.GetRequiredService<LocalizacaoView>();
            await view.RunAsync();

            monitor.PararMonitoramento();
        }
    }
}