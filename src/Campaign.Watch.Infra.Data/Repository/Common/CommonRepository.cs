using Campaign.Watch.Infra.Data.Factories;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Infra.Data.Repository.Common
{
    public abstract class CommonRepository<TEntity> where TEntity : class
    {
        protected readonly IMongoCollection<TEntity> _collection;

        protected CommonRepository(IPersistenceMongoFactory persistenceFactory, string collectionName)
        {
            var database = persistenceFactory.GetDatabase();
            _collection = database.GetCollection<TEntity>(collectionName);
        }

        protected async Task CreateIndexesAsync(IEnumerable<CreateIndexModel<TEntity>> indexes)
        {
            if (indexes != null)
            {
                await _collection.Indexes.CreateManyAsync(indexes);
            }
        }
    }
}
