using AutoMapper;
using Campaign.Watch.Application.Dtos.Read.Campaign;
using Campaign.Watch.Application.Interfaces;
using Campaign.Watch.Domain.Interfaces.Services.Read;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Services
{
    public class CampaignMonitorApplication : ICampaignMonitorApplication
    {
        private readonly ICampaignReadService _campaignReadService;
        private readonly IEffmailReadService _effmailReadService;
        private readonly IMapper _mapper;

        public CampaignMonitorApplication(ICampaignReadService campaignReadService, IMapper mapper)
        {
            _campaignReadService = campaignReadService;
            _mapper = mapper;
        }


        #region Banco de origem (monitoramento)

        public async Task<IEnumerable<CampaignReadDto>> GetSourceCampaignsByClientAsync(string dbName)
        {
            var sourceCampaigns = await _campaignReadService.GetCampaignsByClient(dbName);
            return _mapper.Map<IEnumerable<CampaignReadDto>>(sourceCampaigns);
        }

        public async Task<IEnumerable<ExecutionReadDto>> GetSourceExecutionsByCampaignAsync(string dbName, string campaignId)
        {
            var sourceExecutions = await _campaignReadService.GetExecutionsByCampaign(dbName, campaignId);
            return _mapper.Map<IEnumerable<ExecutionReadDto>>(sourceExecutions);
        }

        #endregion

    }
}
