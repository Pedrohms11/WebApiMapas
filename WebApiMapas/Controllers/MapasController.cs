using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing; 
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WebApiMapas.Models;
using WebApiMapas.Service;
using Microsoft.AspNetCore.Mvc;

namespace WebApiMapas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapasController : ControllerBase
    {
        private readonly LocalizacaoService _service;

        public MapasController(LocalizacaoService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET: api/Mapas - Retorna todas as localizações cadastradas no Firebase.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                // O Service cuida da lógica de listar
                var localizacoes = await _service.Listar();
                return Ok(new { mensagem = "Lista obtida com sucesso.", localizacoes });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao listar localizações: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET: api/Mapas/{id} - Busca uma localização específica por ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var localizacao = await _service.ObterPorId(id);
                if (localizacao == null)
                    return NotFound(new { mensagem = $"ID {id} não encontrado." });

                return Ok(localizacao);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/Mapas - Recebe e valida as coordenadas antes de salvar.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SalvarLocalizacao([FromBody] Localizacao novaLocalizacao)
        {
            if (novaLocalizacao == null) return BadRequest("Corpo vazio.");

            if (string.IsNullOrWhiteSpace(novaLocalizacao.Logradouro))
                return BadRequest("O campo 'logradouro' é obrigatório.");

            // Regra de negócio da SA
            if (novaLocalizacao.Latitude < -90 || novaLocalizacao.Latitude > 90 ||
                novaLocalizacao.Longitude < -180 || novaLocalizacao.Longitude > 180)
            {
                return BadRequest("Coordenadas geográficas inválidas.");
            }

            try
            {
                // Deixa o Service processar a gravação no Firebase
                var resultado = await _service.Criar(novaLocalizacao);
                return Created("", new { mensagem = "Salvo com sucesso!", dados = resultado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao salvar: {ex.Message}");
            }
        }

        /// <summary>
        /// DELETE: api/Mapas/{id} - Remove o registro do Firebase.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var existente = await _service.ObterPorId(id);
                if (existente == null) return NotFound();

                await _service.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao deletar: {ex.Message}");
            }
        }
    }
}