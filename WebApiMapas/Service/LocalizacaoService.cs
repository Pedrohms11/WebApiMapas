using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiMapas.Data;
using WebApiMapas.Models;

namespace WebApiMapas.Service
{
    /// <summary>
    /// Serviço responsável por gerenciar operações relacionadas a localizações georreferenciadas no Firebase.
    /// </summary>
    public class LocalizacaoService
    {
        private readonly ILogger<LocalizacaoService> _logger;
        private readonly FirestoreDb _firestoreDb;
        private readonly string _collectionName = "localizacoes"; // Centralizado para evitar erros de digitação

        /// <summary>
        /// Construtor da classe - Recebe o Logger e o serviço do Firebase 
        /// via injeção de dependência. Repositório antigo removido!
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="firestoreService"></param>
        public LocalizacaoService(ILogger<LocalizacaoService> logger, FirestoreService firestoreService)
        {
            _logger = logger;
            _firestoreDb = firestoreService.Db; // Acessa a instância do FirestoreDb fornecida pelo serviço de configuração
        }

        /// <summary>
        /// Listar todas as localizações buscando direto no Firebase.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Listar uma localização por ID buscando direto no Firebase
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Listar uma localização por logradouro buscando direto no Firebase
        /// </summary>
        /// <param name="logradouro"></param>
        /// <returns></returns>
        public async Task<Localizacao?> ObterPorLogradouro(string logradouro)
        {
            Query query = _firestoreDb.Collection(_collectionName);
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

        /// <summary>
        /// Criar uma nova localização persistindo direto no Firebase (Com Auto-Incremento)
        /// </summary>
        /// <param name="localizacao"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Localizacao> Criar(Localizacao localizacao)
        {
            // Validações
            if (localizacao.Latitude < -90 || localizacao.Latitude > 90)
                throw new ArgumentException("A latitude deve estar entre -90 e 90 graus.");

            if (localizacao.Longitude < -180 || localizacao.Longitude > 180)
                throw new ArgumentException("A longitude deve estar entre -180 e 180 graus.");

            // 1. Referência para o documento que guarda o contador
            DocumentReference contadorRef = _firestoreDb.Collection("configuracoes").Document("contador_localizacoes");

            // 2. Executa a transação para gerar um ID sequencial seguro
            int novoIdNumerico = await _firestoreDb.RunTransactionAsync(async transaction =>
            {
                DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(contadorRef);
                int idAtual = 0;

                if (snapshot.Exists)
                {
                    // Usa TryGetValue para evitar erros caso o campo "ultimoId" não exista ainda
                    snapshot.TryGetValue("ultimoId", out idAtual);
                }

                int proximoId = idAtual + 1;

                // Atualiza o documento do contador com o novo valor
                Dictionary<string, object> atualizacaoContador = new Dictionary<string, object>
                {
                    { "ultimoId", proximoId }
                };

                transaction.Set(contadorRef, atualizacaoContador, SetOptions.MergeAll);

                return proximoId;
            });

            // 3. Converte o número para string e define a referência
            string idString = novoIdNumerico.ToString();
            DocumentReference docRef = _firestoreDb.Collection(_collectionName).Document(idString);

            // 4. Salva a localização com o ID numérico forçado
            localizacao.Id = idString;
            await docRef.SetAsync(localizacao);

            return localizacao;
        }

        /// <summary>
        /// Atualiza uma localização existente no Firebase. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="existente"></param>
        /// <returns></returns>
        public async Task Atualizar(string id, Localizacao existente)
        {
            DocumentReference docRef = _firestoreDb.Collection(_collectionName).Document(id);

            // Troquei de Overwrite para MergeAll para ser mais seguro. 
            // Assim, se houver campos no Firebase que não estão na sua classe, eles não serão apagados.
            await docRef.SetAsync(existente, SetOptions.MergeAll);
        }

        /// <summary>
        /// Deletar um documento pelo ID no Firebase
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task Delete(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection(_collectionName).Document(id);
            await docRef.DeleteAsync();
        }
    }
}