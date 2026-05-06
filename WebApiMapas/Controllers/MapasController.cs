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

        /// <summary>
        /// Construtor da classe MapasController.
        /// </summary>
        /// <param name="service">Serviço de localização injetado via DI.</param>
        public MapasController(LocalizacaoService service)
        {
            _service = service;
        }

        /// <summary>
        /// POST: api/Mapas - Cria uma nova localização georeferenciada.
        /// </summary>
        /// <remarks>
        /// O endpoint recebe um objeto Localizacao no corpo da requisição e tenta criar um novo registro 
        /// no Firebase.
        /// </remarks>
        /// <param name="localizacao">Objeto contendo os dados da localização 
        /// (Latitude, Longitude, Nome).</param>
        /// <returns>Retorna a localização criada com seu respectivo ID.</returns>        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] Localizacao localizacao)
        {
            try
            {
                if (localizacao == null)
                {
                    return BadRequest(new
                    {
                        erro = "Requisição Inválida",
                        mensagem = "O campo da localização não pode ser nulo.",
                        detalhe = "Certifique-se que os dados estão no formato correto."
                    });
                }

                var novaLocalizacao = await _service.Criar(localizacao);

                // Retorno 201: Indica sucesso ao gravar o registro no banco
                return CreatedAtAction(nameof(GetById), new { id = novaLocalizacao.Id }, novaLocalizacao);
            }

            // Validação para localização já existente (ex: coordenadas duplicadas ou mesmo nome)
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    erro = "Conflito",
                    mensagem = "Não foi possível cadastrar a localização pois ela já existe.",
                    detalhe = ex.Message
                });
            }

            // Validação para campos obrigatórios ou valores inválidos (ex: Lat fora de -90 a 90)
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    erro = "Dados Inválidos",
                    mensagem = "Os dados fornecidos estão incorretos ou fora do padrão.",
                    detalhe = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    erro = "Erro Interno do Servidor",
                    mensagem = "Ocorreu um erro inesperado ao salvar a localização.",
                    detalhe = ex.Message
                });
            }
        }

        /// <summary>
        ///  Delete Api para deletar um georeferenciamento existente, caso o id não exista retorna NotFound, caso exista deleta e retorna NoContent
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
