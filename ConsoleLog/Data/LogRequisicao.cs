using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleLog.Models
{
    /// <summary>
    /// Representa um registro de log de requisições HTTP realizadas ao sistema.
    /// Armazena informações detalhadas sobre cada requisição, incluindo dados da operação,
    /// parâmetros, respostas, informações do usuário e métricas de desempenho.
    /// </summary>
    [Table("LogsRequisicao")]
    public class LogRequisicao
    {
        /// <summary>
        /// Identificador único do log de requisição.
        /// Chave primária auto-gerada pelo banco de dados.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome da operação ou ação executada na requisição.
        /// Exemplo: "ConsultarLocalizacao", "AtualizarAuditoria".
        /// Campo obrigatório com limite máximo de 50 caracteres.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Operacao { get; set; } = string.Empty;

        /// <summary>
        /// Endpoint ou rota da API que foi acessada.
        /// Exemplo: "/api/localizacao/consultar", "/api/auditoria/registrar".
        /// Campo obrigatório com limite máximo de 100 caracteres.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Parâmetros da requisição em formato texto (query string, route parameters, etc.).
        /// Limite máximo de 200 caracteres.
        /// </summary>
        [StringLength(200)]
        public string Parametros { get; set; } = string.Empty;

        /// <summary>
        /// Corpo da requisição (request body) em formato JSON ou texto.
        /// Utilizado para registrar dados enviados pelo cliente.
        /// Limite máximo de 500 caracteres.
        /// </summary>
        [StringLength(500)]
        public string RequestBody { get; set; } = string.Empty;

        /// <summary>
        /// Corpo da resposta (response body) retornada ao cliente.
        /// Limite máximo de 500 caracteres.
        /// </summary>
        [StringLength(500)]
        public string ResponseBody { get; set; } = string.Empty;

        /// <summary>
        /// Código de status HTTP retornado pela operação.
        /// Exemplo: 200 (Sucesso), 400 (Requisição inválida), 404 (Não encontrado), 500 (Erro interno).
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Duração total da execução da requisição em milissegundos (ms).
        /// Utilizado para monitoramento de desempenho e identificação de lentidões.
        /// </summary>
        public long DuracaoMs { get; set; }

        /// <summary>
        /// Nome do usuário autenticado que realizou a requisição.
        /// Limite máximo de 100 caracteres.
        /// </summary>
        [StringLength(100)]
        public string Usuario { get; set; } = string.Empty;

        /// <summary>
        /// Endereço de e-mail do usuário autenticado que realizou a requisição.
        /// Limite máximo de 100 caracteres.
        /// </summary>
        [StringLength(100)]
        public string EmailUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Perfil ou papel do usuário no sistema (Admin, Operador, Visualizador, etc.).
        /// Limite máximo de 50 caracteres.
        /// </summary>
        [StringLength(50)]
        public string PerfilUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Nome da máquina ou dispositivo de origem da requisição.
        /// Limite máximo de 100 caracteres.
        /// </summary>
        [StringLength(100)]
        public string Maquina { get; set; } = string.Empty;

        /// <summary>
        /// Endereço IP de origem da requisição.
        /// Armazena tanto IPv4 quanto IPv6.
        /// Limite máximo de 50 caracteres.
        /// </summary>
        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora exata em que a requisição foi processada.
        /// Campo obrigatório com precisão de milissegundos.
        /// </summary>
        [Required]
        public DateTime DataHora { get; set; }

        /// <summary>
        /// Indica se a requisição foi processada com sucesso.
        /// True para requisições bem-sucedidas, False para falhas.
        /// </summary>
        public bool Sucesso { get; set; }

        /// <summary>
        /// Mensagem de erro detalhada quando a requisição falha.
        /// Contém informações sobre exceções ou validações que causaram a falha.
        /// Limite máximo de 500 caracteres.
        /// </summary>
        [StringLength(500)]
        public string MensagemErro { get; set; } = string.Empty;

        /// <summary>
        /// Origem da requisição (Web, API, Mobile, Desktop, etc.).
        /// Limite máximo de 50 caracteres.
        /// </summary>
        [StringLength(50)]
        public string Origem { get; set; } = string.Empty;

        /// <summary>
        /// Categoria ou tipo do log (Segurança, Desempenho, Auditoria, Negócio, etc.).
        /// Limite máximo de 50 caracteres.
        /// </summary>
        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty;
    }
}