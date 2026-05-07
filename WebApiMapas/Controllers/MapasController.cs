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
        /// <remarks>
        /// GET: api/Mapas - Retorna todas as localizações cadastradas no Firebase.
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">Lista de localizações obtida com sucesso.</response>
        /// <response code="500">Erro ao listar localizações.</response>
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
                return StatusCode(500, new { mensagem = $"Erro ao listar localizações: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET BY ID: api/Mapas/{id} - Busca uma localização específica por ID.
        /// </summary>
        /// <remarks>
        /// GET BY ID: api/Mapas/{id} - Busca uma localização específica por ID.
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Localização encontrada com sucesso.</response>
        /// <response code="404">ID não encontrado.</response>
        /// <response code="500">Erro ao buscar localização por ID.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var localizacao = await _service.ObterPorId(id);
                if (localizacao == null)
                    return NotFound(new { mensagem = $"ID {id} não encontrado." });

                return Ok(new { mensagem = $"Localização encontrada: {localizacao}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao buscar localização por ID: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET BY LOGRADOURO: api/Mapas/logradouro/{logradouro} - Busca uma localização específica por logradouro.
        /// </summary>
        /// <remarks>
        /// GET BY LOGRADOURO: api/Mapas/logradouro/{logradouro} - Busca uma localização específica por logradouro.
        /// </remarks>
        /// <param name="logradouro"></param>
        /// <returns></returns>
        /// <response code="200">Localização encontrada com sucesso.</response>
        /// <response code="404">Logradouro não encontrado.</response>
        /// <response code="500">Erro ao buscar localização por logradouro.</response>
        [HttpGet("logradouro/{logradouro}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByLogradouro(string logradouro)
        {
            try
            {
                var localizacao = await _service.ObterPorLogradouro(logradouro);
                if (localizacao == null)
                    return NotFound(new { mensagem = $"Logradouro {logradouro} não encontrado." });

                return Ok(new { mensagem = $"Logradouro encontrado: {localizacao}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao buscar localização por logradouro: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST: api/Mapas - Recebe uma nova localização e a salva no Firebase.
        /// </summary>
        /// <remarks>
        /// POST: api/Mapas - Recebe uma nova localização e a salva no Firebase.
        /// </remarks>
        /// <param name="novaLocalizacao"></param>
        /// <returns></returns>
        /// <response code="201">Localização criada com sucesso.</response>
        /// <response code="400">Requisição inválida.</response>
        /// <response code="500">Erro ao criar localização.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SalvarLocalizacao([FromBody] Localizacao novaLocalizacao)
        {
            if (novaLocalizacao == null)
                return BadRequest(new
                {
                    erro = "Requisição inválida",
                    detalhe = "O corpo do JSON não pode estar vazio."
                });


            if (string.IsNullOrWhiteSpace(novaLocalizacao.Logradouro))
                return BadRequest(new
                {
                    erro = "Campo obrigatório",
                    detalhe = "O logradouro deve ser preenchido."
                });

            if (string.IsNullOrWhiteSpace(novaLocalizacao.Numero))
            {
                novaLocalizacao.Numero = "S/N";
            }

            if (string.IsNullOrWhiteSpace(novaLocalizacao.Bairro))
                return BadRequest(new
                {
                    erro = "Campo obrigatório",
                    detalhe = "O bairro deve ser preenchido."
                });

            if (string.IsNullOrWhiteSpace(novaLocalizacao.Cep))
                return BadRequest(new
                {
                    erro = "Campo obrigatório",
                    detalhe = "O CEP deve ser preenchido."
                });

            // Validação básica de formato (Ex: 34000-000 ou 34000000)
            if (novaLocalizacao.Cep.Length < 8)
                return BadRequest(new
                {
                    erro = "CEP inválido",
                    detalhe = "O CEP deve conter pelo menos 8 caracteres."
                });

            // Validação geográfica: A latitude deve estar entre -90 (Polo Sul) e 90 (Polo Norte)         
            if (novaLocalizacao.Latitude < -90 || novaLocalizacao.Latitude > 90)
            {
                return BadRequest(new
                {
                    erro = "Coordenada inválida",
                    detalhe = "A latitude deve estar entre -90 e 90 graus."
                });
            }

            // Validação geográfica: A longitude deve estar entre -180 (Oeste) e 180 (Leste).
            if (novaLocalizacao.Longitude < -180 || novaLocalizacao.Longitude > 180)
            {
                return BadRequest(new
                {
                    erro = "Coordenada inválida",
                    detalhe = "A longitude deve estar entre -180 e 180 graus."
                });
            }

            try
            {
                // Deixa o Service processar a gravação no Firebase
                var resultado = await _service.Criar(novaLocalizacao);
                return Created("", new
                {
                    mensagem = $"Localização salva com sucesso! ID: {resultado.Id}",
                    dados = resultado
                });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    erro = "Erro no servidor",
                    detalhe = ex.Message
                });
            }
        }

        /// <summary>
        /// PUT: api/Mapas/{id} - Atualiza uma localização específica por ID.
        /// </summary>
        /// <remarks>
        /// PUT: api/Mapas/{id} - Atualiza uma localização específica por ID.
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="localizacaoAtualizada"></param>
        /// <returns></returns>
        /// <response code="200">Localização atualizada com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="404">Localização não encontrada.</response>
        /// <response code="500">Erro ao atualizar localização.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Atualizar(string id, [FromBody] Localizacao atualizadaLocalizacao)
        {
            if (atualizadaLocalizacao == null)
                return BadRequest(new { erro = "Requisição inválida", detalhe = "O corpo do JSON não pode estar vazio." });
            try
            {
                var existente = await _service.ObterPorId(id);

                if (existente == null) 
                    return NotFound(new { mensagem = $"ID {id} não encontrado." });

                // Atualiza os campos necessários
                existente.Logradouro = atualizadaLocalizacao.Logradouro ?? existente.Logradouro;
                existente.Numero = atualizadaLocalizacao.Numero ?? existente.Numero;
                existente.Bairro = atualizadaLocalizacao.Bairro ?? existente.Bairro;
                existente.Cep = atualizadaLocalizacao.Cep ?? existente.Cep;
                existente.Latitude = atualizadaLocalizacao.Latitude != 0 ? atualizadaLocalizacao.Latitude : existente.Latitude;
                existente.Longitude = atualizadaLocalizacao.Longitude != 0 ? atualizadaLocalizacao.Longitude : existente.Longitude;

                await _service.Atualizar(id, existente);
                return Ok(new { mensagem = $"Localização com ID {id} atualizada com sucesso.", dados = existente });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = "Erro no servidor", detalhe = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: api/Mapas/{id} - Deleta uma localização específica por ID.
        /// </summary>
        /// <remarks>
        /// DELETE: api/Mapas/{id} - Deleta uma localização específica por ID.
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var existente = await _service.ObterPorId(id);

                if (existente == null)
                    return NotFound(new { erro = "Localização não encontrada", detalhe = $"Não foi possível encontrar a localização com ID: {id}" });

                await _service.Delete(id);
                return Ok(new { mensagem = "Localização deletada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = $"Erro ao deletar: {ex.Message}" });
            }
        }
    }
}