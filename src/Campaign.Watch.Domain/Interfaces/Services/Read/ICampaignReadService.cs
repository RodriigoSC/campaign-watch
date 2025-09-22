using Campaign.Watch.Domain.Entities.Read;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Domain.Interfaces.Services.Read
{
    public interface ICampaignReadService
    {
        Task<IEnumerable<CampaignReadModel>> GetCampaignsByClient(string dbName);
        Task<IEnumerable<ExecutionReadModel>> GetExecutionsByCampaign(string dbName, string campaignId);
    }
}
