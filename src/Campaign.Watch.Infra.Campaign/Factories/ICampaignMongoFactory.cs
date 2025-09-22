using MongoDB.Driver;

namespace Campaign.Watch.Infra.Campaign.Factories
{
    public interface ICampaignMongoFactory
    {
        IMongoDatabase GetDatabase(string dbName);
    }
}
