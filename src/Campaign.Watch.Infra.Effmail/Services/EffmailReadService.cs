using Campaign.Watch.Domain.Entities.Read.Effmail;
using Campaign.Watch.Domain.Interfaces.Services.Read.Effmail;
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

        public Task<IEnumerable<EffmailRead>> GetTriggerEffmail(string dbName, string workflowId)
        {
            throw new NotImplementedException();
        }
    }
}
