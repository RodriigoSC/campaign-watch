using Campaign.Watch.Domain.Enums;
using MongoDB.Driver;

namespace Campaign.Watch.Infra.Data.Factories.Common
{
    public interface IMongoDbFactory
    {
        IMongoDatabase GetDatabase(string connectionKey, string databaseName);
    }
}
