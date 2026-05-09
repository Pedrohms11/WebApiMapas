
using ConsoleLog.Models;
using ConsoleLog.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleLog.ViewModels;

namespace ConsoleLog.Views
{
    /// <summary>
    /// View do padrão MVVM - Interface de usuário para consulta e auditoria
    /// </summary>
    public class LocalizacaoView
    {
        private readonly LocalizacaoViewModel _viewModel;
        private readonly LogService _logger;
        private readonly string _usuario;
        private bool _dadosCarregados = false;

        public LocalizacaoView(LocalizacaoViewModel viewModel, LogService logger)
        {
            _viewModel = viewModel;
            _logger = logger;
            _usuario = System.Environment.UserName;

            // Inscrever nos eventos do ViewModel
            _viewModel.OnOperationCompleted += (s, msg) =>
                _logger.LogSuccess($"Notificação: {msg}", "VIEW");

            _viewModel.OnErrorOccurred += (s, ex) =>
                _logger.LogError($"Erro na operação: {ex.Message}", ex, "VIEW");

            _viewModel.OnSyncCompleted += (s, result) =>
            {
                if (result.Sucesso)
                {
                    _logger.LogSuccess($"Sincronização concluída: +{result.NovosRegistros} | ~{result.RegistrosAtualizados} | -{result.RegistrosRemovidos}", "VIEW");
                }
            };
        }

