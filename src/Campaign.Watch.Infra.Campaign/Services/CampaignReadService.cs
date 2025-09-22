using Campaign.Watch.Domain.Entities.Read;
using Campaign.Watch.Domain.Interfaces.Services.Read;
using Campaign.Watch.Infra.Campaign.Factories;
using MongoDB.Driver;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace Campaign.Watch.Infra.Campaign.Services
{
    public class CampaignReadService : ICampaignReadService
    {
        private readonly ICampaignMongoFactory _factory;

        public CampaignReadService(ICampaignMongoFactory factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<CampaignReadModel>> GetCampaignsByClient(string dbName)
        {
            var db = _factory.GetDatabase(dbName);
            var collection = db.GetCollection<CampaignReadModel>("Campaign");
            return await collection.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<ExecutionReadModel>> GetExecutionsByCampaign(string dbName, string campaignId)
        {
            var db = _factory.GetDatabase(dbName);
            var collection = db.GetCollection<ExecutionReadModel>("ExecutionPlan");

            return await collection
                .Find(x => x.CampaignId.ToString() == campaignId && x.FlagCount == false)
                .ToListAsync();
        }

    }
}
