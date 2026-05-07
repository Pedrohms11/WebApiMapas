using ConsoleLog.Data;
using ConsoleLog.Models;
using ConsoleLog.Services;
using ConsoleLog.Services.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleLog.ViewModel
{
    /// <summary>
    /// ViewModel para operações de Localização - APENAS LEITURA
    /// </summary>
    public class LocalizacaoViewModel
    {
        private readonly AppDbContext _context;
        private readonly LogService _logger;
        private readonly FirestoreService _firestoreService;
        private readonly DataSyncService _syncService;

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

        public LocalizacaoViewModel(AppDbContext context, LogService logger, FirestoreService firestoreService, DataSyncService syncService)
        {
            _context = context;
            _logger = logger;
            _firestoreService = firestoreService;
            _syncService = syncService;
        }

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

        /// <summary>
        /// Obtém todas as localizações do SQLite local
        /// </summary>
        public async Task<List<Localizacao>> ObterTodasLocalizacoes()
        {
            try
            {
                _logger.LogInfo("Buscando localizações do SQLite local", "VIEWMODEL");
                return await _context.Localizacoes
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao buscar localizações", ex, "VIEWMODEL");
                OnErrorOccurred?.Invoke(this, ex);
                return new List<Localizacao>();
            }
        }

        /// <summary>
        /// Busca localização por ID
        /// </summary>
        public async Task<Localizacao?> BuscarLocalizacaoPorId(int id)
        {
            try
            {
                return await _context.Localizacoes.FindAsync(id);
            }
            catch (Exception ex)
            {
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
            try
            {
                return await _context.Localizacoes
                    .Where(l => l.Cep == cep)
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
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
            try
            {
                return await _context.Localizacoes
                    .Where(l => l.Bairro.Contains(bairro))
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
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
            try
            {
                return await _context.Localizacoes
                    .Where(l => l.Timestamp >= inicio && l.Timestamp <= fim)
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar localizações por período", ex, "VIEWMODEL");
                OnErrorOccurred?.Invoke(this, ex);
                return new List<Localizacao>();
            }
        }

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
