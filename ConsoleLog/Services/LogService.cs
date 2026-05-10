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

        public void LogInfo(string mensagem, string? operacao = null)
        {
            EscreverLog(mensagem, "INFO", ConsoleColor.Cyan, operacao);
        }

        public void LogSuccess(string mensagem, string? operacao = null)
        {
            EscreverLog($"✓ {mensagem}", "SUCCESS", ConsoleColor.Green, operacao);
        }

        public void LogWarning(string mensagem, string? operacao = null)
        {
            EscreverLog($"⚠ {mensagem}", "WARNING", ConsoleColor.Yellow, operacao);
        }

        public void LogError(string mensagem, Exception? ex = null, string? operacao = null)
        {
            var detalhe = ex != null ? $" - {ex.Message}" : "";
            EscreverLog($"✗ {mensagem}{detalhe}", "ERROR", ConsoleColor.Red, operacao);
        }

        public void LogInsert(string mensagem, string? operacao = null)
        {
            EscreverLog($"➕ {mensagem}", "INSERT", ConsoleColor.Green, operacao);
        }

        public void LogUpdate(string mensagem, string? operacao = null)
        {
            EscreverLog($"🔄 {mensagem}", "UPDATE", ConsoleColor.Cyan, operacao);
        }

        public void LogDelete(string mensagem, string? operacao = null)
        {
            EscreverLog($"🗑️ {mensagem}", "DELETE", ConsoleColor.Red, operacao);
        }

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

        public void LogRequisicao(string metodo, string endpoint, int statusCode, long duracaoMs)
        {
            var sucesso = statusCode >= 200 && statusCode < 300;
            var icon = sucesso ? "✅" : "❌";
            var cor = sucesso ? ConsoleColor.Green : ConsoleColor.Red;
            EscreverLog($"{icon} {metodo} {endpoint} - {statusCode} - {duracaoMs}ms", "REQUEST", cor, null);
        }

        public void LogSync(string mensagem, bool sucesso = true)
        {
            var icon = sucesso ? "🔄" : "❌";
            var cor = sucesso ? ConsoleColor.Magenta : ConsoleColor.Red;
            EscreverLog($"{icon} SYNC: {mensagem}", "SYNC", cor, null);
        }

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