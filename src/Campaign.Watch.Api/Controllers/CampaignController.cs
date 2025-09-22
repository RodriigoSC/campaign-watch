using Campaign.Watch.Application.Dtos;
using Campaign.Watch.Application.Interfaces;
using Campaign.Watch.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignApplication _campaignApplication;

        public CampaignController(ICampaignApplication campaignApplication)
        {
            _campaignApplication = campaignApplication;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CampaignDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var campaigns = await _campaignApplication.GetAllCampaignsAsync();
            return Ok(campaigns);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CampaignDto), 200)]
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

        [HttpGet("by-name/{campaignName}")]
        [ProducesResponseType(typeof(CampaignDto), 200)]
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

        [HttpGet("by-number/{campaignNumber}")]
        [ProducesResponseType(typeof(CampaignDto), 200)]
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

        [HttpGet("by-client/{clientName}")]
        [ProducesResponseType(typeof(IEnumerable<CampaignDto>), 200)]
        public async Task<IActionResult> GetByClient(string clientName)
        {
            var campaigns = await _campaignApplication.GetAllCampaignsByClientAsync(clientName);
            return Ok(campaigns);
        }

        [HttpGet("by-status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<CampaignDto>), 200)]
        public async Task<IActionResult> GetByStatus(CampaignStatus status)
        {
            var campaigns = await _campaignApplication.GetCampaignsByStatusAsync(status);
            return Ok(campaigns);
        }

        [HttpGet("paginated")]
        [ProducesResponseType(typeof(IEnumerable<CampaignDto>), 200)]
        public async Task<IActionResult> GetPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var campaigns = await _campaignApplication.GetCampaignsPaginatedAsync(page, pageSize);
            return Ok(campaigns);
        }
    }
}
