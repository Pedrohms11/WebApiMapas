using Google.Cloud.Firestore;
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
            // Validação simples exigida no seu escopo da SA:
            if (novaLocalizacao.Latitude < -90 || novaLocalizacao.Latitude > 90 ||
                novaLocalizacao.Longitude < -180 || novaLocalizacao.Longitude > 180)
            {
                return BadRequest("Coordenadas geográficas inválidas!");
            }

            try
            {
                // A coleção "localizacoes" é criada sozinha se não existir ainda!
                CollectionReference colecao = _firestoreDb.Collection("localizacoes");

                // Salva no Firestore
                DocumentReference docRef = await colecao.AddAsync(novaLocalizacao);

                // Adiciona o ID gerado ao objeto de retorno
                novaLocalizacao.Id = docRef.Id;

                return Ok(new { mensagem = "Localização persistida com sucesso!", dados = novaLocalizacao });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao salvar no Firebase: {ex.Message}");
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

        public async Task<IActionResult> Delete(int id)
        {
            var existente = await _service.GetById(id);
            if (existente == null)
                return NotFound();

            await _service.Delete(id);
            return NoContent();
        }
    }
}
