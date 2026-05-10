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
    public class FirestoreService
    {
        private readonly FirestoreDb _db;
        private readonly LogService _logger;
        private readonly string _colecaoName = "localizacoes";

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

        public FirestoreDb GetDb() => _db;

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

    public class FirestoreStats
    {
        public int TotalRegistros { get; set; }
        public DateTime UltimaAtualizacao { get; set; }
        public int BairrosUnicos { get; set; }
        public int CepsUnicos { get; set; }
    }
}