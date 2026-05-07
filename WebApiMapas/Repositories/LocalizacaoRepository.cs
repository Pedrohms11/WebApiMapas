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
            // Verifique se no seu Firebase a coleção se chama "Localizacoes" ou "localizacoes" (minúsculo)
            _collection = context.Db.Collection("localizacoes");
        }

        public async Task<List<Localizacao>> GetAll()
        {
            var snapshot = await _collection.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => {
                var item = doc.ConvertTo<Localizacao>();
                item.Id = doc.Id; // Garante que o ID do documento vá para a Model
                return item;
            }).ToList();
        }

        // Agora recebe STRING conforme a Interface
        public async Task<Localizacao> GetById(string id)
        {
            var snapshot = await _collection.Document(id).GetSnapshotAsync();

            if (!snapshot.Exists)
                throw new KeyNotFoundException($"Localização com ID {id} não encontrada.");

            var localizacao = snapshot.ConvertTo<Localizacao>();
            localizacao.Id = snapshot.Id;
            return localizacao;
        }

        public async Task Add(Localizacao localizacao)
        {
            // Se o ID vier vazio, o Firebase gera um automático
            if (string.IsNullOrEmpty(localizacao.Id))
            {
                await _collection.AddAsync(localizacao);
            }
            else
            {
                await _collection.Document(localizacao.Id).SetAsync(localizacao);
            }
        }

        public async Task Update(Localizacao localizacao)
        {
            var docRef = _collection.Document(localizacao.Id);
            await docRef.SetAsync(localizacao, SetOptions.MergeAll);
        }

        // Agora recebe STRING conforme a Interface
        public async Task Delete(string id)
        {
            var docRef = _collection.Document(id);
            await docRef.DeleteAsync();
        }
    }
}