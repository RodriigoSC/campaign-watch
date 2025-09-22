using Campaign.Watch.Domain.Entities.Read;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaign.Watch.Domain.Interfaces.Services.Read
{
    public interface IEffmailReadService
    {
        Task<IEnumerable<EffmailReadOnly>> GetTriggerEffmail(string dbName, string workflowId);
    }
}
