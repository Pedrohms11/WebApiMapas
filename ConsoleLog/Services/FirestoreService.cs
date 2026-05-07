using ConsoleLog.Models;
using Microsoft.Extensions.Configuration;


namespace ConsoleLog.Services
{
    /// <summary>
    /// Serviço de integração com Firebase Cloud Firestore - APENAS LEITURA
    /// </summary>
    public class FirestoreService
    {
        private readonly FirestoreDb _db;
        private readonly LogService _logger;
        private readonly string _colecaoName = "localizacoes";

        public FirestoreService(IConfiguration configuration, LogService logger)
        {
            _logger = logger;

            try
            {
                var projectId = configuration["Firebase:ProjectId"];
                var keyPath = configuration["Firebase:KeyFilePath"];

                if (string.IsNullOrEmpty(projectId))
                    throw new Exception("ProjectId não configurado no appsettings.json");

                if (string.IsNullOrEmpty(keyPath))
                    throw new Exception("KeyFilePath não configurado no appsettings.json");

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);
                _db = FirestoreDb.Create(projectId);

                _logger.LogInfo($"Firestore conectado com sucesso - Projeto: {projectId}", "FIRESTORE");
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao conectar ao Firestore", ex, "FIRESTORE");
                throw;
            }
        }

        /// <summary>
        /// Busca todas as localizações do Firestore
        /// </summary>
        public async Task<List<Localizacao>> BuscarTodasLocalizacoes()
        {
            try
            {
                _logger.LogInfo("Iniciando busca de todas localizações no Firestore", "FIRESTORE");

                Query query = _db.Collection(_colecaoName);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                var localizacoes = new List<Localizacao>();
                foreach (var document in snapshot.Documents)
                {
                    var localizacao = ConverterDocumentoParaLocalizacao(document);
                    if (localizacao != null)
                        localizacoes.Add(localizacao);
                }

                _logger.LogSuccess($"Buscadas {localizacoes.Count} localizações do Firestore", "FIRESTORE");
                return localizacoes.OrderByDescending(l => l.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao buscar todas as localizações no Firestore", ex, "FIRESTORE");
                return new List<Localizacao>();
            }
        }

        /// <summary>
        /// Busca localização por ID no Firestore
        /// </summary>
        public async Task<Localizacao?> BuscarLocalizacaoPorId(int id)
        {
            try
            {
                DocumentReference docRef = _db.Collection(_colecaoName).Document(id.ToString());
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    _logger.LogInfo($"Localização {id} encontrada no Firestore", "FIRESTORE");
                    return ConverterDocumentoParaLocalizacao(snapshot);
                }

                _logger.LogWarning($"Localização {id} não encontrada no Firestore", "FIRESTORE");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar localização {id} no Firestore", ex, "FIRESTORE");
                return null;
            }
        }

        /// <summary>
        /// Busca localizações por CEP no Firestore
        /// </summary>
        public async Task<List<Localizacao>> BuscarLocalizacoesPorCep(string cep)
        {
            try
            {
                Query query = _db.Collection(_colecaoName).WhereEqualTo("Cep", cep);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                var localizacoes = new List<Localizacao>();
                foreach (var document in snapshot.Documents)
                {
                    var localizacao = ConverterDocumentoParaLocalizacao(document);
                    if (localizacao != null)
                        localizacoes.Add(localizacao);
                }

                _logger.LogInfo($"Buscadas {localizacoes.Count} localizações com CEP {cep}", "FIRESTORE");
                return localizacoes;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar localizações por CEP {cep} no Firestore", ex, "FIRESTORE");
                return new List<Localizacao>();
            }
        }

        /// <summary>
        /// Busca localizações por bairro no Firestore
        /// </summary>
        public async Task<List<Localizacao>> BuscarLocalizacoesPorBairro(string bairro)
        {
            try
            {
                Query query = _db.Collection(_colecaoName).WhereEqualTo("Bairro", bairro);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                var localizacoes = new List<Localizacao>();
                foreach (var document in snapshot.Documents)
                {
                    var localizacao = ConverterDocumentoParaLocalizacao(document);
                    if (localizacao != null)
                        localizacoes.Add(localizacao);
                }

                _logger.LogInfo($"Buscadas {localizacoes.Count} localizações no bairro {bairro}", "FIRESTORE");
                return localizacoes;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar localizações por bairro {bairro}", ex, "FIRESTORE");
                return new List<Localizacao>();
            }
        }

