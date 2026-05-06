using Google.Cloud.Firestore;
using WebApiMapas.Models;
using WebApiMapas.Repositories.Interfaces;

namespace WebApiMapas.Service
{
    /// <summary>
    /// Serviço responsável por gerenciar operações relacionadas a localizações georreferenciadas.
    /// </summary>
    public class LocalizacaoService
    {
        /// <summary>
        /// Repositorio de localizações - Responsável por acessar os
        /// dados geográficos no banco de dados Firebase
        /// </summary>
        private readonly ILocalizacaoRepository _repo;

        /// <summary>
        /// Construtor da classe - Recebe o repository de localizações
        /// via injeção de dependência
        /// </summary>
        /// <param name="repo">Interface do repositório de dados geográficos</param>
        public LocalizacaoService(ILocalizacaoRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Localizacao>> Listar()
        {
            // Referência da coleção
            CollectionReference collectionRef = _firestoreDb.Collection("Localizacoes");

            // Busca todos os documentos da coleção
            QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();

            List<Localizacao> lista = new List<Localizacao>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    // Converte o documento para o modelo
                    var item = document.ConvertTo<Localizacao>();

                    item.Id = document.Id;

                    lista.Add(item);
                }
            }

            return lista;
        }

        public async Task<Localizacao> ObterPorId(string id)
        {
            // Referência para a coleção no Firestore
            DocumentReference docRef = _firestoreDb.Collection("Localizacoes").Document(id);

            // Busca o snapshot do documento de forma assíncrona
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                // Converte o documento para o seu modelo de classe C#
                var localizacao = snapshot.ConvertTo<Localizacao>();
                return localizacao;
            }

            return null;
        }

        /// <summary>
        /// Criar uma nova localização - Chama o método Add do repository 
        /// para persistir os dados geográficos no Firebase.
        /// </summary>
        /// <param name="localizacao">Objeto contendo as coordenadas e informações do local.</param>
        /// <returns>A localização persistida com os dados atualizados.</returns>
        public async Task<Localizacao> Criar(Localizacao localizacao)
        {

            // Validação de Latitude: deve estar entre -90 e 90
            if (localizacao.Latitude < -90 || localizacao.Latitude > 90)
            {
                throw new ArgumentException("A latitude deve estar entre -90 e 90 graus.");
            }

            // Validação de Longitude: deve estar entre -180 e 180
            if (localizacao.Longitude < -180 || localizacao.Longitude > 180)
            {
                throw new ArgumentException("A longitude deve estar entre -180 e 180 graus.");
            }

            await _repo.Add(localizacao);
            return localizacao;
        }

        public async Task Delete(int id)
            => await _repo.Delete(id);
    }
}
