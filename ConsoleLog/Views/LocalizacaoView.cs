using ConsoleLog.ViewModels;
using ConsoleLog.Models;
using ConsoleLog.Services;

namespace ConsoleLog.Views
{
    /// <summary>
    /// Classe responsável pela interface de usuário no console para o sistema de monitoramento de localizações
    /// </summary>
    public class LocalizacaoView
    {
        private readonly LocalizacaoViewModel _viewModel;
        private readonly LogService _logger;

        /// <summary>
        /// Inicializa uma nova instância da View de localizações
        /// </summary>
        /// <param name="viewModel">ViewModel responsável pela lógica de negócio</param>
        /// <param name="logger">Serviço de logging do sistema</param>
        public LocalizacaoView(LocalizacaoViewModel viewModel, LogService logger)
        {
            _viewModel = viewModel;
            _logger = logger;
        }

        /// <summary>
        /// Executa o loop principal do menu interativo do sistema
        /// </summary>
        public async Task RunAsync()
        {
            await Sincronizar();
            while (true)
            {
                Console.Clear();
                Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
║                    SISTEMA DE MONITORAMENTO FIREBASE - AUDITORIA EM TEMPO REAL                                ║
╠═══════════════════════════════════════════════════════════════════════════════════════════════════════════════╣
║ 1 - Listar Localizações    2 - Buscar por ID    3 - Buscar por CEP    4 - Buscar por Bairro                   ║
║ 5 - Buscar por Período     6 - Sincronizar      7 - Estatísticas                                              ║
║ 8 - Histórico Alterações   9 - Logs Requisições 0 - Sair                                                      ║
╚═══════════════════════════════════════════════════════════════════════════════════════════════════════════════╝");
                Console.Write("\n👉 Opção: ");
                var opcao = Console.ReadLine();
                switch (opcao)
                {
                    case "1": await Listar(); break;
                    case "2": await BuscarId(); break;
                    case "3": await BuscarCep(); break;
                    case "4": await BuscarBairro(); break;
                    case "5": await BuscarPeriodo(); break;
                    case "6": await Sincronizar(); break;
                    case "7": await Estatisticas(); break;
                    case "8": await Historico(); break;
                    case "9": await LogsRequisicao(); break;
                    case "0": return;
                }
                Console.WriteLine("\nPressione qualquer tecla...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Lista todas as localizações disponíveis no banco de dados local
        /// </summary>
        private async Task Listar()
        {
            var list = await _viewModel.ObterTodasLocalizacoes();
            Console.Clear();
            Console.WriteLine($"\n📌 Total: {list.Count}\n");
<<<<<<< HEAD
=======

>>>>>>> 0803e68c81502ac05182e2b46359eaf802f1d777
            foreach (var l in list)
                Console.WriteLine($"ID: {l.Id} | {l.Logradouro}, {l.Numero} - {l.Bairro} | {l.Timestamp:dd/MM/yyyy HH:mm}");
        }

<<<<<<< HEAD
        /// <summary>
        /// Busca e exibe uma localização específica pelo seu identificador único
        /// </summary>
=======
>>>>>>> 0803e68c81502ac05182e2b46359eaf802f1d777
        private async Task BuscarId()
        {
            Console.Write("ID: ");
            var id = Console.ReadLine();
            var l = await _viewModel.BuscarLocalizacaoPorId(id ?? "");
<<<<<<< HEAD
=======

>>>>>>> 0803e68c81502ac05182e2b46359eaf802f1d777
            if (l != null)
                Console.WriteLine($"📍 {l.Logradouro}, {l.Numero} - {l.Bairro} | {l.Timestamp}");
            else
                Console.WriteLine("Não encontrado");
        }

<<<<<<< HEAD
        /// <summary>
        /// Busca localizações por CEP e exibe a quantidade encontrada
        /// </summary>
=======
>>>>>>> 0803e68c81502ac05182e2b46359eaf802f1d777
        private async Task BuscarCep()
        {
            Console.Write("CEP: ");
            var list = await _viewModel.BuscarLocalizacoesPorCep(Console.ReadLine() ?? "");
            Console.WriteLine($"Encontrados: {list.Count}");
        }

<<<<<<< HEAD
        /// <summary>
        /// Busca localizações por bairro (busca parcial) e exibe a quantidade encontrada
        /// </summary>
=======
>>>>>>> 0803e68c81502ac05182e2b46359eaf802f1d777
        private async Task BuscarBairro()
        {
            Console.Write("Bairro: ");
            var list = await _viewModel.BuscarLocalizacoesPorBairro(Console.ReadLine() ?? "");
            Console.WriteLine($"Encontrados: {list.Count}");
        }

<<<<<<< HEAD
        /// <summary>
        /// Busca localizações dentro de um período de datas específico
        /// </summary>
=======
>>>>>>> 0803e68c81502ac05182e2b46359eaf802f1d777
        private async Task BuscarPeriodo()
        {
            Console.Write("Início (yyyy-mm-dd): ");
            var i = DateTime.Parse(Console.ReadLine() ?? "");
            Console.Write("Fim: ");
            var f = DateTime.Parse(Console.ReadLine() ?? "");
            var list = await _viewModel.BuscarLocalizacoesPorPeriodo(i, f);
            Console.WriteLine($"Encontrados: {list.Count}");
        }

<<<<<<< HEAD
        /// <summary>
        /// Executa a sincronização de dados entre Firestore e banco local
        /// </summary>
=======
>>>>>>> 0803e68c81502ac05182e2b46359eaf802f1d777
        private async Task Sincronizar()
        {
            var r = await _viewModel.SincronizarDados();
            _logger.LogSync(r.Mensagem, r.Sucesso);
        }

<<<<<<< HEAD
        /// <summary>
        /// Exibe estatísticas consolidadas dos dados locais
        /// </summary>
=======
>>>>>>> 0803e68c81502ac05182e2b46359eaf802f1d777
        private async Task Estatisticas()
        {
            var s = await _viewModel.ObterEstatisticasLocais();
            Console.WriteLine($"Total: {s.TotalRegistros} | Última sync: {s.UltimaSincronizacao}");
        }

<<<<<<< HEAD
        /// <summary>
        /// Exibe o histórico de alterações (auditoria) das últimas 20 operações
        /// </summary>
=======
>>>>>>> 0803e68c81502ac05182e2b46359eaf802f1d777
        private async Task Historico()
        {
            var h = await _viewModel.ObterTodasAlteracoes();
            Console.WriteLine($"📋 Total alterações: {h.Count}");
<<<<<<< HEAD
=======

>>>>>>> 0803e68c81502ac05182e2b46359eaf802f1d777
            foreach (var a in h.Take(20))
                Console.WriteLine($"[{a.DataHora:dd/MM HH:mm}] {a.Acao} - {a.Tabela} ID:{a.RegistroId} - {a.Usuario}");
        }

<<<<<<< HEAD
        /// <summary>
        /// Exibe os logs de requisição das últimas 20 requisições
        /// </summary>
=======
>>>>>>> 0803e68c81502ac05182e2b46359eaf802f1d777
        private async Task LogsRequisicao()
        {
            var l = await _viewModel.ObterTodosLogsRequisicao();
            Console.WriteLine($"📜 Total requisições: {l.Count}");
            foreach (var log in l.Take(20))
                Console.WriteLine($"[{log.DataHora:HH:mm:ss}] {log.Operacao} {log.Endpoint} - {log.DuracaoMs}ms - {(log.Sucesso ? "✅" : "❌")}");
        }
    }
}