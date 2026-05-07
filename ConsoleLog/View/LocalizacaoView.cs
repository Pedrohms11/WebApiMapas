using ConsoleLog.Models;
using ConsoleLog.Services;
using ConsoleLog.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleLog.View
{
    /// <summary>
    /// View do padrão MVVM - APENAS EXIBIÇÃO DE INFORMAÇÕES
    /// </summary>
    public class LocalizacaoView
    {
        private readonly LocalizacaoViewModel _viewModel;
        private readonly LogService _logger;
        private bool _dadosCarregados = false;

        public LocalizacaoView(LocalizacaoViewModel viewModel, LogService logger)
        {
            _viewModel = viewModel;
            _logger = logger;

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
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                    SISTEMA DE CONSULTA DE LOCALIZAÇÕES                       ║");
                Console.WriteLine("║                              READ-ONLY MODE                                  ║");
                Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════╣");
                Console.WriteLine("║                                                                              ║");
                Console.WriteLine("║  📍 OPERAÇÕES DE CONSULTA:                                                    ║");
                Console.WriteLine("║  1 - Listar Todas Localizações                                                ║");
                Console.WriteLine("║  2 - Buscar por ID                                                           ║");
                Console.WriteLine("║  3 - Buscar por CEP                                                          ║");
                Console.WriteLine("║  4 - Buscar por Bairro                                                       ║");
                Console.WriteLine("║  5 - Buscar por Período                                                      ║");
                Console.WriteLine("║                                                                              ║");
                Console.WriteLine("║  🔄 SINCRONIZAÇÃO:                                                           ║");
                Console.WriteLine("║  6 - Sincronizar com Firebase (atualizar dados)                              ║");
                Console.WriteLine("║                                                                              ║");
                Console.WriteLine("║  📊 ESTATÍSTICAS:                                                            ║");
                Console.WriteLine("║  7 - Estatísticas do Cache Local                                             ║");
                Console.WriteLine("║  8 - Estatísticas do Firebase (online)                                       ║");
                Console.WriteLine("║                                                                              ║");
                Console.WriteLine("║  🧹 MANUTENÇÃO:                                                              ║");
                Console.WriteLine("║  9 - Limpar Cache Antigo (30+ dias)                                          ║");
                Console.WriteLine("║                                                                              ║");
                Console.WriteLine("║  0 - Sair                                                                    ║");
                Console.WriteLine("║                                                                              ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");

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
            if (!int.TryParse(Console.ReadLine(), out int id))
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
            Console.WriteLine($"┌────────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine($"│ ID: {loc.Id,-70} │");
            Console.WriteLine($"├────────────────────────────────────────────────────────────────────────────────┤");
            Console.WriteLine($"│ 📍 Logradouro: {loc.Logradouro,-64} │");
            Console.WriteLine($"│ 🔢 Número: {loc.Numero,-68} │");
            Console.WriteLine($"│ 🏘️ Bairro: {loc.Bairro,-68} │");
            Console.WriteLine($"│ 📮 CEP: {loc.Cep,-71} │");
            Console.WriteLine($"│ 🌐 Latitude: {loc.Latitude,-64:F6} │");
            Console.WriteLine($"│ 🌐 Longitude: {loc.Longitude,-63:F6} │");
            Console.WriteLine($"│ 🕐 Timestamp: {loc.Timestamp:dd/MM/yyyy HH:mm:ss,-63} │");
            if (loc.LastSyncAt.HasValue)
            {
                Console.WriteLine($"│ 🔄 Última sync: {loc.LastSyncAt:dd/MM/yyyy HH:mm:ss,-61} │");
            }
            Console.WriteLine($"└────────────────────────────────────────────────────────────────────────────────┘\n");
        }
    }
}
