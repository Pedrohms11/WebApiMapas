using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiMapas.Models;
using WebApiMapas.Service;

namespace WebApiMapas.Controllers
{
    /// <summary>
    /// Representa um controlador de API que gerencia recursos de localização georreferenciada. 
    /// Fornece endpoints para a criação e exclusão de registros de localização.
    /// </summary>
    /// <remarks>
    /// Este controlador expõe endpoints RESTful para trabalhar com localizações georreferenciadas.
    /// Ele suporta a criação de novas localizações e a exclusão de localizações existentes. 
    /// Todos os endpoints retornam códigos de status HTTP apropriados para indicar o resultado da operação. 
    /// O controlador deve ser utilizado como parte de uma Web API ASP.NET Core e segue as convenções 
    /// de roteamento padrão.
    /// </remarks>

    [Route("api/[controller]")]
    [ApiController]
    public class MapasController : ControllerBase
    {
        /// <summary>
        /// Instância do serviço de localização - Responsável por processar 
        /// a lógica de coordenadas e integração com o Firebase/SQLite.
        /// </summary>
        private readonly LocalizacaoService _service;

        private readonly FirestoreDb _firestoreDb = FirestoreDb.Create("webapimapas");

        /// <summary>
        /// Construtor da classe MapasController.
        /// </summary>
        /// <param name="service">Serviço de localização injetado via DI.</param>
        public MapasController(LocalizacaoService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET: api/Mapas - Retorna uma lista de todas as localizações cadastradas.
        /// </summary>
        /// 
        /// <remarks>
        /// GET: api/Mapas - Retorna uma lista de todas as localizações cadastradas.
        /// </remarks>
        /// 
        /// <returns></returns>
        /// <response code="200">Lista de localizações obtida com sucesso</response>
        /// <response code="500">Erro interno do servidor ao tentar listar as localizações</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var localizacoes = await _service.Listar();
                return Ok(new { mensagem = "Lista obtida com sucesso.", localizacoes });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao listar salas : {ex.Message}" });
            }
        }


        /// <summary>
        /// GET: api/Mapas/{id} - Retorna os detalhes de uma localização específica com base no ID fornecido.
        /// </summary>
        /// 
        /// <remarks>
        /// GET: api/Mapas/{id} - Retorna os detalhes de uma localização específica com base no ID fornecido.
        /// </remarks>
        /// 
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Sala encontrada</response>
        /// <response code="400">Requisição inválida</response>
        /// <response code="404">Não encontrado</response>
        /// <response code="500">Erro interno de servidor</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { mensagem = "ID inválido" });

            try
            {
                var localizacao = await _service.ObterPorId(id);

                if (localizacao == null)
                    return NotFound(new { mensagem = $"Localização com ID {id} não encontrada." });

                return Ok(new { mensagem = "Localização encontrada com sucesso.", localizacao });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao buscar sala : {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SalvarLocalizacao([FromBody] Localizacao novaLocalizacao)
        {
            // 1. Validação básica de segurança
            if (novaLocalizacao == null)
            {
                return BadRequest("O corpo da requisição não pode estar vazio.");
            }

            // 2. Validação de campos obrigatórios do endereço (ajuste conforme a necessidade do projeto)
            if (string.IsNullOrWhiteSpace(novaLocalizacao.Logradouro))
            {
                return BadRequest("O campo 'logradouro' é obrigatório.");
            }

            // 3. Validação das coordenadas (Regra de negócio obrigatória da SA)
            if (novaLocalizacao.Latitude < -90 || novaLocalizacao.Latitude > 90)
            {
                return BadRequest("A latitude deve estar entre -90 e 90 graus.");
            }

            if (novaLocalizacao.Longitude < -180 || novaLocalizacao.Longitude > 180)
            {
                return BadRequest("A longitude deve estar entre -180 e 180 graus.");
            }

            // 4. Se o Squad 1 não enviar a data, nós garantimos que ela é preenchida com o horário atual
            if (novaLocalizacao.Timestamp == default)
            {
                novaLocalizacao.Timestamp = DateTime.UtcNow;
            }

            try
            {
                // 5. Salva diretamente no Firestore (Coleção "localizacoes")
                CollectionReference colecao = _firestoreDb.Collection("localizacoes");
                DocumentReference docRef = await colecao.AddAsync(novaLocalizacao);

                // Atribui o ID gerado automaticamente pelo Firebase de volta ao objeto
                novaLocalizacao.Id = docRef.Id;

                // Retorna HTTP 201 (Created) com os dados finais salvos
                return Created("", new
                {
                    mensagem = "Localização validada e salva com sucesso no Firebase!",
                    dados = novaLocalizacao
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO FIREBASE]: {ex.Message}");
                return StatusCode(500, "Erro interno ao tentar salvar no banco de dados NoSQL.");
            }
        }
    }
}
/// <summary>
///  Delete Api para deletar um georeferenciamento existente, caso o id não exista retorna NotFound, caso exista deleta e retorna NoContent
/// </summary>
/// <param name="id"></param>
/// <returns></returns>
[HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> Delete(string id)
        {
           // var existente = await _service.GetById(id);
            //if (existente == null)
                return NotFound();

            await _service.Delete(id);
            return NoContent();
        }
    }
}
