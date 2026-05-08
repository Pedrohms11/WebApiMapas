using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleLog.Models
{
    /// <summary>
    /// Modelo de dados para Localização
    /// </summary>
    [Table("Localizacoes")]
    public class Localizacao
    {
        /// <summary>
        /// Identificador único da localização (string vindo do Firebase)
        /// </summary>
        [Key]
        public string Id { get; set; } = string.Empty;  // Alterado de int para string

        /// <summary>
        /// Nome do logradouro (rua, avenida, etc.)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Logradouro { get; set; } = string.Empty;

        /// <summary>
        /// Número do imóvel
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Numero { get; set; } = string.Empty;

        /// <summary>
        /// Nome do bairro
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Bairro { get; set; } = string.Empty;

        /// <summary>
        /// Código postal (CEP) - formato: 8 dígitos
        /// </summary>
        [Required]
        [StringLength(8)]
        public string Cep { get; set; } = string.Empty;

        /// <summary>
        /// Latitude da localização (-90 a 90)
        /// </summary>
        [Required]
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude da localização (-180 a 180)
        /// </summary>
        [Required]
        public double Longitude { get; set; }

        /// <summary>
        /// Data e hora do registro
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Hash para controle de versão
        /// </summary>
        [StringLength(64)]
        public string? DataHash { get; set; }

        /// <summary>
        /// Data da última sincronização
        /// </summary>
        public DateTime? LastSyncAt { get; set; }
    }
}