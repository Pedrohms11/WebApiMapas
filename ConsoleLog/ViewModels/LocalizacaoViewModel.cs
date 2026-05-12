using Microsoft.EntityFrameworkCore;
using ConsoleLog.Models;
using ConsoleLog.Data;
using ConsoleLog.Services;
using ConsoleLog.Services.Sync;

namespace ConsoleLog.ViewModels
{
    /// <summary>
    /// ViewModel responsável pela mediação entre a camada de visualização e os serviços do sistema
    /// Gerencia localizações, sincronização, auditoria e logs de requisição
    /// </summary>
    public class LocalizacaoViewModel
    {
        private readonly AppDbContext _context;
        private readonly LogService _logger;
        private readonly FirestoreService _firestoreService;
        private readonly DataSyncService _syncService;
        private readonly AuditoriaService _auditoriaService;
        private readonly RequisicaoLoggerService _requisicaoLogger;

        /// <summary>Evento disparado quando uma operação é concluída com sucesso</summary>
        public event EventHandler<string>? OnOperationCompleted;

        /// <summary>Evento disparado quando ocorre um erro durante uma operação</summary>
        public event EventHandler<Exception>? OnErrorOccurred;

        /// <summary>Evento disparado quando a sincronização de dados é concluída</summary>
        public event EventHandler<SyncResult>? OnSyncCompleted;

        /// <summary>
        /// Inicializa uma nova instância do ViewModel de localizações
        /// </summary>
        /// <param name="context">Contexto do banco de dados local</param>
        /// <param name="logger">Serviço de logging do sistema</param>
        /// <param name="firestoreService">Serviço de integração com Firebase Firestore</param>
        /// <param name="syncService">Serviço de sincronização de dados</param>
        /// <param name="auditoriaService">Serviço de auditoria</param>
        /// <param name="requisicaoLogger">Serviço de logging de requisições</param>
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

<<<<<<< HEAD
        /// <summary>
        /// Obtém todas as localizações do banco de dados local ordenadas por Timestamp decrescente
        /// </summary>
        /// <returns>Lista de todas as localizações</returns>
        public async Task<List<Localizacao>> ObterTodasLocalizacoes() => await _context.Localizacoes.OrderByDescending(l => l.Timestamp).ToListAsync();

        /// <summary>
        /// Busca uma localização pelo seu identificador único
        /// </summary>
        /// <param name="id">Identificador da localização</param>
        /// <returns>Objeto Localizacao ou null se não encontrado</returns>
        public async Task<Localizacao?> BuscarLocalizacaoPorId(string id) => await _context.Localizacoes.FindAsync(id);

        /// <summary>
        /// Busca localizações por CEP (Código Postal)
        /// </summary>
        /// <param name="cep">CEP a ser pesquisado</param>
        /// <returns>Lista de localizações com o CEP especificado</returns>
        public async Task<List<Localizacao>> BuscarLocalizacoesPorCep(string cep) => await _context.Localizacoes.Where(l => l.Cep == cep).OrderByDescending(l => l.Timestamp).ToListAsync();

        /// <summary>
        /// Busca localizações por bairro (busca parcial)
        /// </summary>
        /// <param name="bairro">Nome do bairro a ser pesquisado</param>
        /// <returns>Lista de localizações do bairro especificado</returns>
        public async Task<List<Localizacao>> BuscarLocalizacoesPorBairro(string bairro) => await _context.Localizacoes.Where(l => l.Bairro.Contains(bairro)).OrderByDescending(l => l.Timestamp).ToListAsync();

