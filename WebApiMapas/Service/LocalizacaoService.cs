using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiMapas.Data;
using WebApiMapas.Models;
using System.Net.Http;
using System.Globalization;

namespace WebApiMapas.Service
{
    /// <summary>
    /// Serviço responsável por gerenciar operações relacionadas a localizações georreferenciadas no Firebase.
    /// </summary>
    public class LocalizacaoService
    {
        private readonly ILogger<LocalizacaoService> _logger;
        private readonly FirestoreDb _firestoreDb;
        private readonly string _collectionName = "localizacoes";
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Construtor da classe com Injeção de Dependência.
        /// Resolvido conflito de IHttpClientFactory especificando o namespace System.Net.Http.
        /// </summary>
        public LocalizacaoService(
            ILogger<LocalizacaoService> logger,
            FirestoreService firestoreService,
            System.Net.Http.IHttpClientFactory httpClientFactory) // Correção do erro CS0104 aqui
        {
            _logger = logger;
            _firestoreDb = firestoreService.Db;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WebApiMapas-Squad2");
        }

        public async Task<List<Localizacao>> Listar()
        {
            CollectionReference collectionRef = _firestoreDb.Collection(_collectionName); // Referência para a coleção "localizacoes" no Firebase
            QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync(); // Executa a consulta para obter todos os documentos da coleção

            List<Localizacao> lista = new List<Localizacao>(); // Iniciando uma lista para armazenar as localizações convertidas

            foreach (DocumentSnapshot document in snapshot.Documents) // Percorre cada item retornado pela consulta
            {
                if (document.Exists) // Verifica se o documento existe antes de tentar convertê-lo
                {
                    var item = document.ConvertTo<Localizacao>(); // Converte o documento do Firebase para a classe Localizacao usando o método de extensão ConvertTo
                    item.Id = document.Id; // Forçando o ID do documento para ser o ID da localização, garantindo que tenhamos acesso ao identificador único do Firebase
                    lista.Add(item); // Adiciona a localização convertida à lista de resultados
                }
            }

            return lista; // Retorna a lista de localizações obtidas do Firebase
        }

        public async Task<Localizacao?> ObterPorId(string id)
        {
            // Referência para o documento específico com o ID fornecido
            DocumentReference docRef = _firestoreDb.Collection(_collectionName).Document(id);

            // Executa a consulta para obter o documento do Firebase
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                var localizacao = snapshot.ConvertTo<Localizacao>();
                localizacao.Id = snapshot.Id;
                return localizacao;
            }

            return null;
        }

        /// <summary>
        /// Listar uma localização por logradouro buscando direto no Firebase
        /// </summary>
        /// <param name="logradouro"></param>
        /// <returns></returns>
        public async Task<Localizacao?> ObterPorLogradouro(string logradouro)
        {
            Query query = _firestoreDb.Collection(_collectionName);

            // Obtém todos os documentos da coleção
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            // Verifica se há documentos retornados pela consulta
            if (snapshot.Documents.Count > 0)
            {
                // Converte o primeiro documento encontrado para a classe Localizacao
                var document = snapshot.Documents[0];

                // Convertendo o documento para a classe Localizacao usando o método de extensão ConvertTo
                var localizacao = document.ConvertTo<Localizacao>();

                // Forçando o ID do documento para ser o ID da localização, garantindo
                // que tenhamos acesso ao identificador único do Firebase
                localizacao.Id = document.Id;

                return localizacao;
            }
            return null;
        }

