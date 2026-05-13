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
            CollectionReference collectionRef = _firestoreDb.Collection(_collectionName);
            QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();

            List<Localizacao> lista = new List<Localizacao>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var item = document.ConvertTo<Localizacao>();
                    item.Id = document.Id;
                    lista.Add(item);
                }
            }
            return lista;
        }

        public async Task<Localizacao?> ObterPorId(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection(_collectionName).Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                var localizacao = snapshot.ConvertTo<Localizacao>();
                localizacao.Id = snapshot.Id;
                return localizacao;
            }
            return null;
        }

        public async Task<Localizacao?> ObterPorLogradouro(string logradouro)
        {
            // Nota: No Firestore, buscas por texto exato funcionam assim, 
            // mas para buscas parciais seria necessário um serviço como Algolia.
            Query query = _firestoreDb.Collection(_collectionName).WhereEqualTo("Logradouro", logradouro);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            if (snapshot.Documents.Count > 0)
            {
                var document = snapshot.Documents[0];
                var localizacao = document.ConvertTo<Localizacao>();
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

            // Validação Geográfica via OpenStreetMap
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
                // Conversão para o padrão internacional para evitar que a API se perca.
                var latitudeTexto = latitude.ToString(CultureInfo.InvariantCulture);
                var longitudeTexto = longitude.ToString(CultureInfo.InvariantCulture);
                
                // Aguarda um "JSON" de volta para que o sistema consiga ler a resposta.
                var urlDaConsulta = 
                    $"https://nominatim.openstreetmap.org/reverse?format=json&lat=" +
                    $"{latitudeTexto}&lon={longitudeTexto}";

                // Ligação para o servidor e aguarda o retorno.
                var respostaDaInternet = await _httpClient.GetAsync(urlDaConsulta);

                if (!respostaDaInternet.IsSuccessStatusCode)
                    return false; // Se o site estiver fora do ar, tratamos como inválido por agora.

                // Lê o que o mapa respondeu.
                var corpoDaResposta = await 
                    respostaDaInternet.Content.ReadAsStringAsync();

                // OpenStreetMap: Se ele não conhece o lugar (ex: meio do mar),
                // ele nos envia uma mensagem contendo a palavra "error".                 
                bool enderecoEncontrado = !corpoDaResposta.Contains("\"error\"");

                return enderecoEncontrado;
            }
            catch (Exception erro)
            {
                // Se a internet cair, o usuário não pode ficar travado.
                _logger.LogError(
                    $"Não conseguimos validar o local agora. Motivo: {erro.Message}");
                return true;
            }
        }
    }
}