        /// <summary>
        /// Busca localizações dentro de um período específico
        /// </summary>
        /// <param name="inicio">Data e hora inicial do período</param>
        /// <param name="fim">Data e hora final do período</param>
        /// <returns>Lista de localizações registradas no período</returns>
        public async Task<List<Localizacao>> BuscarLocalizacoesPorPeriodo(DateTime inicio, DateTime fim) => await _context.Localizacoes.Where(l => l.Timestamp >= inicio && l.Timestamp <= fim).OrderByDescending(l => l.Timestamp).ToListAsync();
=======
        public async Task<List<Localizacao>> ObterTodasLocalizacoes()
            => await _context.Localizacoes.OrderByDescending(l => l.Timestamp).ToListAsync();
        public async Task<Localizacao?> BuscarLocalizacaoPorId(string id)
            => await _context.Localizacoes.FindAsync(id);
        public async Task<List<Localizacao>> BuscarLocalizacoesPorCep(string cep)
            => await _context.Localizacoes.Where(l => l.Cep == cep).OrderByDescending(l => l.Timestamp).ToListAsync();
        public async Task<List<Localizacao>> BuscarLocalizacoesPorBairro(string bairro)
            => await _context.Localizacoes.Where(l => l.Bairro.Contains(bairro)).OrderByDescending(l => l.Timestamp).ToListAsync();
        public async Task<List<Localizacao>> BuscarLocalizacoesPorPeriodo(DateTime inicio, DateTime fim)
            => await _context.Localizacoes.Where(l => l.Timestamp >= inicio && l.Timestamp <= fim).OrderByDescending(l => l.Timestamp).ToListAsync();
>>>>>>> 0803e68c81502ac05182e2b46359eaf802f1d777

        /// <summary>
        /// Inicia o processo de sincronização de dados do Firestore para o banco local
        /// </summary>
        /// <returns>Resultado da sincronização com detalhes das operações</returns>
        public async Task<SyncResult> SincronizarDados()
        {
            var result = await _syncService.SincronizarFirestoreParaLocal();
            OnSyncCompleted?.Invoke(this, result);
            return result;
        }

        /// <summary>
        /// Verifica se existem dados armazenados localmente
        /// </summary>
        /// <returns>True se houver dados locais, False caso contrário</returns>
        public async Task<bool> HaDadosLocais() => await _context.Localizacoes.AnyAsync();

        /// <summary>
        /// Limpa dados locais antigos com base na data da última sincronização
        /// </summary>
        /// <param name="dias">Número de dias para considerar como "antigo" (padrão: 30)</param>
        /// <returns>Número de registros removidos</returns>
        public async Task<int> LimparCacheAntigo(int dias = 30) => await _syncService.LimparDadosAntigos(dias);

        /// <summary>
        /// Obtém estatísticas consolidadas dos dados locais
        /// </summary>
        /// <returns>Objeto com estatísticas como total de registros, última sincronização, registros antigos/recentes, bairros e CEPs únicos</returns>
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

<<<<<<< HEAD
        /// <summary>
        /// Obtém estatísticas dos dados armazenados no Firestore
        /// </summary>
        /// <returns>Objeto com estatísticas do Firestore</returns>
        public async Task<FirestoreStats> ObterEstatisticasFirestore() => await _firestoreService.ObterEstatisticas();

        /// <summary>
        /// Obtém todos os registros de auditoria
        /// </summary>
        /// <returns>Lista de todos os registros de auditoria</returns>
        public async Task<List<Auditoria>> ObterTodasAlteracoes() => await _auditoriaService.ObterTodasAlteracoes();

        /// <summary>
        /// Busca registros de auditoria por usuário
        /// </summary>
        /// <param name="usuario">Nome do usuário para filtro</param>
        /// <returns>Lista de registros de auditoria do usuário especificado</returns>
        public async Task<List<Auditoria>> BuscarAlteracoesPorUsuario(string usuario) => await _auditoriaService.BuscarAlteracoesPorUsuario(usuario);

        /// <summary>
        /// Obtém estatísticas consolidadas da auditoria
        /// </summary>
        /// <returns>Objeto com estatísticas de auditoria</returns>
        public async Task<AuditoriaStats> ObterEstatisticasAuditoria() => await _auditoriaService.ObterEstatisticas();

        /// <summary>
        /// Obtém todos os logs de requisição
        /// </summary>
        /// <returns>Lista de todos os logs de requisição</returns>
        public async Task<List<LogRequisicao>> ObterTodosLogsRequisicao() => await _requisicaoLogger.ObterTodosLogs();

