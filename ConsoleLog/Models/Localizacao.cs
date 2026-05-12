using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleLog.Models
{
    /// <summary>
    /// Representa um registro de localização geográfica com endereço e coordenadas
    /// </summary>
    [Table("Localizacoes")]
    public class Localizacao
    {
        /// <summary>
        /// Identificador único da localização
        /// </summary>
        [Key]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nome do logradouro (rua, avenida, praça, etc.)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Logradouro { get; set; } = string.Empty;

        /// <summary>
        /// Número do imóvel ou localização
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Numero { get; set; } = string.Empty;

        /// <summary>
        /// Nome do bairro onde a localização está situada
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Bairro { get; set; } = string.Empty;

        /// <summary>
        /// Código postal (CEP) do endereço
        /// </summary>
        [Required]
        [StringLength(8)]
        public string Cep { get; set; } = string.Empty;

        /// <summary>
        /// Coordenada de latitude da localização geográfica
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Coordenada de longitude da localização geográfica
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Data e hora do registro da localização
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Hash de dados para verificação de integridade ou sincronização
        /// </summary>
        public string? DataHash { get; set; }

        /// <summary>
        /// Data e hora da última sincronização realizada
        /// </summary>
        public DateTime? LastSyncAt { get; set; }
    }
}