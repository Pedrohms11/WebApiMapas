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

            // Carregar informações do usuário atual do appsettings.json
            _usuarioAtual = configuration["UsuarioAtual:Nome"] ?? Environment.UserName;
            _emailUsuario = configuration["UsuarioAtual:Email"] ?? $"{Environment.UserName}@local.com";
            _perfilUsuario = configuration["UsuarioAtual:Perfil"] ?? "Usuário";
            _maquina = configuration["UsuarioAtual:Maquina"] ?? Environment.MachineName;
            _ipAddress = configuration["UsuarioAtual:IpAddress"] ?? "127.0.0.1";
        }

        /// <summary>
        /// Registra uma inserção no banco de dados
        /// </summary>
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

            _logger.LogInsert($"✅ INSERT | Tabela: {tabela} | ID: {registroId} | Usuário: {_usuarioAtual} | Origem: {origem}");

            // Exibir detalhes no console
            ExibirAlteracaoNoConsole(auditoria);
        }

        /// <summary>
        /// Registra uma atualização no banco de dados
        /// </summary>
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
                Detalhes = $"Registro atualizado na tabela {tabela} via {origem}. Mudanças: {mudancas}",
                Origem = origem
            };

            await _context.Auditoria.AddAsync(auditoria);
            await _context.SaveChangesAsync();

            _logger.LogUpdate($"🔄 UPDATE | Tabela: {tabela} | ID: {registroId} | Usuário: {_usuarioAtual} | Mudanças: {mudancas}");

            // Exibir detalhes no console
            ExibirAlteracaoNoConsole(auditoria);
        }

        /// <summary>
        /// Registra uma exclusão no banco de dados
        /// </summary>
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

            _logger.LogDelete($"🗑️ DELETE | Tabela: {tabela} | ID: {registroId} | Usuário: {_usuarioAtual} | Origem: {origem}");

            // Exibir detalhes no console
            ExibirAlteracaoNoConsole(auditoria);
        }

        /// <summary>
        /// Busca todas as alterações de um registro específico
        /// </summary>
        public async Task<List<Auditoria>> BuscarHistoricoRegistro(string tabela, string registroId)
        {
            return await _context.Auditoria
                .Where(a => a.Tabela == tabela && a.RegistroId == registroId)
                .OrderByDescending(a => a.DataHora)
                .ToListAsync();
        }

        /// <summary>
        /// Busca alterações por usuário
        /// </summary>
        public async Task<List<Auditoria>> BuscarAlteracoesPorUsuario(string usuario)
        {
            return await _context.Auditoria
                .Where(a => a.Usuario.Contains(usuario))
                .OrderByDescending(a => a.DataHora)
                .ToListAsync();
        }

        /// <summary>
        /// Busca alterações por período
        /// </summary>
        public async Task<List<Auditoria>> BuscarAlteracoesPorPeriodo(DateTime inicio, DateTime fim)
        {
            return await _context.Auditoria
                .Where(a => a.DataHora >= inicio && a.DataHora <= fim)
                .OrderByDescending(a => a.DataHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém todas as alterações
        /// </summary>
        public async Task<List<Auditoria>> ObterTodasAlteracoes()
        {
            return await _context.Auditoria
                .OrderByDescending(a => a.DataHora)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém estatísticas de auditoria
        /// </summary>
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

        /// <summary>
        /// Exibe uma alteração no console em tempo real
        /// </summary>
        private void ExibirAlteracaoNoConsole(Auditoria auditoria)
        {
            var corOriginal = Console.ForegroundColor;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(new string('═', 100));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"🔔 NOVA ALTERAÇÃO DETECTADA - {auditoria.DataHora:HH:mm:ss}");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(new string('═', 100));

            // Cor da ação
            switch (auditoria.Acao)
            {
                case "INSERT":
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case "UPDATE":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case "DELETE":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            Console.WriteLine($"📌 AÇÃO: {auditoria.Acao}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"📋 TABELA: {auditoria.Tabela}");
            Console.WriteLine($"🔑 ID: {auditoria.RegistroId}");
            Console.WriteLine($"👤 USUÁRIO: {auditoria.Usuario} ({auditoria.EmailUsuario}) - {auditoria.PerfilUsuario}");
            Console.WriteLine($"💻 MÁQUINA: {auditoria.Maquina}");
            Console.WriteLine($"📡 ORIGEM: {auditoria.Origem}");
            Console.WriteLine($"📝 DETALHES: {auditoria.Detalhes}");

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(new string('═', 100));
            Console.ForegroundColor = corOriginal;
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