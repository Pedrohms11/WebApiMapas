using Google.Cloud.Firestore;
using System;
using System.IO;
using System.Text.Json;

namespace WebApiMapas.Data
{
    public class FirestoreService
    {
        public FirestoreDb Db { get; private set; }

        public FirestoreService()
        {
            var relativeKeyPath = "config_API/firebase-key.json";
            var fullPath = Path.Combine(AppContext.BaseDirectory, relativeKeyPath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"ERRO CRÍTICO: Arquivo não encontrado em: {fullPath}");
            }

            // Lê o arquivo JSON para descobrir o ID do Projeto automaticamente
            string jsonString = File.ReadAllText(fullPath);
            using JsonDocument doc = JsonDocument.Parse(jsonString);

            if (!doc.RootElement.TryGetProperty("project_id", out var projectIdElement))
            {
                throw new Exception("O arquivo JSON é inválido. A propriedade 'project_id' não foi encontrada.");
            }

            string projectId = projectIdElement.GetString()!;

            // MODO DEFINITIVO: Injeção direta e explícita da credencial física
            var builder = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                CredentialsPath = fullPath 
            };

            Db = builder.Build();
        }
    }
}