        public async Task RunAsync()
        {
            // Sincronização inicial ao iniciar
            await RealizarSincronizacao();

            while (true)
            {
                Console.Clear();
                Console.WriteLine(@"╔═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine(@"║                         SISTEMA DE CONSULTA DE LOCALIZAÇÕES - READ ONLY                                        ║");
                Console.WriteLine(@"║                                    FIREBASE FIRESTORE + SQLITE                                                  ║");
                Console.WriteLine(@"╠═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════╣");
                Console.WriteLine(@"║                                                                                                               ║");
                Console.WriteLine(@"║  📍 OPERAÇÕES DE CONSULTA:                                                                                     ║");
                Console.WriteLine(@"║     1 - Listar Todas Localizações                                                                              ║");
                Console.WriteLine(@"║     2 - Buscar por ID                                                                                          ║");
                Console.WriteLine(@"║     3 - Buscar por CEP                                                                                         ║");
                Console.WriteLine(@"║     4 - Buscar por Bairro                                                                                      ║");
                Console.WriteLine(@"║     5 - Buscar por Período                                                                                     ║");
                Console.WriteLine(@"║                                                                                                               ║");
                Console.WriteLine(@"║  🔄 SINCRONIZAÇÃO:                                                                                             ║");
                Console.WriteLine(@"║     6 - Sincronizar com Firebase (atualizar dados)                                                            ║");
                Console.WriteLine(@"║                                                                                                               ║");
                Console.WriteLine(@"║  📊 ESTATÍSTICAS:                                                                                              ║");
                Console.WriteLine(@"║     7 - Estatísticas do Cache Local                                                                           ║");
                Console.WriteLine(@"║     8 - Estatísticas do Firebase (online)                                                                     ║");
                Console.WriteLine(@"║                                                                                                               ║");
                Console.WriteLine(@"║  📋 AUDITORIA:                                                                                                 ║");
                Console.WriteLine(@"║     9 - Ver Histórico de Alterações                                                                           ║");
                Console.WriteLine(@"║     10 - Ver Alterações por Usuário                                                                           ║");
                Console.WriteLine(@"║     11 - Ver Estatísticas de Auditoria                                                                        ║");
                Console.WriteLine(@"║                                                                                                               ║");
                Console.WriteLine(@"║  📜 LOGS DE REQUISIÇÕES:                                                                                       ║");
                Console.WriteLine(@"║     12 - Ver Todos os Logs de Requisição                                                                      ║");
                Console.WriteLine(@"║     13 - Ver Logs por Operação (GET/POST/PUT/DELETE/SYNC)                                                     ║");
                Console.WriteLine(@"║     14 - Ver Logs por Categoria (Leitura/Escrita/Sincronizacao)                                              ║");
                Console.WriteLine(@"║     15 - Ver Estatísticas de Requisições                                                                      ║");
                Console.WriteLine(@"║     16 - Ver Monitor de Requisições em Tempo Real                                                             ║");
                Console.WriteLine(@"║                                                                                                               ║");
                Console.WriteLine(@"║  🧹 MANUTENÇÃO:                                                                                                ║");
                Console.WriteLine(@"║     17 - Limpar Cache Antigo (30+ dias)                                                                       ║");
                Console.WriteLine(@"║                                                                                                               ║");
                Console.WriteLine(@"║     0 - Sair                                                                                                  ║");
                Console.WriteLine(@"║                                                                                                               ║");
                Console.WriteLine(@"╚═══════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝");

                Console.Write("\n👉 Escolha uma opção: ");
                var opcao = Console.ReadLine();

                switch (opcao)
                {
                    case "1":
                        await ListarTodasLocalizacoes();
                        break;
                    case "2":
                        await BuscarPorId();
                        break;
                    case "3":
                        await BuscarPorCep();
                        break;
                    case "4":
                        await BuscarPorBairro();
                        break;
                    case "5":
                        await BuscarPorPeriodo();
                        break;
                    case "6":
                        await RealizarSincronizacao();
                        break;
                    case "7":
                        await ExibirEstatisticasLocais();
                        break;
                    case "8":
                        await ExibirEstatisticasFirestore();
                        break;
                    case "9":
                        await VerHistoricoAlteracoes();
                        break;
                    case "10":
                        await VerAlteracoesPorUsuario();
                        break;
                    case "11":
                        await VerEstatisticasAuditoria();
                        break;
                    case "12":
                        await VerTodosLogsRequisicao();
                        break;
                    case "13":
                        await VerLogsPorOperacao();
                        break;
                    case "14":
                        await VerLogsPorCategoria();
                        break;
                    case "15":
                        await VerEstatisticasRequisicoes();
                        break;
                    case "16":
                        await VerMonitorRequisicoesTempoReal();
                        break;
                    case "17":
                        await LimparCacheAntigo();
                        break;
                    case "0":
                        _logger.LogInfo("Encerrando aplicação...", "VIEW");
                        return;
                    default:
                        _logger.LogWarning("Opção inválida!", "VIEW");
                        break;
                }

                Console.WriteLine("\nPressione qualquer tecla para continuar...");
                Console.ReadKey();
            }
        }

        private async Task RealizarSincronizacao()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         SINCRONIZANDO DADOS                                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            _logger.LogInfo("Buscando dados do Firebase Firestore...", "SYNC");

            var result = await _viewModel.SincronizarDados();

            if (result.Sucesso)
            {
                _dadosCarregados = true;
                Console.WriteLine($"\n📊 Resumo da sincronização:");
                Console.WriteLine($"   • Registros no Firestore: {result.OrigemCount}");
                Console.WriteLine($"   • Registros no Cache Local: {result.DestinoCount}");
                Console.WriteLine($"   • + Novos: {result.NovosRegistros}");
                Console.WriteLine($"   • ~ Atualizados: {result.RegistrosAtualizados}");
                Console.WriteLine($"   • - Removidos: {result.RegistrosRemovidos}");
            }
            else
            {
                _logger.LogError($"Falha na sincronização: {result.Mensagem}", null, "SYNC");
            }
        }

        private async Task ListarTodasLocalizacoes()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         TODAS AS LOCALIZAÇÕES                                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            if (!await ValidarDadosCarregados())
                return;

            var localizacoes = await _viewModel.ObterTodasLocalizacoes();

