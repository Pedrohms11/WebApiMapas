using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleLog.Services
{
    /// <summary>
    /// Serviço de logging para console com cores (Camada Service)
    /// </summary>
    public class LogService
    {
        private static readonly object _lock = new object();

        /// <summary>
        /// Registra log de informação
        /// </summary>
        public void LogInfo(string mensagem, string? operacao = null)
        {
            lock (_lock)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var originalColor = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"[{timestamp}] ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[INFO] ");

                if (!string.IsNullOrEmpty(operacao))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"[{operacao}] ");
                }

                Console.ForegroundColor = originalColor;
                Console.WriteLine(mensagem);
            }
        }

        /// <summary>
        /// Registra log de sucesso
        /// </summary>
        public void LogSuccess(string mensagem, string? operacao = null)
        {
            lock (_lock)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var originalColor = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"[{timestamp}] ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[SUCCESS] ");

                if (!string.IsNullOrEmpty(operacao))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"[{operacao}] ");
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ {mensagem}");
                Console.ForegroundColor = originalColor;
            }
        }

        /// <summary>
        /// Registra log de erro
        /// </summary>
        public void LogError(string mensagem, Exception? ex = null, string? operacao = null)
        {
            lock (_lock)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var originalColor = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"[{timestamp}] ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR] ");

                if (!string.IsNullOrEmpty(operacao))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"[{operacao}] ");
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ {mensagem}");

                if (ex != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"  Detalhe: {ex.Message}");
                }

                Console.ForegroundColor = originalColor;
            }
        }

        /// <summary>
        /// Registra log de aviso
        /// </summary>
        public void LogWarning(string mensagem, string? operacao = null)
        {
            lock (_lock)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var originalColor = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"[{timestamp}] ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[WARNING] ");

                if (!string.IsNullOrEmpty(operacao))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"[{operacao}] ");
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠ {mensagem}");
                Console.ForegroundColor = originalColor;
            }
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

                Console.Write($"\r[{bar}] {percent}% - {mensagem}");

                if (current == total)
                    Console.WriteLine();
            }
        }
    }
}

