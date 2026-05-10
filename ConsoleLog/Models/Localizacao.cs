using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleLog.Models
{
    [Table("Localizacoes")]
    public class Localizacao
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Logradouro { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Numero { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Bairro { get; set; } = string.Empty;

        [Required]
        [StringLength(8)]
        public string Cep { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }

        public string? DataHash { get; set; }
        public DateTime? LastSyncAt { get; set; }
    }
}