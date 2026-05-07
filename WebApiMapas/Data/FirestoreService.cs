using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace WebApiMapas.Data
{
    public class FirestoreService
    {
        public FirestoreDb Db { get; private set; }

        public FirestoreService(IConfiguration configuration)
        {
            var projectId = configuration["Firebase:ProjectId"];
            var relativeKeyPath = configuration["Firebase:KeyFilePath"];

            // Garante que o caminho comece na pasta onde a API está rodando
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeKeyPath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"O arquivo de chave não foi encontrado em: {fullPath}");
            }

            // Define a variável de ambiente
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fullPath);

            // Inicializa a conexão
            Db = FirestoreDb.Create(projectId);
        }
    }
}