        public async Task<Localizacao> Criar(Localizacao localizacao)
        {
            // Validações de limites
            if (localizacao.Latitude < -90 || localizacao.Latitude > 90)
                throw new ArgumentException("A latitude deve estar entre -90 e 90 graus.");

            if (localizacao.Longitude < -180 || localizacao.Longitude > 180)
                throw new ArgumentException("A longitude deve estar entre -180 e 180 graus.");

            // Validação de existência da coordenada via OpenStreetMap
            bool coordenadaExiste = await 
                ValidarCoordenadasNoMundoReal(localizacao.Latitude, localizacao.Longitude);
            if (!coordenadaExiste)
            {
                _logger.LogWarning($"Tentativa de cadastro de coordenada inexistente:" +
                    $" {localizacao.Latitude}, {localizacao.Longitude}");
                throw new
                    ArgumentException("As coordenadas fornecidas não correspondem a um local " +
                    "válido no globo terrestre.");
            }

            // Gerar ID Sequencial via Transação no Firebase
            DocumentReference contadorRef = 
                _firestoreDb.Collection("configuracoes").Document("contador_localizacoes");

            int novoIdNumerico = await
                _firestoreDb.RunTransactionAsync(async transaction =>
            {
                DocumentSnapshot snapshot = await
                transaction.GetSnapshotAsync(contadorRef);

                int idAtual = 0;

                if (snapshot.Exists)
                {
                    snapshot.TryGetValue("ultimoId", out idAtual);
                }

                int proximoId = idAtual + 1;

                Dictionary<string, object> atualizacaoContador = new Dictionary<string, object>
                {
                    { "ultimoId", proximoId }
                };

                transaction.Set(contadorRef, atualizacaoContador, SetOptions.MergeAll);
                return proximoId;
            });

            // Persistência com o ID tratado
            string idString = novoIdNumerico.ToString();
            DocumentReference docRef = _firestoreDb.Collection(_collectionName).Document(idString);

            localizacao.Id = idString;
            await docRef.SetAsync(localizacao);

            return localizacao;
        }

        public async Task Atualizar(string id, Localizacao existente)
        {
            DocumentReference docRef = _firestoreDb.Collection(_collectionName).Document(id);
            await docRef.SetAsync(existente, SetOptions.MergeAll);
        }

        public async Task Delete(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection(_collectionName).Document(id);
            await docRef.DeleteAsync();
        }

        /// <summary>
        /// Este método atua como validador da coordenadas no mapa mundial se as 
        /// coordenadas enviadas realmente existem ou se são apenas números aleatórios.
        /// </summary>
        private async Task<bool> ValidarCoordenadasNoMundoReal(double latitude, double longitude)
        {
            try
            {                
                // Conversão para o padrão internacional mudança de , para .
                var latitudeTexto = latitude.ToString(CultureInfo.InvariantCulture);
                var longitudeTexto = longitude.ToString(CultureInfo.InvariantCulture);

                // Construção da URL de consulta para o serviço Nominatim do OpenStreetMap,
                // que é um serviço de geocodificação reversa.
                var urlDaConsulta = 
                    $"https://nominatim.openstreetmap.org/reverse?format=json&lat=" +
                    $"{latitudeTexto}&lon={longitudeTexto}";

                // Envio da requisição HTTP para o serviço de geocodificação reversa.
                var respostaDaInternet = await _httpClient.GetAsync(urlDaConsulta);

                // Verificação de resposta diferente de sucesso (ex: 404, 500)
                if (!respostaDaInternet.IsSuccessStatusCode)
                    return false;

                // Leitura do conteúdo da resposta convertido em string
                var corpoDaResposta = await 
                    respostaDaInternet.Content.ReadAsStringAsync();

                // OpenStreetMap: responde "com sucesso" (HTTP 200), mesmo quando não encontra o lugar,
                // mas escreve a palavra "error", portanto caso a resposta não contenha a palavra "error",
                // ele retorna o endereço encontrado, caso contrário,
                // retorna falso indicando que as coordenadas não correspondem a um local válido.             
                bool enderecoEncontrado = !corpoDaResposta.Contains("\"error\"");

                return enderecoEncontrado;
            }
            catch (Exception erro)
            {
                // Plano B: Se a validação falhar por algum motivo (ex: sem internet),
                //  permite o cadastro, mas registra o erro para análise futura.
                _logger.LogError(
                    $"Não conseguimos validar o local agora. Motivo: {erro.Message}");
                return true;
            }
        }
    }
}