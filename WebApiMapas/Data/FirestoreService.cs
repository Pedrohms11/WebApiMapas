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
            // Pega o ID do projeto e o caminho do arquivo JSON do appsettings.json            
            var projectId = configuration["Firebase:ProjectId"] ?? "web-api-mapas";
            var relativeKeyPath = configuration["Firebase:KeyFilePath"] ?? "config_API/firebase-key.json";

            // Monta o caminho completo onde o arquivo JSON deve estar
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeKeyPath);

            // Verifica se o arquivo existe na pasta para não dar erro de null
            if (File.Exists(fullPath))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fullPath);
                Db = FirestoreDb.Create(projectId);
            }
            else
            {                
                Console.WriteLine($"ERRO: Arquivo de credenciais não encontrado em: {fullPath}");
            }
        }
    }
}