using AutoMapper;
using Campaign.Watch.Application.Dtos.Campaign;
using Campaign.Watch.Application.Interfaces.Campaign;
using Campaign.Watch.Domain.Entities.Campaign;
using Campaign.Watch.Domain.Enums;
using Campaign.Watch.Domain.Interfaces.Services.Campaign;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Services.Campaign
{
    public class CampaignApplication : ICampaignApplication
    {
        private readonly ICampaignService _campaignService;
        private readonly IMapper _mapper;

        public CampaignApplication(ICampaignService campaignService, IMapper mapper)
        {
            _campaignService = campaignService;
            _mapper = mapper;
        }

        #region Banco de persistência (CRUD)

        public async Task<CampaignResponse> CreateCampaignAsync(CampaignResponse dto)
        {
            var entity = _mapper.Map<CampaignEntity>(dto);
            var created = await _campaignService.CreateCampaignAsync(entity);
            return _mapper.Map<CampaignResponse>(created);
        }

        public async Task<bool> UpdateCampaignAsync(string id, CampaignResponse dto)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return false;

            var entity = _mapper.Map<CampaignEntity>(dto);
            return await _campaignService.UpdateCampaignAsync(objectId, entity);
        }

        public async Task<IEnumerable<CampaignResponse>> GetAllCampaignsAsync()
        {
            var campaigns = await _campaignService.GetAllCampaignsAsync();
            return _mapper.Map<IEnumerable<CampaignResponse>>(campaigns);
        }

        public async Task<CampaignResponse> GetCampaignByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return null;
            }
            var campaign = await _campaignService.GetCampaignByIdAsync(objectId);
            return _mapper.Map<CampaignResponse>(campaign);
        }

        public async Task<CampaignResponse> GetCampaignByNameAsync(string campaignName)
        {
            var campaign = await _campaignService.GetCampaignByNameAsync(campaignName);
            return _mapper.Map<CampaignResponse>(campaign);
        }

        public async Task<CampaignResponse> GetCampaignByNumberAsync(long campaignNumber)
        {
            var campaign = await _campaignService.GetCampaignByNumberAsync(campaignNumber);
            return _mapper.Map<CampaignResponse>(campaign);
        }

        public async Task<IEnumerable<CampaignResponse>> GetAllCampaignsByClientAsync(string clientName)
        {
            var campaigns = await _campaignService.GetAllCampaignsByClientAsync(clientName);
            return _mapper.Map<IEnumerable<CampaignResponse>>(campaigns);
        }

        public async Task<IEnumerable<CampaignResponse>> GetCampaignsByStatusAsync(CampaignStatus status)
        {
            var campaigns = await _campaignService.GetCampaignsByStatusAsync(status);
            return _mapper.Map<IEnumerable<CampaignResponse>>(campaigns);
        }

        public async Task<IEnumerable<CampaignResponse>> GetCampaignsPaginatedAsync(int page, int pageSize)
        {
            var campaigns = await _campaignService.GetCampaignsPaginatedAsync(page, pageSize);
            return _mapper.Map<IEnumerable<CampaignResponse>>(campaigns);
        }

        public async Task<CampaignResponse> GetCampaignByIdCampaignAsync(string idCampaign)
        {
            var campaign = await _campaignService.GetCampaignByIdCampaignAsync(idCampaign);
            return _mapper.Map<CampaignResponse>(campaign);
        }

        #endregion
    }
}