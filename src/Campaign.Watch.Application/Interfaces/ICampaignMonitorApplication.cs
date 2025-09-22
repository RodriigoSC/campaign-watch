using Campaign.Watch.Application.Dtos.Read.Campaign;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Interfaces
{
    public interface ICampaignMonitorApplication
    {
        #region Banco de origem (monitoramento)

        Task<IEnumerable<CampaignReadDto>> GetSourceCampaignsByClientAsync(string dbName);
        Task<IEnumerable<ExecutionReadDto>> GetSourceExecutionsByCampaignAsync(string dbName, string campaignId);

        #endregion
    }
}
