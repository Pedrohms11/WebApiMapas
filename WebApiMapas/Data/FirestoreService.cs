using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using System;

namespace WebApiMapas.Data
{
    public class FirestoreService
    {
        public FirestoreDb Db { get; private set; }

        public FirestoreService(IConfiguration configuration)
        {
            // Busca as configurações do seu appsettings.json
            var projectId = configuration["Firebase:ProjectId"];
            var keyPath = configuration["Firebase:KeyFilePath"];

            // Configura a credencial do arquivo JSON que você baixou do Firebase
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);

            // Inicializa a conexão com o Firestore
            Db = FirestoreDb.Create(projectId);
        }
    }
}
