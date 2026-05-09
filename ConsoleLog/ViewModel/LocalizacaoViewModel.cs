using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ConsoleLog.Models;
using ConsoleLog.Data;
using ConsoleLog.Services;
using ConsoleLog.Services.Sync;

namespace ConsoleLog.ViewModels
{
    /// <summary>
    /// ViewModel para operações de Localização - APENAS LEITURA COM AUDITORIA
    /// </summary>
    public class LocalizacaoViewModel
    {
        private readonly AppDbContext _context;
        private readonly LogService _logger;
        private readonly FirestoreService _firestoreService;
        private readonly DataSyncService _syncService;
        private readonly AuditoriaService _auditoriaService;
        private readonly RequisicaoLoggerService _requisicaoLogger;

        /// <summary>
        /// Evento notificado quando há mudança no estado
        /// </summary>
        public event EventHandler<string>? OnOperationCompleted;

        /// <summary>
        /// Evento notificado quando ocorre um erro
        /// </summary>
        public event EventHandler<Exception>? OnErrorOccurred;

        /// <summary>
        /// Evento notificado durante a sincronização
        /// </summary>
        public event EventHandler<SyncResult>? OnSyncCompleted;

        public LocalizacaoViewModel(
            AppDbContext context,
            LogService logger,
            FirestoreService firestoreService,
            DataSyncService syncService,
            AuditoriaService auditoriaService,
            RequisicaoLoggerService requisicaoLogger)
        {
            _context = context;
            _logger = logger;
            _firestoreService = firestoreService;
            _syncService = syncService;
            _auditoriaService = auditoriaService;
            _requisicaoLogger = requisicaoLogger;
        }

        // ==================== OPERAÇÕES DE SINCRONIZAÇÃO ====================

        /// <summary>
        /// Sincroniza dados do Firestore para o SQLite local
        /// </summary>
        public async Task<SyncResult> SincronizarDados()
        {
            try
            {
                _logger.LogInfo("Iniciando sincronização de dados", "VIEWMODEL");
                var result = await _syncService.SincronizarFirestoreParaLocal();

                OnSyncCompleted?.Invoke(this, result);
                OnOperationCompleted?.Invoke(this, result.Mensagem);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro na sincronização de dados", ex, "VIEWMODEL");
                OnErrorOccurred?.Invoke(this, ex);
                return new SyncResult { Sucesso = false, Mensagem = ex.Message };
            }
        }

        // ==================== OPERAÇÕES DE LEITURA ====================

        /// <summary>
        /// Obtém todas as localizações do SQLite local
        /// </summary>
        public async Task<List<Localizacao>> ObterTodasLocalizacoes()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInfo("Buscando localizações do SQLite local", "VIEWMODEL");
                var resultado = await _context.Localizacoes
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();

                stopwatch.Stop();

                await _requisicaoLogger.LogLeitura(
                    endpoint: "api/localizacoes",
                    parametros: "",
                    resultado: $"{resultado.Count} registros",
                    duracaoMs: stopwatch.ElapsedMilliseconds,
                    sucesso: true
                );

                return resultado;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                await _requisicaoLogger.LogLeitura(
                    endpoint: "api/localizacoes",
                    parametros: "",
                    resultado: "",
                    duracaoMs: stopwatch.ElapsedMilliseconds,
                    sucesso: false,
                    erro: ex.Message
                );

