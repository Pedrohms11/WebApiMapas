using ConsoleLog.Data;
using ConsoleLog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace ConsoleLog.Services
{
    /// <summary>
    /// Serviço responsável pelo registro e consulta de auditoria de ações no sistema
    /// </summary>
    public class AuditoriaService
    {
        private readonly AppDbContext _context;
        private readonly LogService _logger;
        private readonly string _usuarioAtual;
        private readonly string _emailUsuario;
        private readonly string _perfilUsuario;
        private readonly string _maquina;
        private readonly string _ipAddress;

        /// <summary>
        /// Inicializa uma nova instância do serviço de auditoria
        /// </summary>
        /// <param name="context">Contexto do banco de dados</param>
        /// <param name="logger">Serviço de logging do sistema</param>
        /// <param name="configuration">Configurações do sistema contendo dados do usuário atual</param>
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

        /// <summary>
        /// Registra uma operação de inserção no banco de dados
        /// </summary>
        /// <param name="tabela">Nome da tabela onde o registro foi inserido</param>
        /// <param name="registroId">Identificador do registro inserido</param>
        /// <param name="dadosNovos">Dados inseridos em formato JSON</param>
        /// <param name="origem">Origem da ação (API, Interface, Mobile, etc.)</param>
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

        /// <summary>
        /// Registra uma operação de atualização no banco de dados
        /// </summary>
        /// <param name="tabela">Nome da tabela onde o registro foi atualizado</param>
        /// <param name="registroId">Identificador do registro atualizado</param>
        /// <param name="dadosAntigos">Dados anteriores em formato JSON</param>
        /// <param name="dadosNovos">Dados atualizados em formato JSON</param>
        /// <param name="mudancas">Descrição resumida das alterações realizadas</param>
        /// <param name="origem">Origem da ação (API, Interface, Mobile, etc.)</param>
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

        /// <summary>
        /// Registra uma operação de exclusão no banco de dados
        /// </summary>
        /// <param name="tabela">Nome da tabela onde o registro foi removido</param>
        /// <param name="registroId">Identificador do registro removido</param>
        /// <param name="dadosAntigos">Dados removidos em formato JSON</param>
        /// <param name="origem">Origem da ação (API, Interface, Mobile, etc.)</param>
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

        /// <summary>
        /// Obtém todos os registros de auditoria ordenados por data/hora decrescente
        /// </summary>
        /// <returns>Lista de todos os registros de auditoria</returns>
        public async Task<List<Auditoria>> ObterTodasAlteracoes() => await _context.Auditoria.OrderByDescending(a => a.DataHora).ToListAsync();

        /// <summary>
        /// Busca registros de auditoria por nome de usuário
        /// </summary>
        /// <param name="usuario">Nome do usuário para filtro (busca parcial)</param>
        /// <returns>Lista de registros de auditoria do usuário especificado</returns>
        public async Task<List<Auditoria>> BuscarAlteracoesPorUsuario(string usuario) => await _context.Auditoria.Where(a => a.Usuario.Contains(usuario)).OrderByDescending(a => a.DataHora).ToListAsync();

        /// <summary>
        /// Obtém estatísticas consolidadas dos registros de auditoria
        /// </summary>
        /// <returns>Objeto com estatísticas como total de registros, inserts, updates, deletes, usuários ativos e última alteração</returns>
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

    /// <summary>
    /// Representa estatísticas de auditoria do sistema
    /// </summary>
    public class AuditoriaStats
    {
        /// <summary>Total de registros de auditoria</summary>
        public int TotalRegistros { get; set; }

        /// <summary>Total de operações de inserção</summary>
        public int TotalInserts { get; set; }

        /// <summary>Total de operações de atualização</summary>
        public int TotalUpdates { get; set; }

        /// <summary>Total de operações de exclusão</summary>
        public int TotalDeletes { get; set; }

        /// <summary>Número de usuários distintos que realizaram alterações</summary>
        public int UsuariosAtivos { get; set; }

        /// <summary>Data e hora da última alteração registrada</summary>
        public DateTime UltimaAlteracao { get; set; }
    }
}