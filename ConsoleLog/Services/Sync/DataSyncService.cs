using Microsoft.EntityFrameworkCore;
using ConsoleLog.Models;
using ConsoleLog.Data;

namespace ConsoleLog.Services.Sync
{
    public class DataSyncService
    {
        private readonly FirestoreService _firestoreService;
        private readonly AppDbContext _context;
        private readonly LogService _logger;

        public DataSyncService(FirestoreService firestoreService, AppDbContext context, LogService logger)
        {
            _firestoreService = firestoreService;
            _context = context;
            _logger = logger;
        }

        public async Task<SyncResult> SincronizarFirestoreParaLocal()
        {
            var result = new SyncResult();
            try
            {
                var firestoreData = await _firestoreService.BuscarTodasLocalizacoes();
                result.OrigemCount = firestoreData.Count;

                var localData = await _context.Localizacoes.ToDictionaryAsync(l => l.Id);
                var novos = 0;
                var atualizados = 0;

                foreach (var item in firestoreData)
                {
                    if (localData.TryGetValue(item.Id, out var local))
                    {
                        local.Logradouro = item.Logradouro;
                        local.Numero = item.Numero;
                        local.Bairro = item.Bairro;
                        local.Cep = item.Cep;
                        local.Latitude = item.Latitude;
                        local.Longitude = item.Longitude;
                        local.Timestamp = item.Timestamp;
                        local.LastSyncAt = DateTime.UtcNow;
                        atualizados++;
                    }
                    else
                    {
                        item.LastSyncAt = DateTime.UtcNow;
                        await _context.Localizacoes.AddAsync(item);
                        novos++;
                    }
                }

                await _context.SaveChangesAsync();
                result.Sucesso = true;
                result.NovosRegistros = novos;
                result.RegistrosAtualizados = atualizados;
                result.DestinoCount = await _context.Localizacoes.CountAsync();
                result.Mensagem = $"Sincronizado: +{novos} ~{atualizados}";
            }
            catch (Exception ex)
            {
                result.Sucesso = false;
                result.Mensagem = ex.Message;
                _logger.LogError("Erro na sincronização", ex, "SYNC");
            }
            return result;
        }

        public async Task<int> LimparDadosAntigos(int diasManter)
        {
            var dataCorte = DateTime.UtcNow.AddDays(-diasManter);
            var antigos = await _context.Localizacoes.Where(l => l.Timestamp < dataCorte).ToListAsync();
            if (antigos.Any())
            {
                _context.Localizacoes.RemoveRange(antigos);
                await _context.SaveChangesAsync();
            }
            return antigos.Count;
        }
    }

    public class SyncResult
    {
        public bool Sucesso { get; set; }
        public int OrigemCount { get; set; }
        public int DestinoCount { get; set; }
        public int NovosRegistros { get; set; }
        public int RegistrosAtualizados { get; set; }
        public int RegistrosRemovidos { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }
}