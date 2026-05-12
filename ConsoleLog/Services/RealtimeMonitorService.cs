using Google.Cloud.Firestore;
using System.Text.Json;
using ConsoleLog.Models;

namespace ConsoleLog.Services
{
    /// <summary>
    /// Serviço responsável pelo monitoramento em tempo real de alterações no Firestore
    /// </summary>
    public class RealtimeMonitorService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly AuditoriaService _auditoriaService;
        private readonly LogService _logger;
        private readonly string _colecaoName = "localizacoes";
        private FirestoreChangeListener? _listener;
        private Dictionary<string, Localizacao> _ultimoEstadoConhecido = new();

        /// <summary>
        /// Inicializa uma nova instância do serviço de monitoramento em tempo real
        /// </summary>
        /// <param name="firestoreService">Serviço de conexão com Firestore</param>
        /// <param name="auditoriaService">Serviço de auditoria para registrar as alterações</param>
        /// <param name="logger">Serviço de logging do sistema</param>
        public RealtimeMonitorService(FirestoreService firestoreService, AuditoriaService auditoriaService, LogService logger)
        {
            _firestoreDb = firestoreService.GetDb();
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        /// <summary>
        /// Inicia o monitoramento em tempo real da coleção de localizações no Firestore
        /// </summary>
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
            catch (Exception ex)
            {
                _logger.LogError("Erro ao iniciar monitoramento", ex, "MONITOR");
            }
        }

        /// <summary>
        /// Para o monitoramento em tempo real e libera os recursos do listener
        /// </summary>
        public void PararMonitoramento()
        {
            if (_listener != null)
            {
                _listener.StopAsync();
                _listener = null;
            }
            _logger.LogInfo("Monitoramento parado.", "MONITOR");
        }

        /// <summary>
        /// Carrega o estado atual de todas as localizações do Firestore
        /// </summary>
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

        /// <summary>
        /// Processa as mudanças detectadas no snapshot do Firestore
        /// </summary>
        /// <param name="snapshot">Snapshot contendo o estado atual da coleção</param>
        private async Task ProcessarMudancas(QuerySnapshot snapshot)
        {
            var docsAtuais = snapshot.Documents.ToDictionary(d => d.Id);

            // Verificar documentos removidos
            foreach (var id in _ultimoEstadoConhecido.Keys.ToList())
                if (!docsAtuais.ContainsKey(id))
                    await ProcessarExclusao(id, _ultimoEstadoConhecido[id]);

            // Verificar documentos novos ou alterados
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

        /// <summary>
        /// Processa uma inserção detectada no Firestore
        /// </summary>
        /// <param name="id">Identificador do documento inserido</param>
        /// <param name="nova">Objeto Localizacao com os novos dados</param>
        private async Task ProcessarInsercao(string id, Localizacao nova)
        {
            var dados = JsonSerializer.Serialize(nova, new JsonSerializerOptions { WriteIndented = true });
            await _auditoriaService.RegistrarInsercao("Localizacoes", id, dados, "Firebase Realtime");
            _logger.LogInsert($"Nova localização detectada: ID={id}", "REALTIME");
        }

        /// <summary>
        /// Processa uma atualização detectada no Firestore
        /// </summary>
        /// <param name="id">Identificador do documento atualizado</param>
        /// <param name="antiga">Objeto Localizacao com os dados anteriores</param>
        /// <param name="nova">Objeto Localizacao com os dados atuais</param>
        /// <param name="mudancas">Descrição das mudanças detectadas</param>
        private async Task ProcessarAtualizacao(string id, Localizacao antiga, Localizacao nova, string mudancas)
        {
            var antigos = JsonSerializer.Serialize(antiga, new JsonSerializerOptions { WriteIndented = true });
            var novos = JsonSerializer.Serialize(nova, new JsonSerializerOptions { WriteIndented = true });
            await _auditoriaService.RegistrarAtualizacao("Localizacoes", id, antigos, novos, mudancas, "Firebase Realtime");
            _logger.LogUpdate($"Localização atualizada: ID={id} | {mudancas}", "REALTIME");
        }

        /// <summary>
        /// Processa uma exclusão detectada no Firestore
        /// </summary>
        /// <param name="id">Identificador do documento excluído</param>
        /// <param name="removida">Objeto Localizacao com os dados removidos</param>
        private async Task ProcessarExclusao(string id, Localizacao removida)
        {
            var dados = JsonSerializer.Serialize(removida, new JsonSerializerOptions { WriteIndented = true });
            await _auditoriaService.RegistrarExclusao("Localizacoes", id, dados, "Firebase Realtime");
            _logger.LogDelete($"Localização removida: ID={id}", "REALTIME");
        }

        /// <summary>
        /// Compara dois objetos Localizacao e retorna as diferenças encontradas
        /// </summary>
        /// <param name="a">Objeto anterior</param>
        /// <param name="b">Objeto atual</param>
        /// <returns>String com as diferenças no formato "campo: 'valor_antigo' → 'valor_novo'"</returns>
        private string Comparar(Localizacao a, Localizacao b)
        {
            var mudancas = new List<string>();
            if (a.Logradouro != b.Logradouro) mudancas.Add($"Logradouro: '{a.Logradouro}' → '{b.Logradouro}'");
            if (a.Numero != b.Numero) mudancas.Add($"Numero: '{a.Numero}' → '{b.Numero}'");
            if (a.Bairro != b.Bairro) mudancas.Add($"Bairro: '{a.Bairro}' → '{b.Bairro}'");
            if (a.Cep != b.Cep) mudancas.Add($"Cep: '{a.Cep}' → '{b.Cep}'");
            return string.Join("; ", mudancas);
        }

        /// <summary>
        /// Converte um documento do Firestore para o objeto Localizacao
        /// </summary>
        /// <param name="doc">Snapshot do documento Firestore</param>
        /// <returns>Objeto Localizacao populado ou null se o documento não existir</returns>
        private Localizacao? ConverterDocumento(DocumentSnapshot doc)
        {
            if (!doc.Exists) return null;
            var dados = doc.ToDictionary();

            // Converter o Timestamp do Firestore para DateTime
            DateTime timestamp = DateTime.UtcNow;
            if (dados.ContainsKey("Timestamp") && dados["Timestamp"] is Timestamp ts)
            {
                timestamp = ts.ToDateTime();
            }

            return new Localizacao
            {
                Id = doc.Id,
                Logradouro = dados.GetValueOrDefault("Logradouro")?.ToString() ?? "",
                Numero = dados.GetValueOrDefault("Numero")?.ToString() ?? "",
                Bairro = dados.GetValueOrDefault("Bairro")?.ToString() ?? "",
                Cep = dados.GetValueOrDefault("Cep")?.ToString() ?? "",
                Latitude = dados.ContainsKey("Latitude") ? Convert.ToDouble(dados["Latitude"] ?? 0) : 0,
                Longitude = dados.ContainsKey("Longitude") ? Convert.ToDouble(dados["Longitude"] ?? 0) : 0,
                Timestamp = timestamp
            };
        }
    }
}