                _logger.LogError("Erro ao buscar localizações", ex, "VIEWMODEL");
                OnErrorOccurred?.Invoke(this, ex);
                return new List<Localizacao>();
            }
        }

        /// <summary>
        /// Busca localização por ID
        /// </summary>
        public async Task<Localizacao?> BuscarLocalizacaoPorId(string id)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var resultado = await _context.Localizacoes.FindAsync(id);

                stopwatch.Stop();

                await _requisicaoLogger.LogLeitura(
                    endpoint: $"api/localizacoes/{id}",
                    parametros: $"id={id}",
                    resultado: resultado != null ? "1 registro" : "0 registros",
                    duracaoMs: stopwatch.ElapsedMilliseconds,
                    sucesso: true
                );

                return resultado;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                await _requisicaoLogger.LogLeitura(
                    endpoint: $"api/localizacoes/{id}",
                    parametros: $"id={id}",
                    resultado: "",
                    duracaoMs: stopwatch.ElapsedMilliseconds,
                    sucesso: false,
                    erro: ex.Message
                );

                _logger.LogError($"Erro ao buscar localização {id}", ex, "VIEWMODEL");
                OnErrorOccurred?.Invoke(this, ex);
                return null;
            }
        }

        /// <summary>
        /// Busca localizações por CEP
        /// </summary>
        public async Task<List<Localizacao>> BuscarLocalizacoesPorCep(string cep)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var resultado = await _context.Localizacoes
                    .Where(l => l.Cep == cep)
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();

                stopwatch.Stop();

                await _requisicaoLogger.LogLeitura(
                    endpoint: "api/localizacoes/cep",
                    parametros: $"cep={cep}",
                    resultado: $"{resultado.Count} registros",
                    duracaoMs: stopwatch.ElapsedMilliseconds,
                    sucesso: true
                );

                return resultado;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                await _requisicaoLogger.LogLeitura(
                    endpoint: "api/localizacoes/cep",
                    parametros: $"cep={cep}",
                    resultado: "",
                    duracaoMs: stopwatch.ElapsedMilliseconds,
                    sucesso: false,
                    erro: ex.Message
                );

                _logger.LogError($"Erro ao buscar localizações por CEP {cep}", ex, "VIEWMODEL");
                OnErrorOccurred?.Invoke(this, ex);
                return new List<Localizacao>();
            }
        }

        /// <summary>
        /// Busca localizações por bairro
        /// </summary>
        public async Task<List<Localizacao>> BuscarLocalizacoesPorBairro(string bairro)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var resultado = await _context.Localizacoes
                    .Where(l => l.Bairro.Contains(bairro))
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();

                stopwatch.Stop();

                await _requisicaoLogger.LogLeitura(
                    endpoint: "api/localizacoes/bairro",
                    parametros: $"bairro={bairro}",
                    resultado: $"{resultado.Count} registros",
                    duracaoMs: stopwatch.ElapsedMilliseconds,
                    sucesso: true
                );

                return resultado;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                await _requisicaoLogger.LogLeitura(
                    endpoint: "api/localizacoes/bairro",
                    parametros: $"bairro={bairro}",
                    resultado: "",
                    duracaoMs: stopwatch.ElapsedMilliseconds,
                    sucesso: false,
                    erro: ex.Message
                );

                _logger.LogError($"Erro ao buscar localizações por bairro {bairro}", ex, "VIEWMODEL");
                OnErrorOccurred?.Invoke(this, ex);
                return new List<Localizacao>();
            }
        }

        /// <summary>
        /// Busca localizações por período
        /// </summary>
        public async Task<List<Localizacao>> BuscarLocalizacoesPorPeriodo(DateTime inicio, DateTime fim)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var resultado = await _context.Localizacoes
                    .Where(l => l.Timestamp >= inicio && l.Timestamp <= fim)
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();

                stopwatch.Stop();

                await _requisicaoLogger.LogLeitura(
                    endpoint: "api/localizacoes/periodo",
                    parametros: $"inicio={inicio:yyyy-MM-dd}&fim={fim:yyyy-MM-dd}",
                    resultado: $"{resultado.Count} registros",
                    duracaoMs: stopwatch.ElapsedMilliseconds,
                    sucesso: true
                );

                return resultado;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                await _requisicaoLogger.LogLeitura(
                    endpoint: "api/localizacoes/periodo",
                    parametros: $"inicio={inicio:yyyy-MM-dd}&fim={fim:yyyy-MM-dd}",
                    resultado: "",
                    duracaoMs: stopwatch.ElapsedMilliseconds,
                    sucesso: false,
                    erro: ex.Message
                );

                _logger.LogError($"Erro ao buscar localizações por período", ex, "VIEWMODEL");
                OnErrorOccurred?.Invoke(this, ex);
                return new List<Localizacao>();
            }
        }

        // ==================== ESTATÍSTICAS ====================

        /// <summary>
        /// Obtém estatísticas dos dados locais
        /// </summary>
        public async Task<LocalStats> ObterEstatisticasLocais()
        {
            try
            {
                var todos = await _context.Localizacoes.ToListAsync();

                return new LocalStats
                {
                    TotalRegistros = todos.Count,
                    UltimaSincronizacao = todos.Any() ? todos.Max(l => l.LastSyncAt) : null,
                    RegistroMaisAntigo = todos.Any() ? todos.Min(l => l.Timestamp) : DateTime.MinValue,
                    RegistroMaisRecente = todos.Any() ? todos.Max(l => l.Timestamp) : DateTime.MinValue,
                    BairrosUnicos = todos.Select(l => l.Bairro).Distinct().Count(),
                    CepsUnicos = todos.Select(l => l.Cep).Distinct().Count()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao obter estatísticas locais", ex, "VIEWMODEL");
                OnErrorOccurred?.Invoke(this, ex);
                return new LocalStats();
            }
        }

        /// <summary>
        /// Obtém estatísticas do Firestore (online)
        /// </summary>
        public async Task<FirestoreStats> ObterEstatisticasFirestore()
        {
            try
            {
                return await _firestoreService.ObterEstatisticas();
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao obter estatísticas do Firestore", ex, "VIEWMODEL");
                OnErrorOccurred?.Invoke(this, ex);
                return new FirestoreStats();
            }
        }

        /// <summary>
        /// Verifica se há dados disponíveis localmente
        /// </summary>
        public async Task<bool> HaDadosLocais()
        {
            return await _context.Localizacoes.AnyAsync();
        }

        /// <summary>
        /// Limpa dados antigos do cache local
        /// </summary>
        public async Task<int> LimparCacheAntigo(int diasManter = 30)
        {
            try
            {
                return await _syncService.LimparDadosAntigos(diasManter);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao limpar cache antigo", ex, "VIEWMODEL");
                OnErrorOccurred?.Invoke(this, ex);
                return 0;
            }
        }

        // ==================== MÉTODOS DE AUDITORIA ====================

        /// <summary>
        /// Obtém todas as alterações registradas
        /// </summary>
        public async Task<List<Auditoria>> ObterTodasAlteracoes()
        {
            return await _auditoriaService.ObterTodasAlteracoes();
        }

        /// <summary>
        /// Busca alterações por usuário
        /// </summary>
        public async Task<List<Auditoria>> BuscarAlteracoesPorUsuario(string usuario)
        {
            return await _auditoriaService.BuscarAlteracoesPorUsuario(usuario);
        }

        /// <summary>
        /// Obtém estatísticas de auditoria
        /// </summary>
        public async Task<AuditoriaStats> ObterEstatisticasAuditoria()
        {
            return await _auditoriaService.ObterEstatisticas();
        }

        // ==================== MÉTODOS DE LOGS DE REQUISIÇÕES ====================

        /// <summary>
        /// Obtém todos os logs de requisição
        /// </summary>
        public async Task<List<LogRequisicao>> ObterTodosLogsRequisicao()
        {
            return await _requisicaoLogger.ObterTodosLogs();
        }

        /// <summary>
        /// Obtém logs por operação
        /// </summary>
        public async Task<List<LogRequisicao>> ObterLogsPorOperacao(string operacao)
        {
            return await _requisicaoLogger.ObterLogsPorOperacao(operacao);
        }

        /// <summary>
        /// Obtém logs por categoria
        /// </summary>
        public async Task<List<LogRequisicao>> ObterLogsPorCategoria(string categoria)
        {
            return await _requisicaoLogger.ObterLogsPorCategoria(categoria);
        }

        /// <summary>
        /// Obtém estatísticas de requisições
        /// </summary>
        public async Task<RequisicaoStats> ObterEstatisticasRequisicoes()
        {
            return await _requisicaoLogger.ObterEstatisticas();
        }

        /// <summary>
        /// Obtém logs recentes (para monitoramento em tempo real)
        /// </summary>
        public async Task<List<LogRequisicao>> ObterLogsRequisicaoRecentes(HashSet<int> idsExistentes)
        {
            var todos = await _requisicaoLogger.ObterTodosLogs();
            return todos.Where(l => !idsExistentes.Contains(l.Id)).ToList();
        }
    }

    /// <summary>
    /// Estatísticas dos dados locais
    /// </summary>
    public class LocalStats
    {
        public int TotalRegistros { get; set; }
        public DateTime? UltimaSincronizacao { get; set; }
        public DateTime RegistroMaisAntigo { get; set; }
        public DateTime RegistroMaisRecente { get; set; }
        public int BairrosUnicos { get; set; }
        public int CepsUnicos { get; set; }
    }
}