            if (localizacoes.Count == 0)
            {
                _logger.LogWarning("Nenhuma localização encontrada no cache local!", "VIEW");
                _logger.LogInfo("Execute a sincronização (opção 6) para carregar dados do Firebase.", "VIEW");
                return;
            }

            Console.WriteLine($"📌 Total de registros: {localizacoes.Count}\n");

            foreach (var loc in localizacoes)
            {
                ExibirDetalhesLocalizacao(loc);
            }
        }

        private async Task BuscarPorId()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         BUSCAR POR ID                                        ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            if (!await ValidarDadosCarregados())
                return;

            Console.Write("Digite o ID da localização: ");
            var id = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("ID inválido!", "VIEW");
                return;
            }

            var localizacao = await _viewModel.BuscarLocalizacaoPorId(id);

            if (localizacao == null)
            {
                _logger.LogWarning($"Localização com ID {id} não encontrada!", "VIEW");
                return;
            }

            ExibirDetalhesLocalizacao(localizacao);
        }

        private async Task BuscarPorCep()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         BUSCAR POR CEP                                       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            if (!await ValidarDadosCarregados())
                return;

            Console.Write("Digite o CEP (8 dígitos): ");
            var cep = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(cep))
            {
                _logger.LogWarning("CEP inválido!", "VIEW");
                return;
            }

            var localizacoes = await _viewModel.BuscarLocalizacoesPorCep(cep);

            if (!localizacoes.Any())
            {
                _logger.LogWarning($"Nenhuma localização encontrada para o CEP {cep}!", "VIEW");
                return;
            }

            Console.WriteLine($"\n📌 Encontradas {localizacoes.Count} localização(ões) para o CEP {cep}:\n");

            foreach (var loc in localizacoes)
            {
                ExibirDetalhesLocalizacao(loc);
            }
        }

        private async Task BuscarPorBairro()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         BUSCAR POR BAIRRO                                    ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            if (!await ValidarDadosCarregados())
                return;

            Console.Write("Digite o nome do bairro: ");
            var bairro = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(bairro))
            {
                _logger.LogWarning("Bairro inválido!", "VIEW");
                return;
            }

            var localizacoes = await _viewModel.BuscarLocalizacoesPorBairro(bairro);

            if (!localizacoes.Any())
            {
                _logger.LogWarning($"Nenhuma localização encontrada para o bairro {bairro}!", "VIEW");
                return;
            }

            Console.WriteLine($"\n📌 Encontradas {localizacoes.Count} localização(ões) no bairro {bairro}:\n");

            foreach (var loc in localizacoes)
            {
                ExibirDetalhesLocalizacao(loc);
            }
        }

        private async Task BuscarPorPeriodo()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         BUSCAR POR PERÍODO                                   ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            if (!await ValidarDadosCarregados())
                return;

            Console.Write("Data inicial (yyyy-mm-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime dataInicio))
            {
                _logger.LogWarning("Data inicial inválida!", "VIEW");
                return;
            }

            Console.Write("Data final (yyyy-mm-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime dataFim))
            {
                _logger.LogWarning("Data final inválida!", "VIEW");
                return;
            }

            if (dataInicio > dataFim)
            {
                _logger.LogWarning("Data inicial não pode ser maior que data final!", "VIEW");
                return;
            }

            var localizacoes = await _viewModel.BuscarLocalizacoesPorPeriodo(dataInicio, dataFim.AddDays(1).AddSeconds(-1));

            if (!localizacoes.Any())
            {
                _logger.LogWarning($"Nenhuma localização encontrada no período de {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}!", "VIEW");
                return;
            }

            Console.WriteLine($"\n📌 Encontradas {localizacoes.Count} localização(ões) no período:\n");

            foreach (var loc in localizacoes)
            {
                ExibirDetalhesLocalizacao(loc);
            }
        }

        private async Task ExibirEstatisticasLocais()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    ESTATÍSTICAS DO CACHE LOCAL                               ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            var stats = await _viewModel.ObterEstatisticasLocais();

            Console.WriteLine($"📊 Dados armazenados localmente (SQLite):\n");
            Console.WriteLine($"   • Total de registros: {stats.TotalRegistros:N0}");
            Console.WriteLine($"   • Última sincronização: {(stats.UltimaSincronizacao?.ToString("dd/MM/yyyy HH:mm:ss") ?? "Nunca")}");
            Console.WriteLine($"   • Registro mais antigo: {(stats.RegistroMaisAntigo > DateTime.MinValue ? stats.RegistroMaisAntigo.ToString("dd/MM/yyyy HH:mm:ss") : "Nenhum")}");
            Console.WriteLine($"   • Registro mais recente: {(stats.RegistroMaisRecente > DateTime.MinValue ? stats.RegistroMaisRecente.ToString("dd/MM/yyyy HH:mm:ss") : "Nenhum")}");
            Console.WriteLine($"   • Bairros únicos: {stats.BairrosUnicos}");
            Console.WriteLine($"   • CEPs únicos: {stats.CepsUnicos}");
        }

        private async Task ExibirEstatisticasFirestore()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   ESTATÍSTICAS DO FIRESTORE (ONLINE)                         ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            _logger.LogInfo("Consultando Firebase Firestore...", "VIEW");

            var stats = await _viewModel.ObterEstatisticasFirestore();

            Console.WriteLine($"📊 Dados armazenados no Firebase Firestore:\n");
            Console.WriteLine($"   • Total de registros: {stats.TotalRegistros:N0}");
            Console.WriteLine($"   • Última atualização: {(stats.UltimaAtualizacao > DateTime.MinValue ? stats.UltimaAtualizacao.ToString("dd/MM/yyyy HH:mm:ss") : "Nenhum")}");
            Console.WriteLine($"   • Bairros únicos: {stats.BairrosUnicos}");
            Console.WriteLine($"   • CEPs únicos: {stats.CepsUnicos}");
        }

        private async Task LimparCacheAntigo()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      LIMPAR CACHE ANTIGO                                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            Console.Write("Remover registros com mais de quantos dias? (padrão 30): ");
            if (!int.TryParse(Console.ReadLine(), out int dias) || dias <= 0)
                dias = 30;

            Console.WriteLine($"\n⚠ ATENÇÃO: Isso removerá registros com mais de {dias} dias do cache local.");
            Console.Write("Confirma? (S/N): ");

            if (Console.ReadLine()?.ToUpper() != "S")
            {
                _logger.LogInfo("Operação cancelada pelo usuário.", "VIEW");
                return;
            }

            var removidos = await _viewModel.LimparCacheAntigo(dias);

            if (removidos > 0)
            {
                _logger.LogSuccess($"Removidos {removidos} registros antigos do cache local!", "VIEW");
            }
            else
            {
                _logger.LogInfo($"Nenhum registro com mais de {dias} dias encontrado no cache.", "VIEW");
            }
        }

        // ==================== MÉTODOS DE AUDITORIA ====================

        private async Task VerHistoricoAlteracoes()
        {
            var alteracoes = await _viewModel.ObterTodasAlteracoes();

            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         HISTÓRICO DE ALTERAÇÕES                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            if (!alteracoes.Any())
            {
                _logger.LogWarning("Nenhuma alteração registrada ainda.");
                return;
            }

            foreach (var audit in alteracoes.Take(50))
            {
                var cor = audit.Acao switch
                {
                    "INSERT" => ConsoleColor.Green,
                    "UPDATE" => ConsoleColor.Cyan,
                    "DELETE" => ConsoleColor.Red,
                    _ => ConsoleColor.White
                };

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"┌────────────────────────────────────────────────────────────────────────────────────────────┐");
                Console.WriteLine($"│ 📅 {audit.DataHora:dd/MM/yyyy HH:mm:ss}                                                    │");
                Console.ForegroundColor = cor;
                Console.WriteLine($"│ 🏷️  {audit.Acao,-81} │");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"│ 📋 Tabela: {audit.Tabela,-75} │");
                Console.WriteLine($"│ 🔑 ID: {audit.RegistroId,-79} │");
                Console.WriteLine($"│ 👤 Usuário: {audit.Usuario} ({audit.EmailUsuario})                                         │");
                Console.WriteLine($"│ 💻 Máquina: {audit.Maquina,-74} │");
                Console.WriteLine($"│ 📡 Origem: {audit.Origem,-76} │");
                if (!string.IsNullOrEmpty(audit.Detalhes))
                {
                    var detalhes = audit.Detalhes.Length > 70 ? audit.Detalhes.Substring(0, 67) + "..." : audit.Detalhes;
                    Console.WriteLine($"│ 📝 Detalhes: {detalhes,-74} │");
                }
                Console.WriteLine($"└────────────────────────────────────────────────────────────────────────────────────────────┘\n");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Total de registros: {alteracoes.Count}");
        }

        private async Task VerAlteracoesPorUsuario()
        {
            Console.Write("Digite o nome do usuário: ");
            var usuario = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(usuario)) return;

            var alteracoes = await _viewModel.BuscarAlteracoesPorUsuario(usuario);

            Console.Clear();
            Console.WriteLine($"╔══════════════════════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║              ALTERAÇÕES DO USUÁRIO: {usuario,-60}║");
            Console.WriteLine($"╚══════════════════════════════════════════════════════════════════════════════════════════════════╝\n");

            if (!alteracoes.Any())
            {
                _logger.LogWarning($"Nenhuma alteração encontrada para o usuário {usuario}");
                return;
            }

            foreach (var audit in alteracoes.Take(50))
            {
                var icon = audit.Acao switch
                {
                    "INSERT" => "➕",
                    "UPDATE" => "🔄",
                    "DELETE" => "🗑️",
                    _ => "📝"
                };
                Console.WriteLine($"{icon} [{audit.DataHora:dd/MM/yyyy HH:mm:ss}] {audit.Acao} - {audit.Tabela} ID:{audit.RegistroId}");
                Console.WriteLine($"   {audit.Detalhes}\n");
            }

            Console.WriteLine($"\nTotal: {alteracoes.Count} alterações");
        }

        private async Task VerEstatisticasAuditoria()
        {
            var stats = await _viewModel.ObterEstatisticasAuditoria();

            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    ESTATÍSTICAS DE AUDITORIA                                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine($"📊 Total de registros de auditoria: {stats.TotalRegistros}");
            Console.WriteLine($"✅ Total de INSERÇÕES: {stats.TotalInserts}");
            Console.WriteLine($"🔄 Total de ATUALIZAÇÕES: {stats.TotalUpdates}");
            Console.WriteLine($"🗑️ Total de EXCLUSÕES: {stats.TotalDeletes}");
            Console.WriteLine($"👥 Usuários que realizaram alterações: {stats.UsuariosAtivos}");
            Console.WriteLine($"📅 Última alteração: {stats.UltimaAlteracao:dd/MM/yyyy HH:mm:ss}");
        }

        // ==================== MÉTODOS DE LOGS DE REQUISIÇÕES ====================

        private async Task VerTodosLogsRequisicao()
        {
            var logs = await _viewModel.ObterTodosLogsRequisicao();

            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         LOGS DE REQUISIÇÕES                                   ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            if (!logs.Any())
            {
                _logger.LogWarning("Nenhum log de requisição registrado ainda.");
                return;
            }

            foreach (var log in logs.Take(50))
            {
                var icon = log.Sucesso ? "✅" : "❌";
                var cor = log.Sucesso ? ConsoleColor.Green : ConsoleColor.Red;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"┌────────────────────────────────────────────────────────────────────────────────────────────┐");
                Console.WriteLine($"│ 📅 {log.DataHora:dd/MM/yyyy HH:mm:ss}                                                    │");
                Console.ForegroundColor = cor;
                Console.WriteLine($"│ {icon} {log.Operacao,-81} │");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"│ 📍 Endpoint: {log.Endpoint,-74} │");
                Console.WriteLine($"│ ⏱️  Duração: {log.DuracaoMs}ms ({log.DuracaoMs / 1000.0:F2}s)                               │");
                Console.WriteLine($"│ 📡 Origem: {log.Origem,-76} │");
                Console.WriteLine($"│ 📂 Categoria: {log.Categoria,-73} │");
                Console.WriteLine($"│ 👤 Usuário: {log.Usuario,-76} │");
                if (log.StatusCode > 0)
                    Console.WriteLine($"│ 🔢 Status Code: {log.StatusCode,-71} │");
                if (!string.IsNullOrEmpty(log.Parametros))
                    Console.WriteLine($"│ 📋 Parâmetros: {log.Parametros,-72} │");
                if (!string.IsNullOrEmpty(log.MensagemErro))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    var erro = log.MensagemErro.Length > 70 ? log.MensagemErro.Substring(0, 67) + "..." : log.MensagemErro;
                    Console.WriteLine($"│ ❌ Erro: {erro,-76} │");
                }
                Console.WriteLine($"└────────────────────────────────────────────────────────────────────────────────────────────┘\n");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Total de logs: {logs.Count}");
            Console.WriteLine($"✅ Sucessos: {logs.Count(l => l.Sucesso)}");
            Console.WriteLine($"❌ Erros: {logs.Count(l => !l.Sucesso)}");
        }

        private async Task VerLogsPorOperacao()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    LOGS DE REQUISIÇÕES POR OPERAÇÃO                          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine("Operações disponíveis: GET, POST, PUT, PATCH, DELETE, SYNC");
            Console.Write("\nDigite a operação: ");
            var operacao = Console.ReadLine()?.ToUpper();

            if (string.IsNullOrWhiteSpace(operacao)) return;

            var logs = await _viewModel.ObterLogsPorOperacao(operacao);

            Console.Clear();
            Console.WriteLine($"╔══════════════════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║              LOGS DE REQUISIÇÕES - OPERAÇÃO: {operacao,-60}║");
            Console.WriteLine($"╚══════════════════════════════════════════════════════════════════════════════════════════════╝\n");

            if (!logs.Any())
            {
                _logger.LogWarning($"Nenhum log encontrado para operação {operacao}");
                return;
            }

            var stats = logs.GroupBy(l => l.Endpoint)
                .Select(g => new { Endpoint = g.Key, Count = g.Count(), AvgMs = g.Average(l => l.DuracaoMs) })
                .OrderByDescending(g => g.Count)
                .ToList();

            Console.WriteLine("📊 Estatísticas por endpoint:");
            foreach (var s in stats.Take(10))
            {
                Console.WriteLine($"   • {s.Endpoint}: {s.Count} req, média {s.AvgMs:F0}ms");
            }

            Console.WriteLine("\n📋 Últimas requisições:\n");
            foreach (var log in logs.Take(30))
            {
                var icon = log.Sucesso ? "✅" : "❌";
                Console.WriteLine($"{icon} [{log.DataHora:HH:mm:ss}] {log.Endpoint} - {log.DuracaoMs}ms - {log.Usuario}");
                if (!string.IsNullOrEmpty(log.MensagemErro))
                    Console.WriteLine($"   └─ Erro: {log.MensagemErro}");
            }

            Console.WriteLine($"\n📈 Total: {logs.Count} requisições");
            Console.WriteLine($"✅ Sucesso: {logs.Count(l => l.Sucesso)}");
            Console.WriteLine($"❌ Erro: {logs.Count(l => !l.Sucesso)}");
            Console.WriteLine($"⏱️  Tempo médio: {logs.Average(l => l.DuracaoMs):F0}ms");
        }

        private async Task VerLogsPorCategoria()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    LOGS DE REQUISIÇÕES POR CATEGORIA                         ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine("Categorias disponíveis: Leitura, Escrita, Sincronizacao");
            Console.Write("\nDigite a categoria: ");
            var categoria = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(categoria)) return;

            var logs = await _viewModel.ObterLogsPorCategoria(categoria);

            Console.Clear();
            Console.WriteLine($"╔══════════════════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║              LOGS DE REQUISIÇÕES - CATEGORIA: {categoria,-60}║");
            Console.WriteLine($"╚══════════════════════════════════════════════════════════════════════════════════════════════╝\n");

            if (!logs.Any())
            {
                _logger.LogWarning($"Nenhum log encontrado para categoria {categoria}");
                return;
            }

            var statsPorOperacao = logs.GroupBy(l => l.Operacao)
                .Select(g => $"{g.Key}: {g.Count()}")
                .ToList();

            var statsPorOrigem = logs.GroupBy(l => l.Origem)
                .Select(g => $"{g.Key}: {g.Count()}")
                .ToList();

            Console.WriteLine($"📊 Distribuição por operação: {string.Join(" | ", statsPorOperacao)}");
            Console.WriteLine($"📡 Distribuição por origem: {string.Join(" | ", statsPorOrigem)}");
            Console.WriteLine($"⏱️  Tempo médio: {logs.Average(l => l.DuracaoMs):F0}ms");
            Console.WriteLine($"✅ Taxa de sucesso: {(double)logs.Count(l => l.Sucesso) / logs.Count * 100:F1}%\n");

            Console.WriteLine("📋 Últimas requisições:\n");
            foreach (var log in logs.Take(30))
            {
                var icon = log.Sucesso ? "✅" : "❌";
                Console.WriteLine($"{icon} [{log.DataHora:HH:mm:ss}] {log.Operacao} - {log.Endpoint} - {log.DuracaoMs}ms");
            }

            Console.WriteLine($"\n📈 Total: {logs.Count} requisições");
        }

        private async Task VerEstatisticasRequisicoes()
        {
            var stats = await _viewModel.ObterEstatisticasRequisicoes();

            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    ESTATÍSTICAS DE REQUISIÇÕES                                ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine($"📊 Total de requisições: {stats.TotalRequisicoes}");
            Console.WriteLine($"✅ Requisições bem-sucedidas: {stats.TotalSucesso}");
            Console.WriteLine($"❌ Requisições com erro: {stats.TotalErros}");
            Console.WriteLine($"⏱️  Tempo médio de resposta: {stats.MediaDuracaoMs:F0}ms");
            Console.WriteLine($"📅 Última requisição: {stats.UltimaRequisicao:dd/MM/yyyy HH:mm:ss}");

            Console.WriteLine("\n📈 Por operação:");
            foreach (var op in stats.PorOperacao)
            {
                Console.WriteLine($"   • {op.Key}: {op.Value} requisições");
            }

            Console.WriteLine("\n📡 Por origem:");
            foreach (var org in stats.PorOrigem)
            {
                Console.WriteLine($"   • {org.Key}: {org.Value} requisições");
            }

            // Calcular taxa de erro por operação
            Console.WriteLine("\n⚠️ Taxa de erro por operação:");
            var logsPorOperacao = await _viewModel.ObterTodosLogsRequisicao();
            var errorsPorOperacao = logsPorOperacao
                .Where(l => !l.Sucesso)
                .GroupBy(l => l.Operacao)
                .Select(g => new { Operacao = g.Key, Erros = g.Count() });

            foreach (var err in errorsPorOperacao)
            {
                var total = stats.PorOperacao.GetValueOrDefault(err.Operacao, 0);
                var taxa = total > 0 ? (double)err.Erros / total * 100 : 0;
                Console.WriteLine($"   • {err.Operacao}: {err.Erros}/{total} ({taxa:F1}%)");
            }
        }

        private async Task VerMonitorRequisicoesTempoReal()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              MONITOR DE REQUISIÇÕES EM TEMPO REAL                             ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine("🟢 Monitor ativo - Pressione 'Q' para sair\n");
            Console.WriteLine(new string('═', 100));

            var ultimosLogs = await _viewModel.ObterTodosLogsRequisicao();
            var ultimosIds = ultimosLogs.Select(l => l.Id).ToHashSet();

            var tokenSource = new CancellationTokenSource();
            var task = Task.Run(async () =>
            {
                while (!tokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var novosLogs = await _viewModel.ObterLogsRequisicaoRecentes(ultimosIds);

                        foreach (var log in novosLogs)
                        {
                            var corOriginal = Console.ForegroundColor;

                            if (log.Sucesso)
                                Console.ForegroundColor = ConsoleColor.Green;
                            else
                                Console.ForegroundColor = ConsoleColor.Red;

                            var icon = log.Sucesso ? "✅" : "❌";
                            var categoriaIcon = log.Categoria switch
                            {
                                "Leitura" => "📖",
                                "Escrita" => "✍️",
                                "Sincronizacao" => "🔄",
                                _ => "📡"
                            };

                            Console.WriteLine($"{icon} {categoriaIcon} [{log.DataHora:HH:mm:ss}] {log.Operacao} {log.Endpoint}");
                            Console.WriteLine($"   ⏱️  {log.DuracaoMs}ms | 👤 {log.Usuario} | 📡 {log.Origem}");

                            if (!string.IsNullOrEmpty(log.MensagemErro))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"   ❌ Erro: {log.MensagemErro}");
                            }

                            Console.ForegroundColor = corOriginal;
                            Console.WriteLine(new string('─', 100));

                            ultimosIds.Add(log.Id);
                        }

                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        // Ignorar erros durante monitoramento
                    }
                }
            });

            while (Console.ReadKey(true).Key != ConsoleKey.Q) { }

            tokenSource.Cancel();
            await task;

            Console.Clear();
            _logger.LogInfo("Monitoramento encerrado.", "VIEW");
        }

        // ==================== MÉTODOS AUXILIARES ====================

        private async Task<bool> ValidarDadosCarregados()
        {
            var dadosExistem = await _viewModel.HaDadosLocais();

            if (!dadosExistem)
            {
                _logger.LogWarning("Nenhum dado disponível no cache local!", "VIEW");
                _logger.LogInfo("Por favor, sincronize com o Firebase primeiro (opção 6).", "VIEW");
                return false;
            }

            return true;
        }

        private void ExibirDetalhesLocalizacao(Localizacao loc)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"┌────────────────────────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine($"│ ID: {loc.Id,-81} │");
            Console.WriteLine($"├────────────────────────────────────────────────────────────────────────────────────────────────┤");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"│ 📍 Logradouro: {loc.Logradouro,-75} │");
            Console.WriteLine($"│ 🔢 Número: {loc.Numero,-79} │");
            Console.WriteLine($"│ 🏘️ Bairro: {loc.Bairro,-79} │");
            Console.WriteLine($"│ 📮 CEP: {loc.Cep,-82} │");
            Console.WriteLine($"│ 🌐 Latitude: {loc.Latitude,-75:F6} │");
            Console.WriteLine($"│ 🌐 Longitude: {loc.Longitude,-74:F6} │");
            Console.WriteLine($"│ 🕐 Timestamp: {loc.Timestamp:dd/MM/yyyy HH:mm:ss,-74} │");
            if (loc.LastSyncAt.HasValue)
            {
                Console.WriteLine($"│ 🔄 Última sync: {loc.LastSyncAt:dd/MM/yyyy HH:mm:ss,-72} │");
            }
            Console.WriteLine($"└────────────────────────────────────────────────────────────────────────────────────────────────┘\n");
            Console.ResetColor();
        }
    }
}