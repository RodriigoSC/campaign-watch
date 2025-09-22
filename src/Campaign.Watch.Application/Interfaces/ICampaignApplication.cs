using Campaign.Watch.Application.Dtos;
using Campaign.Watch.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Interfaces
{
    public interface ICampaignApplication
    {
        #region Banco de persistência (CRUD)

        Task<CampaignMonitoringDto> CreateCampaignAsync(CampaignDto dto);
        Task<bool> UpdateCampaignAsync(string id, CampaignMonitoringDto dto);
        Task<IEnumerable<CampaignDto>> GetAllCampaignsAsync();
        Task<CampaignDto> GetCampaignByIdAsync(string id);
        Task<CampaignDto> GetCampaignByNameAsync(string campaignName);
        Task<CampaignDto> GetCampaignByNumberAsync(long campaignNumber);
        Task<IEnumerable<CampaignDto>> GetAllCampaignsByClientAsync(string clientName);
        Task<IEnumerable<CampaignDto>> GetCampaignsByStatusAsync(CampaignStatus status);
        Task<IEnumerable<CampaignDto>> GetCampaignsPaginatedAsync(int page, int pageSize);
        Task<CampaignMonitoringDto> GetCampaignByIdCampaignAsync(string idCampaign);

        #endregion
    }
}
