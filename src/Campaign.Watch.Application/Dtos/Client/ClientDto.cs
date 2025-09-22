using Campaign.Watch.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Campaign.Watch.Application.Dtos.Client
{
    public class ClientDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public CampaignConfigDto CampaignConfig { get; set; }
        public List<EffectiveChannelDto> EffectiveChannels { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }

    public class CampaignConfigDto
    {
        public string ProjectID { get; set; }
        public string Database { get; set; }
    }

    public class EffectiveChannelDto
    {
        public TypeChannels TypeChannel { get; set; }
        public string Name { get; set; }
        public string Database { get; set; }
        public string TenantID { get; set; }
    }
}
