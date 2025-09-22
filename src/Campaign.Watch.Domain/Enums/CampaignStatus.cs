using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaign.Watch.Domain.Enums
{
    public enum CampaignStatus
    {
        Draft = 0,
        Completed = 1,
        Error = 3,
        Executing = 5,
        Scheduled = 7,
        Canceled = 8,
        Canceling = 9
    }
}
