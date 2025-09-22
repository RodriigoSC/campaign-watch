using Campaign.Watch.Domain.Entities;
using Campaign.Watch.Domain.Enums;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Domain.Interfaces.Repositories
{
    public interface ICampaignRepository
    {
        Task<CampaignEntity> CreateCampaignAsync(CampaignEntity entity);
        Task<bool> UpdateCampaignAsync(ObjectId id, CampaignEntity entity);

        Task<IEnumerable<CampaignEntity>> GetAllCampaignsAsync();
        Task<IEnumerable<CampaignEntity>> GetAllCampaignsByClientAsync(string clientName);
        Task<IEnumerable<CampaignEntity>> GetAllCampaignsByDateAsync(DateTime start, DateTime finish);
        Task<IEnumerable<CampaignEntity>> GetAllCampaignsByClientOrDateAsync(string clientName, DateTime start, DateTime finish);
        Task<CampaignEntity> GetCampaignByIdAsync(ObjectId id);
        Task<CampaignEntity> GetCampaignByNameAsync(string campaignName);
        Task<CampaignEntity> GetCampaignByNumberAsync(long campaignNumber);
        Task<CampaignEntity> GetCampaignByIdCampaignAsync(string idCampaign);

        Task<IEnumerable<CampaignEntity>> GetActiveCampaignsAsync();
        Task<IEnumerable<CampaignEntity>> GetCampaignsByStatusAsync(CampaignStatus statusCampaign);
        Task<IEnumerable<CampaignEntity>> GetCampaignsPaginatedAsync(int page, int pageSize);
        Task<int> CountCampaignsByClientAsync(string clientName);
    }
}
