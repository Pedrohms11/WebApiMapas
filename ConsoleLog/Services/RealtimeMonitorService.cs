using Google.Cloud.Firestore;
using System.Text.Json;
using ConsoleLog.Models;

namespace ConsoleLog.Services
{
    public class RealtimeMonitorService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly AuditoriaService _auditoriaService;
        private readonly LogService _logger;
        private readonly string _colecaoName = "localizacoes";
        private FirestoreChangeListener? _listener;
        private Dictionary<string, Localizacao> _ultimoEstadoConhecido = new();

        public RealtimeMonitorService(FirestoreService firestoreService, AuditoriaService auditoriaService, LogService logger)
        {
            _firestoreDb = firestoreService.GetDb();
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        public async Task IniciarMonitoramento()
        {
            try
            {
                _logger.LogInfo("Iniciando monitoramento em tempo real...", "MONITOR");
                await CarregarEstadoAtual();

                var collectionRef = _firestoreDb.Collection(_colecaoName);
                _listener = collectionRef.Listen(snapshot => Task.Run(() => ProcessarMudancas(snapshot)));

                _logger.LogSuccess("Monitoramento em tempo real ativado!", "MONITOR");
            }
            catch (Exception ex) { _logger.LogError("Erro ao iniciar monitoramento", ex, "MONITOR"); }
        }

        private async Task CarregarEstadoAtual()
        {
            var snapshot = await _firestoreDb.Collection(_colecaoName).GetSnapshotAsync();
            _ultimoEstadoConhecido.Clear();
            foreach (var doc in snapshot.Documents)
            {
                var loc = ConverterDocumento(doc);
                if (loc != null) _ultimoEstadoConhecido[doc.Id] = loc;
            }
            _logger.LogInfo($"Estado inicial: {_ultimoEstadoConhecido.Count} registros", "MONITOR");
        }

        private async Task ProcessarMudancas(QuerySnapshot snapshot)
        {
            var docsAtuais = snapshot.Documents.ToDictionary(d => d.Id);
            foreach (var id in _ultimoEstadoConhecido.Keys.ToList())
                if (!docsAtuais.ContainsKey(id))
                    await ProcessarExclusao(id, _ultimoEstadoConhecido[id]);

            foreach (var doc in snapshot.Documents)
            {
                var atual = ConverterDocumento(doc);
                if (atual == null) continue;

                if (!_ultimoEstadoConhecido.ContainsKey(doc.Id))
                    await ProcessarInsercao(doc.Id, atual);
                else
                {
                    var anterior = _ultimoEstadoConhecido[doc.Id];
                    var mudancas = Comparar(anterior, atual);
                    if (!string.IsNullOrEmpty(mudancas))
                        await ProcessarAtualizacao(doc.Id, anterior, atual, mudancas);
                }
                _ultimoEstadoConhecido[doc.Id] = atual;
            }
        }

        private async Task ProcessarInsercao(string id, Localizacao nova)
        {
            var dados = JsonSerializer.Serialize(nova, new JsonSerializerOptions { WriteIndented = true });
            await _auditoriaService.RegistrarInsercao("Localizacoes", id, dados, "Firebase Realtime");
            _logger.LogInsert($"Nova localização detectada: ID={id}", "REALTIME");
        }

        private async Task ProcessarAtualizacao(string id, Localizacao antiga, Localizacao nova, string mudancas)
        {
            var antigos = JsonSerializer.Serialize(antiga, new JsonSerializerOptions { WriteIndented = true });
            var novos = JsonSerializer.Serialize(nova, new JsonSerializerOptions { WriteIndented = true });
            await _auditoriaService.RegistrarAtualizacao("Localizacoes", id, antigos, novos, mudancas, "Firebase Realtime");
            _logger.LogUpdate($"Localização atualizada: ID={id} | {mudancas}", "REALTIME");
        }

        private async Task ProcessarExclusao(string id, Localizacao removida)
        {
            var dados = JsonSerializer.Serialize(removida, new JsonSerializerOptions { WriteIndented = true });
            await _auditoriaService.RegistrarExclusao("Localizacoes", id, dados, "Firebase Realtime");
            _logger.LogDelete($"Localização removida: ID={id}", "REALTIME");
        }

        private string Comparar(Localizacao a, Localizacao b)
        {
            var mudancas = new List<string>();
            if (a.Logradouro != b.Logradouro) mudancas.Add($"Logradouro: '{a.Logradouro}' → '{b.Logradouro}'");
            if (a.Numero != b.Numero) mudancas.Add($"Numero: '{a.Numero}' → '{b.Numero}'");
            if (a.Bairro != b.Bairro) mudancas.Add($"Bairro: '{a.Bairro}' → '{b.Bairro}'");
            if (a.Cep != b.Cep) mudancas.Add($"Cep: '{a.Cep}' → '{b.Cep}'");
            return string.Join("; ", mudancas);
        }

        private Localizacao? ConverterDocumento(DocumentSnapshot doc)
        {
            if (!doc.Exists) return null;
            var dados = doc.ToDictionary();
            return new Localizacao
            {
                Id = doc.Id,
                Logradouro = dados.GetValueOrDefault("Logradouro")?.ToString() ?? "",
                Numero = dados.GetValueOrDefault("Numero")?.ToString() ?? "",
                Bairro = dados.GetValueOrDefault("Bairro")?.ToString() ?? "",
                Cep = dados.GetValueOrDefault("Cep")?.ToString() ?? "",
                Latitude = dados.ContainsKey("Latitude") ? Convert.ToDouble(dados["Latitude"] ?? 0) : 0,
                Longitude = dados.ContainsKey("Longitude") ? Convert.ToDouble(dados["Longitude"] ?? 0) : 0,
                Timestamp = dados.ContainsKey("Timestamp") && dados["Timestamp"] is Timestamp ts ? ts.ToDateTime() : DateTime.UtcNow
            };
        }

        public void PararMonitoramento() { _listener?.Dispose(); _logger.LogInfo("Monitoramento parado.", "MONITOR"); }
    }
}