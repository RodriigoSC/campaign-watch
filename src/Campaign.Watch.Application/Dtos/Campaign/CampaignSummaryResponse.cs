using Campaign.Watch.Domain.Enums;
using System;

namespace Campaign.Watch.Application.Dtos.Campaign
{
    public class CampaignSummaryResponse
    {
        public string Id { get; set; }
        public string ClientName { get; set; }
        public long NumberId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public CampaignStatus StatusCampaign { get; set; }
        public MonitoringStatus MonitoringStatus { get; set; }
        public DateTime? NextExecutionMonitoring { get; set; }
        public DateTime? LastCheckMonitoring { get; set; }
        public MonitoringHealthStatusDto HealthStatus { get; set; }
    }
}
