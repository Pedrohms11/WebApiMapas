using Microsoft.EntityFrameworkCore;
using ConsoleLog.Models;
using ConsoleLog.Data;
using ConsoleLog.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleLog.Services.Sync
{
    /// <summary>
    /// Serviço de sincronização entre Firestore e SQLite Local
    /// </summary>
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

        /// <summary>
        /// Sincroniza dados do Firestore para o SQLite local
        /// </summary>
        public async Task<SyncResult> SincronizarFirestoreParaLocal()
        {
            var result = new SyncResult();

            try
            {
                _logger.LogInfo("Iniciando sincronização Firestore → SQLite Local", "SYNC");

                // 1. Buscar dados do Firestore
                var firestoreData = await _firestoreService.BuscarTodasLocalizacoes();
                result.OrigemCount = firestoreData.Count;

                if (!firestoreData.Any())
                {
                    _logger.LogWarning("Nenhum dado encontrado no Firestore para sincronizar", "SYNC");
                    result.Mensagem = "Nenhum dado encontrado no Firestore";
                    return result;
                }

                // 2. Buscar dados existentes no SQLite - CORRIGIDO: Especificar os tipos explicitamente
                var localData = await _context.Localizacoes
                    .ToDictionaryAsync<Localizacao, string>(l => l.Id);  // ✅ Tipos explícitos

                // 3. Calcular hashes dos dados do Firestore - CORRIGIDO
                var firestoreDataComHash = firestoreData.Select(f =>
                {
                    f.DataHash = CalcularHash(f);
                    return f;
                }).ToList();

                var registrosNovos = 0;
                var registrosAtualizados = 0;
                var registrosRemovidos = 0;

                // 4. Processar novos registros e atualizações
                foreach (var firestoreItem in firestoreDataComHash)
                {
                    if (localData.TryGetValue(firestoreItem.Id, out var localItem))
                    {
                        // Registro existe - verificar se precisa atualizar
                        if (localItem.DataHash != firestoreItem.DataHash)
                        {
                            // Atualizar registro existente
                            localItem.Logradouro = firestoreItem.Logradouro;
                            localItem.Numero = firestoreItem.Numero;
                            localItem.Bairro = firestoreItem.Bairro;
                            localItem.Cep = firestoreItem.Cep;
                            localItem.Latitude = firestoreItem.Latitude;
                            localItem.Longitude = firestoreItem.Longitude;
                            localItem.Timestamp = firestoreItem.Timestamp;
                            localItem.DataHash = firestoreItem.DataHash;
                            localItem.LastSyncAt = DateTime.UtcNow;

                            registrosAtualizados++;
                            _logger.LogInfo($"Atualizando localização ID: {firestoreItem.Id}", "SYNC");
                        }
                        else
                        {
                            // Dados iguais, apenas atualizar timestamp de sync
                            localItem.LastSyncAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        // Novo registro - adicionar
                        firestoreItem.LastSyncAt = DateTime.UtcNow;
                        await _context.Localizacoes.AddAsync(firestoreItem);
                        registrosNovos++;
                        _logger.LogInfo($"Adicionando nova localização ID: {firestoreItem.Id}", "SYNC");
                    }

                    // Remover do dicionário local os que foram processados
                    localData.Remove(firestoreItem.Id);
                }

                // 5. Registros que existem no SQLite mas não no Firestore (foram removidos)
                foreach (var localItem in localData.Values)
                {
                    _context.Localizacoes.Remove(localItem);
                    registrosRemovidos++;
                    _logger.LogInfo($"Removendo localização ID: {localItem.Id} (não existe mais no Firestore)", "SYNC");
                }

                // 6. Salvar alterações
                await _context.SaveChangesAsync();

                result.Sucesso = true;
                result.NovosRegistros = registrosNovos;
                result.RegistrosAtualizados = registrosAtualizados;
                result.RegistrosRemovidos = registrosRemovidos;
                result.DestinoCount = await _context.Localizacoes.CountAsync();  // ✅ Sem tipos explícitos
                result.Mensagem = $"Sincronização concluída! +{registrosNovos} ~{registrosAtualizados} -{registrosRemovidos}";

                _logger.LogSuccess(result.Mensagem, "SYNC");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro durante sincronização", ex, "SYNC");
                result.Sucesso = false;
                result.Mensagem = $"Erro na sincronização: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Calcula hash único dos dados da localização para comparação
        /// </summary>
        private string CalcularHash(Localizacao localizacao)
        {
            var dados = $"{localizacao.Logradouro}|{localizacao.Numero}|{localizacao.Bairro}|{localizacao.Cep}|{localizacao.Latitude}|{localizacao.Longitude}|{localizacao.Timestamp:yyyyMMddHHmmss}";

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(dados);
            var hash = sha256.ComputeHash(bytes);

            return Convert.ToHexString(hash);
        }

        /// <summary>
        /// Limpa dados antigos do SQLite
        /// </summary>
        public async Task<int> LimparDadosAntigos(int diasManter = 30)
        {
            try
            {
                var dataCorte = DateTime.UtcNow.AddDays(-diasManter);
                var registrosAntigos = await _context.Localizacoes
                    .Where(l => l.Timestamp < dataCorte)
                    .ToListAsync();  // ✅ Sem tipos explícitos

                if (registrosAntigos.Any())
                {
                    _context.Localizacoes.RemoveRange(registrosAntigos);
                    await _context.SaveChangesAsync();

                    _logger.LogSuccess($"Removidos {registrosAntigos.Count} registros antigos (> {diasManter} dias)", "SYNC");
                    return registrosAntigos.Count;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao limpar dados antigos", ex, "SYNC");
                return 0;
            }
        }
    }

    /// <summary>
    /// Resultado da operação de sincronização
    /// </summary>
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