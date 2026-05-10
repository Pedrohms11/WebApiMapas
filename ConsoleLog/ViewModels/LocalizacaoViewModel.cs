using Microsoft.EntityFrameworkCore;
using ConsoleLog.Models;
using ConsoleLog.Data;
using ConsoleLog.Services;
using ConsoleLog.Services.Sync;

namespace ConsoleLog.ViewModels
{
    public class LocalizacaoViewModel
    {
        private readonly AppDbContext _context;
        private readonly LogService _logger;
        private readonly FirestoreService _firestoreService;
        private readonly DataSyncService _syncService;
        private readonly AuditoriaService _auditoriaService;
        private readonly RequisicaoLoggerService _requisicaoLogger;

        public event EventHandler<string>? OnOperationCompleted;
        public event EventHandler<Exception>? OnErrorOccurred;
        public event EventHandler<SyncResult>? OnSyncCompleted;

        public LocalizacaoViewModel(AppDbContext context, LogService logger, FirestoreService firestoreService,
            DataSyncService syncService, AuditoriaService auditoriaService, RequisicaoLoggerService requisicaoLogger)
        {
            _context = context;
            _logger = logger;
            _firestoreService = firestoreService;
            _syncService = syncService;
            _auditoriaService = auditoriaService;
            _requisicaoLogger = requisicaoLogger;
        }

        public async Task<List<Localizacao>> ObterTodasLocalizacoes() => await _context.Localizacoes.OrderByDescending(l => l.Timestamp).ToListAsync();
        public async Task<Localizacao?> BuscarLocalizacaoPorId(string id) => await _context.Localizacoes.FindAsync(id);
        public async Task<List<Localizacao>> BuscarLocalizacoesPorCep(string cep) => await _context.Localizacoes.Where(l => l.Cep == cep).OrderByDescending(l => l.Timestamp).ToListAsync();
        public async Task<List<Localizacao>> BuscarLocalizacoesPorBairro(string bairro) => await _context.Localizacoes.Where(l => l.Bairro.Contains(bairro)).OrderByDescending(l => l.Timestamp).ToListAsync();
        public async Task<List<Localizacao>> BuscarLocalizacoesPorPeriodo(DateTime inicio, DateTime fim) => await _context.Localizacoes.Where(l => l.Timestamp >= inicio && l.Timestamp <= fim).OrderByDescending(l => l.Timestamp).ToListAsync();

        public async Task<SyncResult> SincronizarDados()
        {
            var result = await _syncService.SincronizarFirestoreParaLocal();
            OnSyncCompleted?.Invoke(this, result);
            return result;
        }

        public async Task<bool> HaDadosLocais() => await _context.Localizacoes.AnyAsync();
        public async Task<int> LimparCacheAntigo(int dias = 30) => await _syncService.LimparDadosAntigos(dias);

        public async Task<LocalStats> ObterEstatisticasLocais()
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

        public async Task<FirestoreStats> ObterEstatisticasFirestore() => await _firestoreService.ObterEstatisticas();

        public async Task<List<Auditoria>> ObterTodasAlteracoes() => await _auditoriaService.ObterTodasAlteracoes();
        public async Task<List<Auditoria>> BuscarAlteracoesPorUsuario(string usuario) => await _auditoriaService.BuscarAlteracoesPorUsuario(usuario);
        public async Task<AuditoriaStats> ObterEstatisticasAuditoria() => await _auditoriaService.ObterEstatisticas();

        public async Task<List<LogRequisicao>> ObterTodosLogsRequisicao() => await _requisicaoLogger.ObterTodosLogs();
        public async Task<List<LogRequisicao>> ObterLogsPorOperacao(string op) => await _requisicaoLogger.ObterLogsPorOperacao(op);
        public async Task<List<LogRequisicao>> ObterLogsPorCategoria(string cat) => await _requisicaoLogger.ObterLogsPorCategoria(cat);
        public async Task<RequisicaoStats> ObterEstatisticasRequisicoes() => await _requisicaoLogger.ObterEstatisticas();
        public async Task<List<LogRequisicao>> ObterLogsRequisicaoRecentes(HashSet<int> ids) => (await _requisicaoLogger.ObterTodosLogs()).Where(l => !ids.Contains(l.Id)).ToList();
    }

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