using Campaign.Watch.Infra.Data.Factories.Common;
using MongoDB.Driver;

namespace Campaign.Watch.Infra.Campaign.Factories
{
    public class CampaignMongoFactory : ICampaignMongoFactory
    {
        private readonly IMongoDbFactory _factory;

        public CampaignMongoFactory(IMongoDbFactory factory)
        {
            _factory = factory;
        }

        public IMongoDatabase GetDatabase(string dbName)
        {
            return _factory.GetDatabase("MongoDB.Campaign", dbName);
        }
    }
}
