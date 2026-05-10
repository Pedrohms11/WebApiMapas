using ConsoleLog.Data;
using ConsoleLog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace ConsoleLog.Services
{
    public class AuditoriaService
    {
        private readonly AppDbContext _context;
        private readonly LogService _logger;
        private readonly string _usuarioAtual;
        private readonly string _emailUsuario;
        private readonly string _perfilUsuario;
        private readonly string _maquina;
        private readonly string _ipAddress;

        public AuditoriaService(AppDbContext context, LogService logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _usuarioAtual = configuration["UsuarioAtual:Nome"] ?? Environment.UserName;
            _emailUsuario = configuration["UsuarioAtual:Email"] ?? $"{Environment.UserName}@local.com";
            _perfilUsuario = configuration["UsuarioAtual:Perfil"] ?? "Usuário";
            _maquina = configuration["UsuarioAtual:Maquina"] ?? Environment.MachineName;
            _ipAddress = configuration["UsuarioAtual:IpAddress"] ?? "127.0.0.1";
        }

        public async Task RegistrarInsercao(string tabela, string registroId, string dadosNovos, string origem = "API")
        {
            var auditoria = new Auditoria
            {
                Tabela = tabela,
                RegistroId = registroId,
                Acao = "INSERT",
                DadosAntigos = "{}",
                DadosNovos = dadosNovos,
                Usuario = _usuarioAtual,
                EmailUsuario = _emailUsuario,
                PerfilUsuario = _perfilUsuario,
                Maquina = _maquina,
                IpAddress = _ipAddress,
                DataHora = DateTime.Now,
                Detalhes = $"Novo registro criado na tabela {tabela} via {origem}",
                Origem = origem
            };

            await _context.Auditoria.AddAsync(auditoria);
            await _context.SaveChangesAsync();
            _logger.LogInsert($"INSERT | {tabela} ID:{registroId} | Usuário: {_usuarioAtual}", "AUDITORIA");
        }

        public async Task RegistrarAtualizacao(string tabela, string registroId, string dadosAntigos, string dadosNovos, string mudancas, string origem = "API")
        {
            var auditoria = new Auditoria
            {
                Tabela = tabela,
                RegistroId = registroId,
                Acao = "UPDATE",
                DadosAntigos = dadosAntigos,
                DadosNovos = dadosNovos,
                Usuario = _usuarioAtual,
                EmailUsuario = _emailUsuario,
                PerfilUsuario = _perfilUsuario,
                Maquina = _maquina,
                IpAddress = _ipAddress,
                DataHora = DateTime.Now,
                Detalhes = $"Registro atualizado: {mudancas}",
                Origem = origem
            };

            await _context.Auditoria.AddAsync(auditoria);
            await _context.SaveChangesAsync();
            _logger.LogUpdate($"UPDATE | {tabela} ID:{registroId} | {mudancas}", "AUDITORIA");
        }

        public async Task RegistrarExclusao(string tabela, string registroId, string dadosAntigos, string origem = "API")
        {
            var auditoria = new Auditoria
            {
                Tabela = tabela,
                RegistroId = registroId,
                Acao = "DELETE",
                DadosAntigos = dadosAntigos,
                DadosNovos = "{}",
                Usuario = _usuarioAtual,
                EmailUsuario = _emailUsuario,
                PerfilUsuario = _perfilUsuario,
                Maquina = _maquina,
                IpAddress = _ipAddress,
                DataHora = DateTime.Now,
                Detalhes = $"Registro removido da tabela {tabela} via {origem}",
                Origem = origem
            };

            await _context.Auditoria.AddAsync(auditoria);
            await _context.SaveChangesAsync();
            _logger.LogDelete($"DELETE | {tabela} ID:{registroId}", "AUDITORIA");
        }

        public async Task<List<Auditoria>> ObterTodasAlteracoes() => await _context.Auditoria.OrderByDescending(a => a.DataHora).ToListAsync();
        public async Task<List<Auditoria>> BuscarAlteracoesPorUsuario(string usuario) => await _context.Auditoria.Where(a => a.Usuario.Contains(usuario)).OrderByDescending(a => a.DataHora).ToListAsync();

        public async Task<AuditoriaStats> ObterEstatisticas()
        {
            var todas = await _context.Auditoria.ToListAsync();
            return new AuditoriaStats
            {
                TotalRegistros = todas.Count,
                TotalInserts = todas.Count(a => a.Acao == "INSERT"),
                TotalUpdates = todas.Count(a => a.Acao == "UPDATE"),
                TotalDeletes = todas.Count(a => a.Acao == "DELETE"),
                UsuariosAtivos = todas.Select(a => a.Usuario).Distinct().Count(),
                UltimaAlteracao = todas.Any() ? todas.Max(a => a.DataHora) : DateTime.MinValue
            };
        }
    }

    public class AuditoriaStats
    {
        public int TotalRegistros { get; set; }
        public int TotalInserts { get; set; }
        public int TotalUpdates { get; set; }
        public int TotalDeletes { get; set; }
        public int UsuariosAtivos { get; set; }
        public DateTime UltimaAlteracao { get; set; }
    }
}