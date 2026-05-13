using ConsoleLog.Data;
using ConsoleLog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace ConsoleLog.Services
{
    /// <summary>
    /// Serviço responsável pelo logging de requisições HTTP/API para auditoria e monitoramento
    /// </summary>
    public class RequisicaoLoggerService
    {
        private readonly AppDbContext _context;
        private readonly LogService _logger;
        private readonly string _usuarioAtual;
        private readonly string _emailUsuario;
        private readonly string _perfilUsuario;
        private readonly string _maquina;
        private readonly string _ipAddress;

        /// <summary>
        /// Inicializa uma nova instância do serviço de logging de requisições
        /// </summary>
        /// <param name="context">Contexto do banco de dados</param>
        /// <param name="logger">Serviço de logging do sistema</param>
        /// <param name="configuration">Configurações do sistema contendo dados do usuário atual</param>
        public RequisicaoLoggerService(AppDbContext context, LogService logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _usuarioAtual = configuration["UsuarioAtual:Nome"] ?? Environment.UserName;
            _emailUsuario = configuration["UsuarioAtual:Email"] ?? $"{Environment.UserName}@local.com";
            _perfilUsuario = configuration["UsuarioAtual:Perfil"] ?? "Usuário";
            _maquina = configuration["UsuarioAtual:Maquina"] ?? Environment.MachineName;
            _ipAddress = configuration["UsuarioAtual:IpAddress"] ?? "127.0.0.1";
        }

        /// <summary>
        /// Registra uma requisição de leitura (GET) no log de requisições
        /// </summary>
        /// <param name="endpoint">Endpoint da requisição</param>
        /// <param name="parametros">Parâmetros enviados na requisição</param>
        /// <param name="resultado">Resultado ou resposta da requisição</param>
        /// <param name="duracaoMs">Duração da requisição em milissegundos</param>
        /// <param name="sucesso">Indica se a requisição foi bem-sucedida</param>
        /// <param name="erro">Mensagem de erro (opcional, em caso de falha)</param>
        /// <returns>Objeto LogRequisicao criado</returns>
        public async Task<LogRequisicao> LogLeitura(string endpoint, string parametros, string resultado, long duracaoMs, bool sucesso, string erro = "")
        {
            var log = new LogRequisicao
            {
                Operacao = "GET",
                Endpoint = endpoint,
                Parametros = parametros,
                ResponseBody = Truncate(resultado, 500),
                StatusCode = sucesso ? 200 : 500,
                DuracaoMs = duracaoMs,
                Usuario = _usuarioAtual,
                EmailUsuario = _emailUsuario,
                PerfilUsuario = _perfilUsuario,
                Maquina = _maquina,
                IpAddress = _ipAddress,
                DataHora = DateTime.Now,
                Sucesso = sucesso,
                MensagemErro = erro,
                Origem = "API",
                Categoria = "Leitura"
            };

            await _context.LogsRequisicao.AddAsync(log);
            await _context.SaveChangesAsync();
            return log;
        }

        /// <summary>
        /// Obtém todos os logs de requisição ordenados por data/hora decrescente
        /// </summary>
        /// <returns>Lista de todos os logs de requisição</returns>
        public async Task<List<LogRequisicao>> ObterTodosLogs() => await _context.LogsRequisicao.OrderByDescending(l => l.DataHora).ToListAsync();

        /// <summary>
        /// Obtém logs de requisição filtrados por operação (GET, POST, PUT, DELETE)
        /// </summary>
        /// <param name="operacao">Nome da operação HTTP</param>
        /// <returns>Lista de logs da operação especificada</returns>
        public async Task<List<LogRequisicao>> ObterLogsPorOperacao(string operacao) => await _context.LogsRequisicao.Where(l => l.Operacao == operacao).OrderByDescending(l => l.DataHora).ToListAsync();

        /// <summary>
        /// Obtém logs de requisição filtrados por categoria (Leitura, Escrita, etc.)
        /// </summary>
        /// <param name="categoria">Categoria da requisição</param>
        /// <returns>Lista de logs da categoria especificada</returns>
        public async Task<List<LogRequisicao>> ObterLogsPorCategoria(string categoria) => await _context.LogsRequisicao.Where(l => l.Categoria == categoria).OrderByDescending(l => l.DataHora).ToListAsync();

        /// <summary>
        /// Obtém estatísticas consolidadas das requisições registradas
        /// </summary>
        /// <returns>Objeto com estatísticas como total, sucessos, erros, média de duração e distribuições</returns>
        public async Task<RequisicaoStats> ObterEstatisticas()
        {
            var logs = await _context.LogsRequisicao.ToListAsync();
            return new RequisicaoStats
            {
                TotalRequisicoes = logs.Count,
                TotalSucesso = logs.Count(l => l.Sucesso),
                TotalErros = logs.Count(l => !l.Sucesso),
                MediaDuracaoMs = logs.Any() ? logs.Average(l => l.DuracaoMs) : 0,
                PorOperacao = logs.GroupBy(l => l.Operacao).ToDictionary(g => g.Key, g => g.Count()),
                PorOrigem = logs.GroupBy(l => l.Origem).ToDictionary(g => g.Key, g => g.Count()),
                UltimaRequisicao = logs.Any() ? logs.Max(l => l.DataHora) : DateTime.MinValue
            };
        }

        /// <summary>
        /// Trunca um texto para o comprimento máximo especificado, adicionando "..." ao final se necessário
        /// </summary>
        /// <param name="text">Texto a ser truncado</param>
        /// <param name="maxLength">Comprimento máximo permitido</param>
        /// <returns>Texto truncado ou original se dentro do limite</returns>
        private string Truncate(string text, int maxLength) => string.IsNullOrEmpty(text) ? "" : text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
    }

    /// <summary>
    /// Representa estatísticas das requisições registradas no sistema
    /// </summary>
    public class RequisicaoStats
    {
        /// <summary>Total de requisições registradas</summary>
        public int TotalRequisicoes { get; set; }

        /// <summary>Total de requisições bem-sucedidas</summary>
        public int TotalSucesso { get; set; }

        /// <summary>Total de requisições com erro</summary>
        public int TotalErros { get; set; }

        /// <summary>Média de duração das requisições em milissegundos</summary>
        public double MediaDuracaoMs { get; set; }

        /// <summary>Distribuição de requisições por operação HTTP (GET, POST, PUT, DELETE)</summary>
        public Dictionary<string, int> PorOperacao { get; set; } = new();

        /// <summary>Distribuição de requisições por origem (API, Mobile, Web, etc.)</summary>
        public Dictionary<string, int> PorOrigem { get; set; } = new();

        /// <summary>Data e hora da última requisição registrada</summary>
        public DateTime UltimaRequisicao { get; set; }
    }
}