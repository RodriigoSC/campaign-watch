using Campaign.Watch.Infra.Data.Factories.Common;
using DTM_Vault.Data.KeyValue;
using MongoDB.Driver;

namespace Campaign.Watch.Infra.Data.Factories
{
    public class PersistenceMongoFactory : IPersistenceMongoFactory
    {
        private readonly IMongoDbFactory _factory;
        private readonly IVaultService _vault;
        private readonly string _environment;

        public PersistenceMongoFactory(IMongoDbFactory factory, IVaultService vault, string environment)
        {
            _factory = factory;
            _vault = vault;
            _environment = environment;
        }

        public IMongoDatabase GetDatabase()
        {
            var dbName = _vault.GetKeyAsync($"monitoring/{_environment}/data/keys", "MongoDB.Persistence.database")
                               .GetAwaiter().GetResult();

            return _factory.GetDatabase("MongoDB.Persistence", dbName);
        }
    }
}