        /// <summary>
        /// Obtém logs de requisição filtrados por operação HTTP
        /// </summary>
        /// <param name="op">Operação HTTP (GET, POST, PUT, DELETE)</param>
        /// <returns>Lista de logs da operação especificada</returns>
        public async Task<List<LogRequisicao>> ObterLogsPorOperacao(string op) => await _requisicaoLogger.ObterLogsPorOperacao(op);

        /// <summary>
        /// Obtém logs de requisição filtrados por categoria
        /// </summary>
        /// <param name="cat">Categoria da requisição (Leitura, Escrita, etc.)</param>
        /// <returns>Lista de logs da categoria especificada</returns>
        public async Task<List<LogRequisicao>> ObterLogsPorCategoria(string cat) => await _requisicaoLogger.ObterLogsPorCategoria(cat);

        /// <summary>
        /// Obtém estatísticas consolidadas das requisições
        /// </summary>
        /// <returns>Objeto com estatísticas de requisições</returns>
        public async Task<RequisicaoStats> ObterEstatisticasRequisicoes() => await _requisicaoLogger.ObterEstatisticas();

        /// <summary>
        /// Obtém logs de requisição recentes excluindo IDs já visualizados
        /// </summary>
        /// <param name="ids">HashSet com IDs já carregados para exclusão</param>
        /// <returns>Lista de logs de requisição não visualizados anteriormente</returns>
        public async Task<List<LogRequisicao>> ObterLogsRequisicaoRecentes(HashSet<int> ids) => (await _requisicaoLogger.ObterTodosLogs()).Where(l => !ids.Contains(l.Id)).ToList();
=======
        public async Task<FirestoreStats> ObterEstatisticasFirestore()
            => await _firestoreService.ObterEstatisticas();

        public async Task<List<Auditoria>> ObterTodasAlteracoes()
            => await _auditoriaService.ObterTodasAlteracoes();

        public async Task<List<Auditoria>> BuscarAlteracoesPorUsuario(string usuario)
            => await _auditoriaService.BuscarAlteracoesPorUsuario(usuario);

        public async Task<AuditoriaStats> ObterEstatisticasAuditoria()
            => await _auditoriaService.ObterEstatisticas();

        public async Task<List<LogRequisicao>> ObterTodosLogsRequisicao()
            => await _requisicaoLogger.ObterTodosLogs();

        public async Task<List<LogRequisicao>> ObterLogsPorOperacao(string op)
            => await _requisicaoLogger.ObterLogsPorOperacao(op);

        public async Task<List<LogRequisicao>> ObterLogsPorCategoria(string cat)
            => await _requisicaoLogger.ObterLogsPorCategoria(cat);

        public async Task<RequisicaoStats> ObterEstatisticasRequisicoes()
            => await _requisicaoLogger.ObterEstatisticas();

        public async Task<List<LogRequisicao>> ObterLogsRequisicaoRecentes(HashSet<int> ids)
            => (await _requisicaoLogger.ObterTodosLogs()).Where(l => !ids.Contains(l.Id)).ToList();
>>>>>>> 0803e68c81502ac05182e2b46359eaf802f1d777
    }

    /// <summary>
    /// Representa estatísticas consolidadas dos dados locais de localização
    /// </summary>
    public class LocalStats
    {
        /// <summary>Total de registros de localização no banco local</summary>
        public int TotalRegistros { get; set; }

        /// <summary>Data e hora da última sincronização realizada</summary>
        public DateTime? UltimaSincronizacao { get; set; }

        /// <summary>Data e hora do registro mais antigo no banco</summary>
        public DateTime RegistroMaisAntigo { get; set; }

        /// <summary>Data e hora do registro mais recente no banco</summary>
        public DateTime RegistroMaisRecente { get; set; }

        /// <summary>Número de bairros distintos presentes nos registros</summary>
        public int BairrosUnicos { get; set; }

        /// <summary>Número de CEPs distintos presentes nos registros</summary>
        public int CepsUnicos { get; set; }
    }
}