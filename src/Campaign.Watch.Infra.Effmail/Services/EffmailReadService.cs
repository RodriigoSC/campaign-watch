using Campaign.Watch.Domain.Entities.Read;
using Campaign.Watch.Domain.Interfaces.Services.Read;
using Campaign.Watch.Infra.Effmail.Factories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Infra.Effmail.Services
{
    public class EffmailReadService : IEffmailReadService
    {
        private readonly IEffmailMongoFactory _factory;

        public EffmailReadService(IEffmailMongoFactory factory)
        {
            _factory = factory;
        }

        public Task<IEnumerable<EffmailReadOnly>> GetTriggerEffmail(string dbName, string workflowId)
        {
            throw new NotImplementedException();
        }
    }
}
