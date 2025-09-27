using Campaign.Watch.Application.Dtos.Campaign;
using Campaign.Watch.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Interfaces.Campaign
{
    public interface ICampaignApplication
    {
        #region Banco de persistência

        Task<CampaignDetailResponse> CreateCampaignAsync(CampaignDetailResponse dto);
        Task<bool> UpdateCampaignAsync(string id, CampaignDetailResponse dto);
        Task<IEnumerable<CampaignDetailResponse>> GetAllCampaignsAsync();
        Task<CampaignDetailResponse> GetCampaignByIdAsync(string id);
        Task<CampaignDetailResponse> GetCampaignByNameAsync(string campaignName);
        Task<CampaignDetailResponse> GetCampaignByNumberAsync(long campaignNumber);
        Task<IEnumerable<CampaignDetailResponse>> GetAllCampaignsByClientAsync(string clientName);
        Task<IEnumerable<CampaignStatusResponse>> GetCampaignsByStatusAsync(CampaignStatus status);
        Task<IEnumerable<CampaignDetailResponse>> GetCampaignsPaginatedAsync(int page, int pageSize);
        Task<CampaignDetailResponse> GetCampaignByIdCampaignAsync(string clientName, string idCampaign);
        Task<IEnumerable<CampaignDetailResponse>> GetCampaignsDueForMonitoringAsync();
        Task<IEnumerable<CampaignDetailResponse>> GetCampaignsWithIntegrationErrorsAsync();
        Task<IEnumerable<CampaignDetailResponse>> GetCampaignsWithDelayedExecutionAsync();
        Task<IEnumerable<CampaignDetailResponse>> GetSuccessfullyMonitoredCampaignsAsync();

        #endregion
    }
}
