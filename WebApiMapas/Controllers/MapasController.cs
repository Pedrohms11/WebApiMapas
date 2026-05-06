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

        private readonly FirestoreDb _firestoreDb;

        /// <summary>
        /// Construtor da classe MapasController.
        /// </summary>
        /// <param name="service">Serviço de localização injetado via DI.</param>
        public MapasController(LocalizacaoService service)
        {
            _service = service;
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
