using System.Text;

namespace ConsoleLog.Services
{
    /// <summary>
    /// Serviço responsável pelo gerenciamento de logs do sistema com suporte a arquivos e console colorido
    /// </summary>
    public class LogService
    {
        private readonly string _logFilePath;
        private readonly object _lock = new object();

        /// <summary>
        /// Inicializa uma nova instância do serviço de logging
        /// </summary>
        /// <param name="logFilePath">Caminho do arquivo de log. Padrão: "auditoria.log"</param>
        public LogService(string logFilePath = "auditoria.log")
        {
            _logFilePath = logFilePath;
        }

        /// <summary>
        /// Método privado que escreve o log no arquivo e no console com formatação colorida
        /// </summary>
        /// <param name="mensagem">Mensagem a ser registrada</param>
        /// <param name="tipo">Tipo de log (INFO, ERROR, WARNING, etc.)</param>
        /// <param name="cor">Cor para exibição no console</param>
        /// <param name="operacao">Nome da operação relacionada ao log (opcional)</param>
        private void EscreverLog(string mensagem, string tipo, ConsoleColor cor, string? operacao = null)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var prefixo = string.IsNullOrEmpty(operacao) ? "" : $"[{operacao}] ";
            var linha = $"[{timestamp}] [{tipo}] {prefixo}{mensagem}";

            lock (_lock)
            {
                File.AppendAllText(_logFilePath, linha + Environment.NewLine);
            }

            var corOriginal = Console.ForegroundColor;
            Console.ForegroundColor = cor;
            Console.WriteLine(linha);
            Console.ForegroundColor = corOriginal;
        }

        /// <summary>
        /// Registra uma mensagem informativa no log
        /// </summary>
        /// <param name="mensagem">Mensagem informativa</param>
        /// <param name="operacao">Nome da operação relacionada (opcional)</param>
        public void LogInfo(string mensagem, string? operacao = null)
        {
            EscreverLog(mensagem, "INFO", ConsoleColor.Cyan, operacao);
        }

        /// <summary>
        /// Registra uma mensagem de sucesso no log
        /// </summary>
        /// <param name="mensagem">Mensagem de sucesso</param>
        /// <param name="operacao">Nome da operação relacionada (opcional)</param>
        public void LogSuccess(string mensagem, string? operacao = null)
        {
            EscreverLog($"✓ {mensagem}", "SUCCESS", ConsoleColor.Green, operacao);
        }

        /// <summary>
        /// Registra uma mensagem de aviso no log
        /// </summary>
        /// <param name="mensagem">Mensagem de aviso</param>
        /// <param name="operacao">Nome da operação relacionada (opcional)</param>
        public void LogWarning(string mensagem, string? operacao = null)
        {
            EscreverLog($"⚠ {mensagem}", "WARNING", ConsoleColor.Yellow, operacao);
        }

        /// <summary>
        /// Registra uma mensagem de erro no log com detalhes da exceção
        /// </summary>
        /// <param name="mensagem">Mensagem de erro</param>
        /// <param name="ex">Exceção ocorrida (opcional)</param>
        /// <param name="operacao">Nome da operação relacionada (opcional)</param>
        public void LogError(string mensagem, Exception? ex = null, string? operacao = null)
        {
            var detalhe = ex != null ? $" - {ex.Message}" : "";
            EscreverLog($"✗ {mensagem}{detalhe}", "ERROR", ConsoleColor.Red, operacao);
        }

        /// <summary>
        /// Registra uma operação de inserção no log
        /// </summary>
        /// <param name="mensagem">Mensagem descritiva da inserção</param>
        /// <param name="operacao">Nome da operação relacionada (opcional)</param>
        public void LogInsert(string mensagem, string? operacao = null)
        {
            EscreverLog($"➕ {mensagem}", "INSERT", ConsoleColor.Green, operacao);
        }

        /// <summary>
        /// Registra uma operação de atualização no log
        /// </summary>
        /// <param name="mensagem">Mensagem descritiva da atualização</param>
        /// <param name="operacao">Nome da operação relacionada (opcional)</param>
        public void LogUpdate(string mensagem, string? operacao = null)
        {
            EscreverLog($"🔄 {mensagem}", "UPDATE", ConsoleColor.Cyan, operacao);
        }

        /// <summary>
        /// Registra uma operação de exclusão no log
        /// </summary>
        /// <param name="mensagem">Mensagem descritiva da exclusão</param>
        /// <param name="operacao">Nome da operação relacionada (opcional)</param>
        public void LogDelete(string mensagem, string? operacao = null)
        {
            EscreverLog($"🗑️ {mensagem}", "DELETE", ConsoleColor.Red, operacao);
        }

        /// <summary>
        /// Registra uma mensagem de auditoria no log com tipo personalizado
        /// </summary>
        /// <param name="mensagem">Mensagem de auditoria</param>
        /// <param name="tipo">Tipo de auditoria (INSERT, UPDATE, DELETE, ERROR, WARNING)</param>
        public void LogAuditoria(string mensagem, string tipo = "INFO")
        {
            var cor = tipo switch
            {
                "INSERT" => ConsoleColor.Green,
                "UPDATE" => ConsoleColor.Cyan,
                "DELETE" => ConsoleColor.Red,
                "ERROR" => ConsoleColor.Red,
                "WARNING" => ConsoleColor.Yellow,
                _ => ConsoleColor.White
            };
            EscreverLog(mensagem, tipo, cor, null);
        }

        /// <summary>
        /// Registra uma requisição HTTP no log com status e duração
        /// </summary>
        /// <param name="metodo">Método HTTP (GET, POST, PUT, DELETE)</param>
        /// <param name="endpoint">Endpoint da requisição</param>
        /// <param name="statusCode">Código de status HTTP da resposta</param>
        /// <param name="duracaoMs">Duração da requisição em milissegundos</param>
        public void LogRequisicao(string metodo, string endpoint, int statusCode, long duracaoMs)
        {
            var sucesso = statusCode >= 200 && statusCode < 300;
            var icon = sucesso ? "✅" : "❌";
            var cor = sucesso ? ConsoleColor.Green : ConsoleColor.Red;
            EscreverLog($"{icon} {metodo} {endpoint} - {statusCode} - {duracaoMs}ms", "REQUEST", cor, null);
        }

        /// <summary>
        /// Registra uma operação de sincronização no log
        /// </summary>
        /// <param name="mensagem">Mensagem descritiva da sincronização</param>
        /// <param name="sucesso">Indica se a sincronização foi bem-sucedida</param>
        public void LogSync(string mensagem, bool sucesso = true)
        {
            var icon = sucesso ? "🔄" : "❌";
            var cor = sucesso ? ConsoleColor.Magenta : ConsoleColor.Red;
            EscreverLog($"{icon} SYNC: {mensagem}", "SYNC", cor, null);
        }

        /// <summary>
        /// Exibe uma barra de progresso no console para operações longas
        /// </summary>
        /// <param name="current">Progresso atual</param>
        /// <param name="total">Total de itens a processar</param>
        /// <param name="mensagem">Mensagem descritiva da operação em andamento</param>
        public void LogProgress(int current, int total, string mensagem)
        {
            lock (_lock)
            {
                var percent = (int)((double)current / total * 100);
                var barLength = 30;
                var filledLength = (int)((double)current / total * barLength);
                var bar = new string('█', filledLength) + new string('░', barLength - filledLength);
                Console.Write($"\r[{bar}] {percent}% - {mensagem}               ");
                if (current == total) Console.WriteLine();
            }
        }
    }
}