using System.Text;

namespace ConsoleLog.Services
{
    public class LogService
    {
        private readonly string _logFilePath;
        private readonly object _lock = new object();

        public LogService(string logFilePath = "auditoria.log")
        {
            _logFilePath = logFilePath;
        }

        /// <summary>
        /// Método genérico para registrar logs no arquivo e console
        /// </summary>
        private void EscreverLog(string mensagem, string tipo, ConsoleColor cor)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var linha = $"[{timestamp}] [{tipo}] {mensagem}";

            // Salvar no arquivo
            lock (_lock)
            {
                File.AppendAllText(_logFilePath, linha + Environment.NewLine);
            }

            // Exibir no console com cor
            var corOriginal = Console.ForegroundColor;
            Console.ForegroundColor = cor;
            Console.WriteLine(linha);
            Console.ForegroundColor = corOriginal;
        }

        /// <summary>
        /// Log de informação geral
        /// </summary>
        public void LogInfo(string mensagem, string? operacao = null)
        {
            var prefixo = string.IsNullOrEmpty(operacao) ? "" : $"[{operacao}] ";
            EscreverLog($"{prefixo}{mensagem}", "INFO", ConsoleColor.Cyan);
        }

        /// <summary>
        /// Log de sucesso
        /// </summary>
        public void LogSuccess(string mensagem, string? operacao = null)
        {
            var prefixo = string.IsNullOrEmpty(operacao) ? "" : $"[{operacao}] ";
            EscreverLog($"{prefixo}✓ {mensagem}", "SUCCESS", ConsoleColor.Green);
        }

        /// <summary>
        /// Log de aviso
        /// </summary>
        public void LogWarning(string mensagem, string? operacao = null)
        {
            var prefixo = string.IsNullOrEmpty(operacao) ? "" : $"[{operacao}] ";
            EscreverLog($"{prefixo}⚠ {mensagem}", "WARNING", ConsoleColor.Yellow);
        }

        /// <summary>
        /// Log de erro
        /// </summary>
        public void LogError(string mensagem, Exception? ex = null, string? operacao = null)
        {
            var prefixo = string.IsNullOrEmpty(operacao) ? "" : $"[{operacao}] ";
            var detalhe = ex != null ? $" - {ex.Message}" : "";
            EscreverLog($"{prefixo}✗ {mensagem}{detalhe}", "ERROR", ConsoleColor.Red);
        }

        /// <summary>
        /// Log específico para operação de INSERT
        /// </summary>
        public void LogInsert(string mensagem, string? operacao = null)
        {
            var prefixo = string.IsNullOrEmpty(operacao) ? "" : $"[{operacao}] ";
            EscreverLog($"{prefixo}➕ {mensagem}", "INSERT", ConsoleColor.Green);
        }

        /// <summary>
        /// Log específico para operação de UPDATE
        /// </summary>
        public void LogUpdate(string mensagem, string? operacao = null)
        {
            var prefixo = string.IsNullOrEmpty(operacao) ? "" : $"[{operacao}] ";
            EscreverLog($"{prefixo}🔄 {mensagem}", "UPDATE", ConsoleColor.Cyan);
        }

        /// <summary>
        /// Log específico para operação de DELETE
        /// </summary>
        public void LogDelete(string mensagem, string? operacao = null)
        {
            var prefixo = string.IsNullOrEmpty(operacao) ? "" : $"[{operacao}] ";
            EscreverLog($"{prefixo}🗑️ {mensagem}", "DELETE", ConsoleColor.Red);
        }

        /// <summary>
        /// Log de auditoria genérico
        /// </summary>
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

            EscreverLog(mensagem, tipo, cor);
        }

        /// <summary>
        /// Log de requisição HTTP/API
        /// </summary>
        public void LogRequisicao(string metodo, string endpoint, int statusCode, long duracaoMs)
        {
            var sucesso = statusCode >= 200 && statusCode < 300;
            var icon = sucesso ? "✅" : "❌";
            var cor = sucesso ? ConsoleColor.Green : ConsoleColor.Red;

            EscreverLog($"{icon} {metodo} {endpoint} - {statusCode} - {duracaoMs}ms", "REQUEST", cor);
        }

        /// <summary>
        /// Log de sincronização
        /// </summary>
        public void LogSync(string mensagem, bool sucesso = true)
        {
            var icon = sucesso ? "🔄" : "❌";
            var cor = sucesso ? ConsoleColor.Magenta : ConsoleColor.Red;

            EscreverLog($"{icon} SYNC: {mensagem}", "SYNC", cor);
        }

        /// <summary>
        /// Cria uma barra de progresso simples
        /// </summary>
        public void LogProgress(int current, int total, string mensagem)
        {
            lock (_lock)
            {
                var percent = (int)((double)current / total * 100);
                var barLength = 30;
                var filledLength = (int)((double)current / total * barLength);
                var bar = new string('█', filledLength) + new string('░', barLength - filledLength);

                Console.Write($"\r[{bar}] {percent}% - {mensagem}               ");

                if (current == total)
                    Console.WriteLine();
            }
        }

        /// <summary>
        /// Limpa o arquivo de log
        /// </summary>
        public void LimparLog()
        {
            lock (_lock)
            {
                if (File.Exists(_logFilePath))
                {
                    File.WriteAllText(_logFilePath, string.Empty);
                    LogInfo("Arquivo de log limpo com sucesso", "LOG");
                }
            }
        }

        /// <summary>
        /// Lê o conteúdo do arquivo de log
        /// </summary>
        public string LerLog()
        {
            lock (_lock)
            {
                if (File.Exists(_logFilePath))
                {
                    return File.ReadAllText(_logFilePath);
                }
                return "Nenhum log encontrado.";
            }
        }
    }
}