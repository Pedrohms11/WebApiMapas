using Microsoft.Extensions.Logging;

private async Task VerMonitorRequisicoesTempoReal()
{
    Console.Clear();
    Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║              MONITOR DE REQUISIÇÕES EM TEMPO REAL                             ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝\n");

    Console.WriteLine("🟢 Monitor ativo - Pressione 'Q' para sair\n");
    Console.WriteLine(new string('═', 100));

    var ultimosLogs = await _viewModel.ObterTodosLogsRequisicao();
    var ultimosIds = ultimosLogs.Select(l => l.Id).ToHashSet();

    var tokenSource = new CancellationTokenSource();

    var task = Task.Run(async () =>
    {
        while (!tokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var novosLogs = await _viewModel.ObterLogsRequisicaoRecentes(ultimosIds);

                foreach (var log in novosLogs)
                {
                    var corOriginal = Console.ForegroundColor;

                    // ✅ CORREÇÃO: Usar 'Sucesso' (com S maiúsculo) ou verificar o nome correto da propriedade
                    // Verifique se a propriedade se chama 'Sucesso' ou 'Sucess'
                    bool isSuccess = log.Sucesso;  // Ou log.Success dependendo do seu modelo

                    if (isSuccess)
                        Console.ForegroundColor = ConsoleColor.Green;
                    else
                        Console.ForegroundColor = ConsoleColor.Red;

                    var icon = isSuccess ? "✅" : "❌";
                    var categoriaIcon = log.Categoria switch
                    {
                        "Leitura" => "📖",
                        "Escrita" => "✍️",
                        "Sincronizacao" => "🔄",
                        _ => "📡"
                    };

                    Console.WriteLine($"{icon} {categoriaIcon} [{log.DataHora:HH:mm:ss}] {log.Operacao} {log.Endpoint}");
                    Console.WriteLine($"   ⏱️  {log.DuracaoMs}ms | 👤 {log.Usuario} | 📡 {log.Origem} | 🔢 Status: {log.StatusCode}");

                    if (!string.IsNullOrEmpty(log.MensagemErro))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"   ❌ Erro: {log.MensagemErro}");
                    }

                    Console.ForegroundColor = corOriginal;
                    Console.WriteLine(new string('─', 100));

                    ultimosIds.Add(log.Id);
                }

                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                // Ignorar erros durante monitoramento para não parar o loop
                await Task.Delay(1000);
            }
        }
    });

    while (Console.ReadKey(true).Key != ConsoleKey.Q) { }

    tokenSource.Cancel();
    await task;

    Console.Clear();
    Logger.LogInfo("Monitoramento encerrado.", "VIEW");
}