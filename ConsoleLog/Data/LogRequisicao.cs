using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleLog.Models
{
    [Table("LogsRequisicao")]
    public class LogRequisicao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Operacao { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Endpoint { get; set; } = string.Empty;

        [StringLength(200)]
        public string Parametros { get; set; } = string.Empty;

        [StringLength(500)]
        public string RequestBody { get; set; } = string.Empty;

        [StringLength(500)]
        public string ResponseBody { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public long DuracaoMs { get; set; }

        [StringLength(100)]
        public string Usuario { get; set; } = string.Empty;

        [StringLength(100)]
        public string EmailUsuario { get; set; } = string.Empty;

        [StringLength(50)]
        public string PerfilUsuario { get; set; } = string.Empty;

        [StringLength(100)]
        public string Maquina { get; set; } = string.Empty;

        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        [Required]
        public DateTime DataHora { get; set; }

        public bool Sucesso { get; set; }

        [StringLength(500)]
        public string MensagemErro { get; set; } = string.Empty;

        [StringLength(50)]
        public string Origem { get; set; } = string.Empty;

        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty;
    }
}