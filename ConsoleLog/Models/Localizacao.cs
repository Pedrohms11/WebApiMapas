using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleLog.Models
{
    /// <summary>
    /// Modelo de dados para Localização (Camada Model - MVVM)
    /// </summary>
    [Table("Localizacoes")]
    public class Localizacao
    {
        /// <summary>
        /// Identificador único da localização
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        /// <summary>
        /// Nome do logradouro (rua, avenida, etc.)
        /// </summary>
        [Required(ErrorMessage = "Logradouro é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Logradouro deve ter entre 3 e 100 caracteres")]
        public string Logradouro { get; set; } = string.Empty;

        /// <summary>
        /// Número do imóvel
        /// </summary>
        [Required(ErrorMessage = "Número é obrigatório")]
        [StringLength(10, ErrorMessage = "Número deve ter no máximo 10 caracteres")]
        public string Numero { get; set; } = string.Empty;

        /// <summary>
        /// Nome do bairro
        /// </summary>
        [Required(ErrorMessage = "Bairro é obrigatório")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Bairro deve ter entre 2 e 50 caracteres")]
        public string Bairro { get; set; } = string.Empty;

        /// <summary>
        /// Código postal (CEP) - formato: 8 dígitos
        /// </summary>
        [Required(ErrorMessage = "CEP é obrigatório")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "CEP deve conter exatamente 8 dígitos numéricos")]
        public string Cep { get; set; } = string.Empty;

        /// <summary>
        /// Latitude da localização (-90 a 90)
        /// </summary>
        [Required(ErrorMessage = "Latitude é obrigatória")]
        [Range(-90, 90, ErrorMessage = "Latitude deve estar entre -90 e 90")]
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude da localização (-180 a 180)
        /// </summary>
        [Required(ErrorMessage = "Longitude é obrigatória")]
        [Range(-180, 180, ErrorMessage = "Longitude deve estar entre -180 e 180")]
        public double Longitude { get; set; }

        /// <summary>
        /// Data e hora do registro
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

    

