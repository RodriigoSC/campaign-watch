using Campaign.Watch.Application.Dtos.Campaign;
using Campaign.Watch.Application.Interfaces.Campaign;
using Campaign.Watch.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Api.Controllers
{
    /// <summary>
    /// Controller da API para consultar informações sobre as campanhas monitoradas.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignApplication _campaignApplication;

        /// <summary>
        /// Inicializa uma nova instância do CampaignController.
        /// </summary>
        /// <param name="campaignApplication">A camada de aplicação da campanha a ser injetada.</param>
        public CampaignController(ICampaignApplication campaignApplication)
        {
            _campaignApplication = campaignApplication;
        }        

        /// <summary>
        /// Obtém uma campanha específica pelo seu ID único.
        /// </summary>
        /// <param name="id">O ID da campanha.</param>
        /// <returns>Os dados da campanha.</returns>
        /// <response code="200">Retorna os dados da campanha encontrada.</response>
        /// <response code="404">Se a campanha não for encontrada.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CampaignDetailResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(string id)
        {
            var campaign = await _campaignApplication.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound();
            }
            return Ok(campaign);
        }

        /// <summary>
        /// Obtém uma campanha específica pelo seu nome.
        /// </summary>
        /// <param name="campaignName">O nome da campanha.</param>
        /// <returns>Os dados da campanha.</returns>
        /// <response code="200">Retorna os dados da campanha encontrada.</response>
        /// <response code="404">Se a campanha não for encontrada.</response>
        [HttpGet("by-name/{campaignName}")]
        [ProducesResponseType(typeof(CampaignDetailResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByName(string campaignName)
        {
            var campaign = await _campaignApplication.GetCampaignByNameAsync(campaignName);
            if (campaign == null)
            {
                return NotFound();
            }
            return Ok(campaign);
        }

        /// <summary>
        /// Obtém uma campanha específica pelo seu ID numérico.
        /// </summary>
        /// <param name="campaignNumber">O ID numérico da campanha.</param>
        /// <returns>Os dados da campanha.</returns>
        /// <response code="200">Retorna os dados da campanha encontrada.</response>
        /// <response code="404">Se a campanha não for encontrada.</response>
        [HttpGet("by-number/{campaignNumber}")]
        [ProducesResponseType(typeof(CampaignDetailResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByNumber(long campaignNumber)
        {
            var campaign = await _campaignApplication.GetCampaignByNumberAsync(campaignNumber);
            if (campaign == null)
            {
                return NotFound();
            }
            return Ok(campaign);
        }

        /// <summary>
        /// Obtém todas as campanhas de um cliente específico.
        /// </summary>
        /// <param name="clientName">O nome do cliente.</param>
        /// <returns>Uma lista de campanhas do cliente especificado.</returns>
        /// <response code="200">Retorna a lista de campanhas.</response>
        [HttpGet("by-client/{clientName}")]
        [ProducesResponseType(typeof(IEnumerable<CampaignDetailResponse>), 200)]
        public async Task<IActionResult> GetByClient(string clientName)
        {
            var campaigns = await _campaignApplication.GetAllCampaignsByClientAsync(clientName);
            return Ok(campaigns);
        }

        /// <summary>
        /// Obtém todas as campanhas com um status de monitoramento específico.
        /// </summary>
        /// <param name="status">O status da campanha (ex: Draft, Completed, Executing).</param>
        /// <returns>Uma lista de campanhas que correspondem ao status.</returns>
        /// <response code="200">Retorna a lista de campanhas.</response>
        [HttpGet("by-status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<CampaignStatusResponse>), 200)]
        public async Task<IActionResult> GetByStatus(CampaignStatus status)
        {
            var campaigns = await _campaignApplication.GetCampaignsByStatusAsync(status);
            return Ok(campaigns);
        }

        /// <summary>
        /// Obtém uma lista paginada de campanhas.
        /// </summary>
        /// <param name="page">O número da página a ser retornada (padrão: 1).</param>
        /// <param name="pageSize">O número de itens por página (padrão: 10).</param>
        /// <returns>Uma lista paginada de campanhas.</returns>
        /// <response code="200">Retorna a lista de campanhas para a página especificada.</response>
        [HttpGet("paginated")]
        [ProducesResponseType(typeof(IEnumerable<CampaignDetailResponse>), 200)]
        public async Task<IActionResult> GetPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var campaigns = await _campaignApplication.GetCampaignsPaginatedAsync(page, pageSize);
            return Ok(campaigns);
        }

        /// <summary>
        /// Obtém todas as campanhas com erro de integração.
        /// </summary>
        /// <returns>Uma lista de campanhas com erro de integração.</returns>
        /// <response code="200">Retorna a lista de campanhas.</response>
        [HttpGet("with-integration-errors")]
        [ProducesResponseType(typeof(IEnumerable<CampaignDetailResponse>), 200)]
        public async Task<IActionResult> GetWithIntegrationErrors()
        {
            var campaigns = await _campaignApplication.GetCampaignsWithIntegrationErrorsAsync();
            return Ok(campaigns);
        }

        /// <summary>
        /// Obtém todas as campanhas com execução atrasada.
        /// </summary>
        /// <returns>Uma lista de campanhas com execução atrasada.</returns>
        /// <response code="200">Retorna a lista de campanhas.</response>
        [HttpGet("with-delayed-execution")]
        [ProducesResponseType(typeof(IEnumerable<CampaignDetailResponse>), 200)]
        public async Task<IActionResult> GetWithDelayedExecution()
        {
            var campaigns = await _campaignApplication.GetCampaignsWithDelayedExecutionAsync();
            return Ok(campaigns);
        }

        /// <summary>
        /// Obtém todas as campanhas monitoradas com sucesso.
        /// </summary>
        /// <returns>Uma lista de campanhas monitoradas com sucesso.</returns>
        /// <response code="200">Retorna a lista de campanhas.</response>
        [HttpGet("successfully-monitored")]
        [ProducesResponseType(typeof(IEnumerable<CampaignDetailResponse>), 200)]
        public async Task<IActionResult> GetSuccessfullyMonitored()
        {
            var campaigns = await _campaignApplication.GetSuccessfullyMonitoredCampaignsAsync();

            // ----- INÍCIO DO CÓDIGO DE DEBUG -----
            try
            {
                // Esta linha vai forçar o serializador a ler todos os dados,
                // mas dentro de um try/catch para podermos inspecionar o erro.
                var json = System.Text.Json.JsonSerializer.Serialize(campaigns);
            }
            catch (Exception ex)
            {
                // Se o código chegar aqui, a teoria está correta.
                // Coloque um breakpoint (ponto de parada) na linha abaixo para
                // inspecionar a variável 'ex'. A InnerException vai mostrar
                // o erro de BsonString para BsonBoolean.
                return BadRequest("Erro de serialização detectado: " + ex.ToString());
            }
            // ----- FIM DO CÓDIGO DE DEBUG -----

            return Ok(campaigns);
        }
    }
}