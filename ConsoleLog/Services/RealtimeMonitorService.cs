using Google.Cloud.Firestore;
using System.Text.Json;
using ConsoleLog.Models;

namespace ConsoleLog.Services
{
    /// <summary>
    /// Serviço de monitoramento em tempo real do Firebase Firestore
    /// </summary>
    public class RealtimeMonitorService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly AuditoriaService _auditoriaService;
        private readonly LogService _logger;
        private readonly string _colecaoName = "localizacoes";
        private IDisposable? _listener;
        private Dictionary<string, Localizacao> _ultimoEstadoConhecido = new();

        public RealtimeMonitorService(FirestoreService firestoreService, AuditoriaService auditoriaService, LogService logger)
        {
            _firestoreDb = firestoreService.GetDb();
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        /// <summary>
        /// Inicia o monitoramento em tempo real do Firestore
        /// </summary>
        public async Task IniciarMonitoramento()
        {
            try
            {
                _logger.LogInfo("🚀 Iniciando monitoramento em tempo real do Firebase...", "MONITOR");

                // Carregar estado atual
                await CarregarEstadoAtual();

                // Configurar listener para mudanças em tempo real
                var collectionRef = _firestoreDb.Collection(_colecaoName);
                _listener = collectionRef.Listen(snapshot =>
                {
                    Task.Run(() => ProcessarMudancas(snapshot));
                });

                _logger.LogSuccess("✅ Monitoramento em tempo real ativado!", "MONITOR");
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Erro ao iniciar monitoramento", ex, "MONITOR");
            }
        }

        /// <summary>
        /// Para o monitoramento em tempo real
        /// </summary>
        public void PararMonitoramento()
        {
            _listener?.Dispose();
            _logger.LogInfo("⏹️ Monitoramento em tempo real parado.", "MONITOR");
        }

        /// <summary>
        /// Carrega o estado atual de todos os documentos
        /// </summary>
        private async Task CarregarEstadoAtual()
        {
            var snapshot = await _firestoreDb.Collection(_colecaoName).GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                var localizacao = ConverterDocumentoParaLocalizacao(doc);
                if (localizacao != null)
                {
                    _ultimoEstadoConhecido[doc.Id] = localizacao;
                }
            }

            _logger.LogInfo($"📊 Estado inicial carregado: {_ultimoEstadoConhecido.Count} registros", "MONITOR");
        }

        /// <summary>
        /// Processa as mudanças detectadas no Firestore
        /// </summary>
        private async Task ProcessarMudancas(QuerySnapshot snapshot)
        {
            var documentosAtuais = snapshot.Documents.ToDictionary(d => d.Id);

            // Verificar documentos removidos
            foreach (var id in _ultimoEstadoConhecido.Keys.ToList())
            {
                if (!documentosAtuais.ContainsKey(id))
                {
                    await ProcessarExclusao(id, _ultimoEstadoConhecido[id]);
                    _ultimoEstadoConhecido.Remove(id);
                }
            }

            // Verificar documentos adicionados/alterados
            foreach (var doc in snapshot.Documents)
            {
                var localizacaoAtual = ConverterDocumentoParaLocalizacao(doc);
                if (localizacaoAtual == null) continue;

                if (!_ultimoEstadoConhecido.ContainsKey(doc.Id))
                {
                    // Documento novo
                    await ProcessarInsercao(doc.Id, localizacaoAtual);
                    _ultimoEstadoConhecido[doc.Id] = localizacaoAtual;
                }
                else
                {
                    // Documento existente - verificar mudanças
                    var localizacaoAnterior = _ultimoEstadoConhecido[doc.Id];
                    var mudancas = CompararLocalizacoes(localizacaoAnterior, localizacaoAtual);

                    if (!string.IsNullOrEmpty(mudancas))
                    {
                        await ProcessarAtualizacao(doc.Id, localizacaoAnterior, localizacaoAtual, mudancas);
                        _ultimoEstadoConhecido[doc.Id] = localizacaoAtual;
                    }
                }
            }
        }

