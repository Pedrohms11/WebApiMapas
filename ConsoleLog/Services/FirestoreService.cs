using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using ConsoleLog.Models;


namespace ConsoleLog.Services
{
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

                // Resolver caminho completo
                string resolvedKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, keyPath);

                _logger.LogInfo($"ProjectId: {projectId}", "FIRESTORE");
                _logger.LogInfo($"Arquivo de credencial: {resolvedKeyPath}", "FIRESTORE");

                // Verificar se o arquivo existe
                if (!File.Exists(resolvedKeyPath))
                {
                    throw new FileNotFoundException($"Arquivo de credencial não encontrado: {resolvedKeyPath}");
                }

                // ✅ LER O CONTEÚDO DO ARQUIVO JSON
                var jsonCredentials = File.ReadAllText(resolvedKeyPath);

                // ✅ CRIAR O CLIENTE USANDO O JSON DIRETAMENTE (Método mais confiável)
                var builder = new FirestoreDbBuilder
                {
                    ProjectId = projectId,
                    JsonCredentials = jsonCredentials
                };

                _db = builder.Build();

                _logger.LogSuccess($"Firestore conectado com sucesso - Projeto: {projectId}", "FIRESTORE");
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao conectar ao Firestore", ex, "FIRESTORE");
                throw;
            }
        }

        /// <summary>
        /// Verifica se a conexão com Firestore está ativa
        /// </summary>
        public async Task<bool> VerificarConexao()
        {
            try
            {
                // Tenta listar a primeira coleção para testar a conexão
                var collections = await _db.ListRootCollectionsAsync().Take(1).ToListAsync();
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
        /// Converte documento Firestore para objeto Localizacao
        /// </summary>
        private Localizacao? ConverterDocumentoParaLocalizacao(DocumentSnapshot document)
        {
            try
            {
                if (!document.Exists)
                    return null;

                var dados = document.ToDictionary();
                var localizacao = new Localizacao();

                // ID - vem do DocumentId
                localizacao.Id = document.Id;

                // Logradouro
                localizacao.Logradouro = dados.ContainsKey("Logradouro") && dados["Logradouro"] != null
                    ? dados["Logradouro"].ToString() ?? string.Empty
                    : string.Empty;

                // Numero
                localizacao.Numero = dados.ContainsKey("Numero") && dados["Numero"] != null
                    ? dados["Numero"].ToString() ?? string.Empty
                    : string.Empty;

                // Bairro
                localizacao.Bairro = dados.ContainsKey("Bairro") && dados["Bairro"] != null
                    ? dados["Bairro"].ToString() ?? string.Empty
                    : string.Empty;

                // CEP
                if (dados.ContainsKey("Cep") && dados["Cep"] != null)
                {
                    var cepObj = dados["Cep"];
                    localizacao.Cep = cepObj switch
                    {
                        string cepStr => cepStr,
                        long cepLong => cepLong.ToString("D8"),
                        int cepInt => cepInt.ToString("D8"),
                        _ => cepObj.ToString() ?? string.Empty
                    };
                }
                else
                {
                    localizacao.Cep = string.Empty;
                }

                // Latitude
                if (dados.ContainsKey("Latitude") && dados["Latitude"] != null)
                {
                    try
                    {
                        localizacao.Latitude = Convert.ToDouble(dados["Latitude"]);
                    }
                    catch
                    {
                        localizacao.Latitude = 0;
                    }
                }

                // Longitude
                if (dados.ContainsKey("Longitude") && dados["Longitude"] != null)
                {
                    try
                    {
                        localizacao.Longitude = Convert.ToDouble(dados["Longitude"]);
                    }
                    catch
                    {
                        localizacao.Longitude = 0;
                    }
                }

                // Timestamp
                if (dados.ContainsKey("Timestamp") && dados["Timestamp"] is Timestamp ts)
                {
                    localizacao.Timestamp = ts.ToDateTime();
                }
                else
                {
                    localizacao.Timestamp = DateTime.UtcNow;
                }

                return localizacao;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao converter documento Firestore: {ex.Message}", ex, "FIRESTORE");
                return null;
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