using Campaign.Watch.Domain.Enums;

namespace Campaign.Watch.Domain.Entities.Campaign
{
    /// <summary>
    /// Modelo auxiliar para armazenar o resultado da agregação de contagem de status.
    /// </summary>
    public class CampaignStatusCount
    {
        public MonitoringStatus Status { get; set; }
        public int Count { get; set; }
    }
}
