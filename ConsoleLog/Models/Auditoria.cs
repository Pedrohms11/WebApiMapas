using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleLog.Models
{
    [Table("Auditoria")]
    public class Auditoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Tabela { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string RegistroId { get; set; } = string.Empty; // Firebase ID (string)

        [Required]
        [StringLength(20)]
        public string Acao { get; set; } = string.Empty; // INSERT, UPDATE, DELETE

        [Required]
        public string DadosAntigos { get; set; } = string.Empty;

        [Required]
        public string DadosNovos { get; set; } = string.Empty;

        [StringLength(100)]
        public string Usuario { get; set; } = string.Empty;

        [StringLength(100)]
        public string EmailUsuario { get; set; } = string.Empty;

        [StringLength(50)]
        public string PerfilUsuario { get; set; } = string.Empty;

        [StringLength(100)]
        public string Maquina { get; set; } = string.Empty;

        [Required]
        public DateTime DataHora { get; set; }

        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        [StringLength(500)]
        public string Detalhes { get; set; } = string.Empty;

        [StringLength(50)]
        public string Origem { get; set; } = string.Empty; // Firebase, API, Console
    }
}