using Google.Cloud.Firestore;
using WebApiMapas.Data;
using WebApiMapas.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApiMapas.Service
{
    /// <summary>
    /// Serviço responsável por gerenciar operações relacionadas a localizações georreferenciadas no Firebase.
    /// </summary>
    public class LocalizacaoService
    {
        private readonly ILogger<LocalizacaoService> _logger;
        private readonly FirestoreDb _firestoreDb;

        /// <summary>
        /// Construtor da classe - Recebe o Logger e o serviço do Firebase 
        /// via injeção de dependência. Repositório antigo removido!
        /// </summary>
        public LocalizacaoService(ILogger<LocalizacaoService> logger, FirestoreService firestoreService)
        {
            _logger = logger;
            _firestoreDb = firestoreService.Db; // Pegamos a conexão do banco aqui
        }

        /// <summary>
        /// Listar todas as localizações buscando direto no Firebase, sem passar por um repositório intermediário.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Localizacao>> Listar()
        {
            CollectionReference collectionRef = _firestoreDb.Collection("localizacoes");
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

        /// <summary>
        /// Listar uma localização por ID buscando direto no Firebase
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Localizacao> ObterPorId(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection("localizacoes").Document(id);
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
        public async Task<Localizacao> ObterPorLogradouro(string logradouro)
        {
            Query query = _firestoreDb.Collection("localizacoes").WhereEqualTo("Logradouro", logradouro);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            if (snapshot.Documents.Count > 0)
            {
                var localizacao = snapshot.Documents[0].ConvertTo<Localizacao>();
                localizacao.Id = snapshot.Documents[0].Id;
                return localizacao;
            }

            return null;
        }

        /// <summary>
        /// Criar uma nova localização persistindo direto no Firebase
        /// </summary>
        public async Task<Localizacao> Criar(Localizacao localizacao)
        {
            // Validações
            if (localizacao.Latitude < -90 || localizacao.Latitude > 90)
                throw new ArgumentException("A latitude deve estar entre -90 e 90 graus.");

            if (localizacao.Longitude < -180 || localizacao.Longitude > 180)
                throw new ArgumentException("A longitude deve estar entre -180 e 180 graus.");

            // 1. Referência para o documento que guarda o contador
            DocumentReference contadorRef = _firestoreDb.Collection("configuracoes").Document("contador_localizacoes");

            // 2. Executa a transação
            int novoIdNumerico = await _firestoreDb.RunTransactionAsync(async transaction =>
            {
                DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(contadorRef);
                int idAtual = 0;

                if (snapshot.Exists)
                {
                    // Pega o valor atual do contador
                    idAtual = snapshot.GetValue<int>("ultimoId");
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

            // 3. O Firestore exige que o ID do documento seja uma string. 
            // Convertemos o número para string (ex: "1", "2", "3")
            string idString = novoIdNumerico.ToString();
            DocumentReference docRef = _firestoreDb.Collection("localizacoes").Document(idString);

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
          DocumentReference docRef = _firestoreDb.Collection("localizacoes").Document(id);
            await docRef.SetAsync(existente, SetOptions.Overwrite);
        }

        /// <summary>
        /// Deletar um documento pelo ID no Firebase
        /// </summary>
        public async Task Delete(string id) // <-- ATENÇÃO: Mudou de int para string!
        {
            DocumentReference docRef = _firestoreDb.Collection("localizacoes").Document(id);
            await docRef.DeleteAsync();
        }
    }
}