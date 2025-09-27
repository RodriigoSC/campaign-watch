using Campaign.Watch.Domain.Enums;

namespace Campaign.Watch.Application.Dtos.Campaign
{
    public class CampaignStatusResponse
    {
        public string Id { get; set; }
        public string ClientName { get; set; }
        public string IdCampaign { get; set; }
        public long NumberId { get; set; }
        public string Name { get; set; }
        public string CampaignType { get; set; }
        public string Description { get; set; }
        public string StatusCampaign { get; set; }
    }
}
