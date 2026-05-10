using ConsoleLog.Data;
using ConsoleLog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace ConsoleLog.Services
{
    public class RequisicaoLoggerService
    {
        private readonly AppDbContext _context;
        private readonly LogService _logger;
        private readonly string _usuarioAtual;
        private readonly string _emailUsuario;
        private readonly string _perfilUsuario;
        private readonly string _maquina;
        private readonly string _ipAddress;

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

        public async Task<List<LogRequisicao>> ObterTodosLogs() => await _context.LogsRequisicao.OrderByDescending(l => l.DataHora).ToListAsync();
        public async Task<List<LogRequisicao>> ObterLogsPorOperacao(string operacao) => await _context.LogsRequisicao.Where(l => l.Operacao == operacao).OrderByDescending(l => l.DataHora).ToListAsync();
        public async Task<List<LogRequisicao>> ObterLogsPorCategoria(string categoria) => await _context.LogsRequisicao.Where(l => l.Categoria == categoria).OrderByDescending(l => l.DataHora).ToListAsync();

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

        private string Truncate(string text, int maxLength) => string.IsNullOrEmpty(text) ? "" : text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
    }

    public class RequisicaoStats
    {
        public int TotalRequisicoes { get; set; }
        public int TotalSucesso { get; set; }
        public int TotalErros { get; set; }
        public double MediaDuracaoMs { get; set; }
        public Dictionary<string, int> PorOperacao { get; set; } = new();
        public Dictionary<string, int> PorOrigem { get; set; } = new();
        public DateTime UltimaRequisicao { get; set; }
    }
}