using Campaign.Watch.Application.Dtos.Campaign;
using Campaign.Watch.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Interfaces.Campaign
{
    public interface ICampaignApplication
    {
        #region Banco de persistência (CRUD)

        Task<CampaignResponse> CreateCampaignAsync(CampaignResponse dto);
        Task<bool> UpdateCampaignAsync(string id, CampaignResponse dto);
        Task<IEnumerable<CampaignResponse>> GetAllCampaignsAsync();
        Task<CampaignResponse> GetCampaignByIdAsync(string id);
        Task<CampaignResponse> GetCampaignByNameAsync(string campaignName);
        Task<CampaignResponse> GetCampaignByNumberAsync(long campaignNumber);
        Task<IEnumerable<CampaignResponse>> GetAllCampaignsByClientAsync(string clientName);
        Task<IEnumerable<CampaignResponse>> GetCampaignsByStatusAsync(CampaignStatus status);
        Task<IEnumerable<CampaignResponse>> GetCampaignsPaginatedAsync(int page, int pageSize);
        Task<CampaignResponse> GetCampaignByIdCampaignAsync(string idCampaign);

        #endregion
    }
}
