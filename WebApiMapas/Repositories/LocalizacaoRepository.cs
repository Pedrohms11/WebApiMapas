using Google.Cloud.Firestore;
using WebApiMapas.Data;
using WebApiMapas.Models;
using WebApiMapas.Repositories.Interfaces;

namespace WebApiMapas.Repositories
{
    public class LocalizacaoRepository : ILocalizacaoRepository
    {
        private readonly CollectionReference _collection;

        public LocalizacaoRepository(FirestoreService context)
        {
            _collection = context.Db.Collection("Localizacoes");
        }

        public async Task<List<Localizacao>> GetAll()
        {
            var snapshot = await _collection.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<Localizacao>()).ToList();
        }

        // Recebe o INT e converte para string na hora de buscar o documento
        public async Task<Localizacao> GetById(int id)
        {
            var snapshot = await _collection.Document(id.ToString()).GetSnapshotAsync();

            if (!snapshot.Exists)
                throw new KeyNotFoundException($"Localização com ID {id} não encontrada.");

            return snapshot.ConvertTo<Localizacao>();
        }

        public async Task Add(Localizacao localizacao)
        {
            // IMPORTANTE: Aqui assumimos que "localizacao.Id" já possui um número inteiro válido!
            // Usamos o SetAsync no documento com o nome do ID (ex: documento "1")
            var docRef = _collection.Document(localizacao.Id.ToString());
            await docRef.SetAsync(localizacao);
        }

        public async Task Update(Localizacao localizacao)
        {
            var docRef = _collection.Document(localizacao.Id.ToString());

            // Verifica se o documento existe antes de atualizar
            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists)
            {
                throw new KeyNotFoundException($"Localização com ID {localizacao.Id} não encontrada.");
            }

            await docRef.SetAsync(localizacao, SetOptions.MergeAll);
        }

        // Recebe o INT e converte para string na hora de deletar
        public async Task Delete(int id)
        {
            var docRef = _collection.Document(id.ToString());

            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists)
            {
                throw new KeyNotFoundException($"Localização com ID {id} não encontrada.");
            }

            await docRef.DeleteAsync();
        }
    }
}