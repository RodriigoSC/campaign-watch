using Campaign.Watch.Domain.Entities;
using Campaign.Watch.Domain.Interfaces.Repositories;
using Campaign.Watch.Infra.Data.Factories;
using Campaign.Watch.Infra.Data.Repository.Common;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Infra.Data.Repository
{
    public class ClientRepository : CommonRepository<ClientEntity>, IClientRepository
    {
        public ClientRepository(IPersistenceMongoFactory persistenceFactory) : base(persistenceFactory, "Clients")
        {
            var indexKeys = Builders<ClientEntity>.IndexKeys.Ascending(x => x.Name);
            var indexModel = new CreateIndexModel<ClientEntity>(
                indexKeys,
                new CreateIndexOptions { Unique = true }
            );

            CreateIndexesAsync(new List<CreateIndexModel<ClientEntity>> { indexModel }) .GetAwaiter() .GetResult();
        }

        public async Task<ClientEntity> CreateAsync(ClientEntity client)
        {
            await _collection.InsertOneAsync(client);
            return client;
        }

        public async Task<IEnumerable<ClientEntity>> GetAllAsync()
        {
            return await _collection.Find(Builders<ClientEntity>.Filter.Empty).ToListAsync();
        }

        public async Task<ClientEntity> GetByIdAsync(ObjectId id)
        {
            return await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<ClientEntity> GetByNameAsync(string clientName)
        {
            return await _collection.Find(c => c.Name == clientName).FirstOrDefaultAsync();
        }

        
        public async Task<bool> UpdateAsync(ObjectId id, ClientEntity client)
        {
            var updateDefinition = Builders<ClientEntity>.Update
                .Set(c => c.Name, client.Name)
                .Set(c => c.IsActive, client.IsActive)
                .Set(c => c.CampaignConfig, client.CampaignConfig)
                .Set(c => c.EffectiveChannels, client.EffectiveChannels)
                .Set(c => c.ModifiedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(c => c.Id == id, updateDefinition);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(ObjectId id)
        {
            var result = await _collection.DeleteOneAsync(c => c.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}