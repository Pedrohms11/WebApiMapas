using Google.Cloud.Firestore;

namespace WebApiMapas.Models
{
    public class Localizacao
    {
        [FirestoreProperty]
        public string Id { get; set; }
        [FirestoreProperty]
        public string Logradouro { get; set; }
        [FirestoreProperty]
        public string Numero { get; set; }
        [FirestoreProperty]
        public string Bairro { get; set; }
        [FirestoreProperty]
        public string Cep { get; set; }
        [FirestoreProperty]
        public double Latitude { get; set; }
        [FirestoreProperty]
        public double Longitude { get; set; }
        [FirestoreProperty]
        public DateTime Timestamp { get; set; }
    }
}