        /// <summary>
        /// Busca localizações por período no Firestore
        /// </summary>
        public async Task<List<Localizacao>> BuscarLocalizacoesPorPeriodo(DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                var startTimestamp = Timestamp.FromDateTime(dataInicio.ToUniversalTime());
                var endTimestamp = Timestamp.FromDateTime(dataFim.ToUniversalTime());

                Query query = _db.Collection(_colecaoName)
                    .WhereGreaterThanOrEqualTo("Timestamp", startTimestamp)
                    .WhereLessThanOrEqualTo("Timestamp", endTimestamp);

                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                var localizacoes = new List<Localizacao>();
                foreach (var document in snapshot.Documents)
                {
                    var localizacao = ConverterDocumentoParaLocalizacao(document);
                    if (localizacao != null)
                        localizacoes.Add(localizacao);
                }

                _logger.LogInfo($"Buscadas {localizacoes.Count} localizações no período", "FIRESTORE");
                return localizacoes;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar localizações por período", ex, "FIRESTORE");
                return new List<Localizacao>();
            }
        }

        /// <summary>
        /// Converte documento Firestore para objeto Localizacao
        /// </summary>
        private Localizacao? ConverterDocumentoParaLocalizacao(DocumentSnapshot document)
        {
            try
            {
                if (!document.Exists)
                    return null;

                var dados = document.ToDictionary();

                // Converte Timestamp do Firestore para DateTime
                DateTime timestamp = DateTime.UtcNow;
                if (dados.ContainsKey("Timestamp") && dados["Timestamp"] is Timestamp ts)
                {
                    timestamp = ts.ToDateTime();
                }

                return new Localizacao
                {
                    Id = Convert.ToInt32(document.Id),
                    Logradouro = dados.ContainsKey("Logradouro") ? dados["Logradouro"]?.ToString() ?? string.Empty : string.Empty,
                    Numero = dados.ContainsKey("Numero") ? dados["Numero"]?.ToString() ?? string.Empty : string.Empty,
                    Bairro = dados.ContainsKey("Bairro") ? dados["Bairro"]?.ToString() ?? string.Empty : string.Empty,
                    Cep = dados.ContainsKey("Cep") ? dados["Cep"]?.ToString() ?? string.Empty : string.Empty,
                    Latitude = dados.ContainsKey("Latitude") ? Convert.ToDouble(dados["Latitude"]) : 0,
                    Longitude = dados.ContainsKey("Longitude") ? Convert.ToDouble(dados["Longitude"]) : 0,
                    Timestamp = timestamp
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao converter documento Firestore: {ex.Message}", ex, "FIRESTORE");
                return null;
            }
        }

        /// <summary>
        /// Verifica se a conexão com Firestore está ativa
        /// </summary>
        public async Task<bool> VerificarConexao()
        {
            try
            {
                var collections = await _db.ListRootCollectionsAsync().FirstOrDefaultAsync();
                _logger.LogSuccess("Conexão com Firestore verificada com sucesso", "FIRESTORE");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Falha na conexão com Firestore", ex, "FIRESTORE");
                return false;
            }
        }

        /// <summary>
        /// Obtém estatísticas do Firestore
        /// </summary>
        public async Task<FirestoreStats> ObterEstatisticas()
        {
            try
            {
                var todas = await BuscarTodasLocalizacoes();

                return new FirestoreStats
                {
                    TotalRegistros = todas.Count,
                    UltimaAtualizacao = todas.Any() ? todas.Max(l => l.Timestamp) : DateTime.MinValue,
                    BairrosUnicos = todas.Select(l => l.Bairro).Distinct().Count(),
                    CepsUnicos = todas.Select(l => l.Cep).Distinct().Count()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao obter estatísticas do Firestore", ex, "FIRESTORE");
                return new FirestoreStats();
            }
        }
    }

    /// <summary>
    /// Estatísticas do Firestore
    /// </summary>
    public class FirestoreStats
    {
        public int TotalRegistros { get; set; }
        public DateTime UltimaAtualizacao { get; set; }
        public int BairrosUnicos { get; set; }
        public int CepsUnicos { get; set; }
    }

}
