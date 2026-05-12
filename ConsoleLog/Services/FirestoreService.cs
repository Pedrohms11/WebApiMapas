using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using ConsoleLog.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleLog.Services
{
    /// <summary>
    /// Serviço para integração com o Firebase Firestore, responsável por operações CRUD de localizações
    /// </summary>
    public class FirestoreService
    {
        private readonly FirestoreDb _db;
        private readonly LogService _logger;
        private readonly string _colecaoName = "localizacoes";

        /// <summary>
        /// Inicializa uma nova instância do serviço Firestore
        /// </summary>
        /// <param name="configuration">Configurações do sistema contendo credenciais do Firebase</param>
        /// <param name="logger">Serviço de logging do sistema</param>
        /// <exception cref="Exception">Lançada quando o ProjectId não está configurado</exception>
        /// <exception cref="FileNotFoundException">Lançada quando o arquivo de credenciais não é encontrado</exception>
        public FirestoreService(IConfiguration configuration, LogService logger)
        {
            _logger = logger;

            try
            {
                var projectId = configuration["Firebase:ProjectId"];
                var keyPath = configuration["Firebase:KeyFilePath"];

                if (string.IsNullOrEmpty(projectId))
                    throw new Exception("ProjectId não configurado");

                string resolvedKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, keyPath);

                if (!File.Exists(resolvedKeyPath))
                    throw new FileNotFoundException($"Arquivo não encontrado: {resolvedKeyPath}");

                var jsonCredentials = File.ReadAllText(resolvedKeyPath);

                var builder = new FirestoreDbBuilder
                {
                    ProjectId = projectId,
                    JsonCredentials = jsonCredentials
                };

                _db = builder.Build();
                _logger.LogSuccess($"Firestore conectado - Projeto: {projectId}", "FIRESTORE");
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao conectar ao Firestore", ex, "FIRESTORE");
                throw;
            }
        }

        /// <summary>
        /// Obtém a instância do FirestoreDb
        /// </summary>
        /// <returns>Instância configurada do FirestoreDb</returns>
        public FirestoreDb GetDb() => _db;

        /// <summary>
        /// Verifica se a conexão com o Firestore está ativa
        /// </summary>
        /// <returns>True se a conexão estiver OK, False caso contrário</returns>
        public async Task<bool> VerificarConexao()
        {
            try
            {
                await _db.ListRootCollectionsAsync().Take(1).ToListAsync();
                _logger.LogSuccess("Conexão com Firestore OK", "FIRESTORE");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Falha na conexão com Firestore", ex, "FIRESTORE");
                return false;
            }
        }

        /// <summary>
        /// Busca todas as localizações armazenadas no Firestore
        /// </summary>
        /// <returns>Lista de objetos Localizacao ordenados por Timestamp decrescente</returns>
        public async Task<List<Localizacao>> BuscarTodasLocalizacoes()
        {
            try
            {
                var snapshot = await _db.Collection(_colecaoName).GetSnapshotAsync();
                var localizacoes = new List<Localizacao>();

                foreach (var doc in snapshot.Documents)
                {
                    var loc = ConverterDocumentoParaLocalizacao(doc);
                    if (loc != null) localizacoes.Add(loc);
                }

                return localizacoes.OrderByDescending(l => l.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao buscar localizações", ex, "FIRESTORE");
                return new List<Localizacao>();
            }
        }

        /// <summary>
        /// Obtém estatísticas consolidadas das localizações no Firestore
        /// </summary>
        /// <returns>Objeto com estatísticas como total de registros, última atualização, bairros e CEPs únicos</returns>
        public async Task<FirestoreStats> ObterEstatisticas()
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

        /// <summary>
        /// Converte um documento do Firestore para o objeto Localizacao
        /// </summary>
        /// <param name="doc">Snapshot do documento Firestore</param>
        /// <returns>Objeto Localizacao populado ou null se o documento não existir</returns>
        private Localizacao? ConverterDocumentoParaLocalizacao(DocumentSnapshot doc)
        {
            if (!doc.Exists) return null;

            var dados = doc.ToDictionary();
            return new Localizacao
            {
                Id = doc.Id,
                Logradouro = dados.ContainsKey("Logradouro") ? dados["Logradouro"]?.ToString() ?? "" : "",
                Numero = dados.ContainsKey("Numero") ? dados["Numero"]?.ToString() ?? "" : "",
                Bairro = dados.ContainsKey("Bairro") ? dados["Bairro"]?.ToString() ?? "" : "",
                Cep = dados.ContainsKey("Cep") ? dados["Cep"]?.ToString() ?? "" : "",
                Latitude = dados.ContainsKey("Latitude") ? Convert.ToDouble(dados["Latitude"] ?? 0) : 0,
                Longitude = dados.ContainsKey("Longitude") ? Convert.ToDouble(dados["Longitude"] ?? 0) : 0,
                Timestamp = dados.ContainsKey("Timestamp") && dados["Timestamp"] is Timestamp ts ? ts.ToDateTime() : DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Representa estatísticas do Firestore para localizações
    /// </summary>
    public class FirestoreStats
    {
        /// <summary>Total de registros de localização no Firestore</summary>
        public int TotalRegistros { get; set; }

        /// <summary>Data e hora da última atualização registrada</summary>
        public DateTime UltimaAtualizacao { get; set; }

        /// <summary>Número de bairros distintos presentes nos registros</summary>
        public int BairrosUnicos { get; set; }

        /// <summary>Número de CEPs distintos presentes nos registros</summary>
        public int CepsUnicos { get; set; }
    }
}