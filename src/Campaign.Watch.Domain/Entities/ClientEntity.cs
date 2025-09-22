using Campaign.Watch.Domain.Entities.Common;
using Campaign.Watch.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Campaign.Watch.Domain.Entities
{
    public class ClientEntity : CommonFields
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public CampaignConfig CampaignConfig { get; set; }
        public List<EffectiveChannel> EffectiveChannels { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }

    public class CampaignConfig
    {
        public string ProjectID { get; set; }
        public string Database { get; set; }
    }

    [BsonKnownTypes(
    typeof(EffectiveMail),
    typeof(EffectiveSms),
    typeof(EffectivePush),
    typeof(EffectivePages),
    typeof(EffectiveSocial),
    typeof(EffectiveWhastApp)
    )]
    public abstract class EffectiveChannel
    {
        public TypeChannels TypeChannel { get; set; }
        public string Name { get; set; }
        public string Database { get; set; }
        public string TenantID { get; set; }
    }

    public class EffectiveMail : EffectiveChannel { }
    public class EffectiveSms : EffectiveChannel { }
    public class EffectivePush : EffectiveChannel { }
    public class EffectivePages : EffectiveChannel { }
    public class EffectiveSocial : EffectiveChannel { }
    public class EffectiveWhastApp : EffectiveChannel { }

}