        /// <summary>
        /// Processa uma inserção detectada
        /// </summary>
        private async Task ProcessarInsercao(string id, Localizacao nova)
        {
            var dadosNovos = JsonSerializer.Serialize(nova, new JsonSerializerOptions { WriteIndented = true });

            await _auditoriaService.RegistrarInsercao(
                tabela: "Localizacoes",
                registroId: id,
                dadosNovos: dadosNovos,
                origem: "Firebase Realtime"
            );
        }

        /// <summary>
        /// Processa uma atualização detectada
        /// </summary>
        private async Task ProcessarAtualizacao(string id, Localizacao antiga, Localizacao nova, string mudancas)
        {
            var dadosAntigos = JsonSerializer.Serialize(antiga, new JsonSerializerOptions { WriteIndented = true });
            var dadosNovos = JsonSerializer.Serialize(nova, new JsonSerializerOptions { WriteIndented = true });

            await _auditoriaService.RegistrarAtualizacao(
                tabela: "Localizacoes",
                registroId: id,
                dadosAntigos: dadosAntigos,
                dadosNovos: dadosNovos,
                mudancas: mudancas,
                origem: "Firebase Realtime"
            );
        }

        /// <summary>
        /// Processa uma exclusão detectada
        /// </summary>
        private async Task ProcessarExclusao(string id, Localizacao removida)
        {
            var dadosAntigos = JsonSerializer.Serialize(removida, new JsonSerializerOptions { WriteIndented = true });

            await _auditoriaService.RegistrarExclusao(
                tabela: "Localizacoes",
                registroId: id,
                dadosAntigos: dadosAntigos,
                origem: "Firebase Realtime"
            );
        }

        /// <summary>
        /// Compara duas localizações e retorna as mudanças
        /// </summary>
        private string CompararLocalizacoes(Localizacao antiga, Localizacao nova)
        {
            var mudancas = new List<string>();

            if (antiga.Logradouro != nova.Logradouro)
                mudancas.Add($"Logradouro: '{antiga.Logradouro}' → '{nova.Logradouro}'");

            if (antiga.Numero != nova.Numero)
                mudancas.Add($"Numero: '{antiga.Numero}' → '{nova.Numero}'");

            if (antiga.Bairro != nova.Bairro)
                mudancas.Add($"Bairro: '{antiga.Bairro}' → '{nova.Bairro}'");

            if (antiga.Cep != nova.Cep)
                mudancas.Add($"Cep: '{antiga.Cep}' → '{nova.Cep}'");

            if (Math.Abs(antiga.Latitude - nova.Latitude) > 0.0001)
                mudancas.Add($"Latitude: {antiga.Latitude} → {nova.Latitude}");

            if (Math.Abs(antiga.Longitude - nova.Longitude) > 0.0001)
                mudancas.Add($"Longitude: {antiga.Longitude} → {nova.Longitude}");

            return mudancas.Any() ? string.Join("; ", mudancas) : "";
        }

        /// <summary>
        /// Converte documento Firestore para Localizacao
        /// </summary>
        private Localizacao? ConverterDocumentoParaLocalizacao(DocumentSnapshot doc)
        {
            if (!doc.Exists) return null;

            var dados = doc.ToDictionary();

            return new Localizacao
            {
                Id = doc.Id,
                Logradouro = dados.ContainsKey("Logradouro") ? dados["Logradouro"]?.ToString() ?? "" : "",
                Numero = dados.ContainsKey("Numero") ? dados["Numero"]?.ToString() ?? "" : "",
                Bairro = dados.ContainsKey("Bairro") ? dados["Bairro"]?.ToString() ?? "" : "",
                Cep = dados.ContainsKey("Cep") ? dados["Cep"]?.ToString() ?? "" : "",
                Latitude = dados.ContainsKey("Latitude") ? Convert.ToDouble(dados["Latitude"]) : 0,
                Longitude = dados.ContainsKey("Longitude") ? Convert.ToDouble(dados["Longitude"]) : 0,
                Timestamp = dados.ContainsKey("Timestamp") && dados["Timestamp"] is Timestamp ts
                    ? ts.ToDateTime()
                    : DateTime.UtcNow
            };
        }
    }
}