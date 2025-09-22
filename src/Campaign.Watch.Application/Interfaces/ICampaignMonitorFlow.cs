using Campaign.Watch.Application.Dtos;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Interfaces
{
    public interface ICampaignMonitorFlow
    {
        Task MonitorCampaignsAsync();
        Task ValidateCampaignAsync(CampaignDto campaignDto);
    }
}
