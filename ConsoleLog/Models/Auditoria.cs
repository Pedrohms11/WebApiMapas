using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleLog.Models
{
    /// <summary>
    /// Representa um registro de auditoria para rastreamento de alterações em dados do sistema
    /// </summary>
    [Table("Auditoria")]
    public class Auditoria
    {
        /// <summary>
        /// Identificador único do registro de auditoria
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome da tabela onde a alteração foi realizada
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Tabela { get; set; } = string.Empty;

        /// <summary>
        /// Identificador do registro afetado na tabela original
        /// </summary>
        [Required]
        [StringLength(100)]
        public string RegistroId { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de ação executada (Insert, Update, Delete, etc.)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Acao { get; set; } = string.Empty;

        /// <summary>
        /// Estado anterior dos dados antes da alteração (geralmente em formato JSON)
        /// </summary>
        [Required]
        public string DadosAntigos { get; set; } = string.Empty;

        /// <summary>
        /// Estado atual dos dados após a alteração (geralmente em formato JSON)
        /// </summary>
        [Required]
        public string DadosNovos { get; set; } = string.Empty;

        /// <summary>
        /// Nome/login do usuário que realizou a alteração
        /// </summary>
        [StringLength(100)]
        public string Usuario { get; set; } = string.Empty;

        /// <summary>
        /// Endereço de e-mail do usuário que realizou a alteração
        /// </summary>
        [StringLength(100)]
        public string EmailUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Perfil/função do usuário no sistema no momento da alteração
        /// </summary>
        [StringLength(50)]
        public string PerfilUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Nome da máquina/computador onde a alteração foi realizada
        /// </summary>
        [StringLength(100)]
        public string Maquina { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora exata em que a alteração foi executada
        /// </summary>
        [Required]
        public DateTime DataHora { get; set; }

        /// <summary>
        /// Endereço IP da máquina que realizou a alteração
        /// </summary>
        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Informações adicionais ou detalhes complementares sobre a auditoria
        /// </summary>
        [StringLength(500)]
        public string Detalhes { get; set; } = string.Empty;

        /// <summary>
        /// Origem da ação (ex: API, Interface Web, Mobile, Batch, etc.)
        /// </summary>
        [StringLength(50)]
        public string Origem { get; set; } = string.Empty;
    }
}