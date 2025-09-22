using Campaign.Watch.Domain.Entities;
using Campaign.Watch.Domain.Enums;
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
    public class CampaignRepository : CommonRepository<CampaignEntity>, ICampaignRepository
    {        
        public CampaignRepository(IPersistenceMongoFactory persistenceFactory) : base(persistenceFactory, "CampaignMonitoring")
        {
            var indexKeys = Builders<CampaignEntity>.IndexKeys.Ascending(x => x.Name);
            var indexModel = new CreateIndexModel<CampaignEntity>(
                indexKeys,
                new CreateIndexOptions { Unique = true }
            );

            CreateIndexesAsync(new List<CreateIndexModel<CampaignEntity>> { indexModel }).GetAwaiter().GetResult();
        }

        public async Task<CampaignEntity> CreateCampaignAsync(CampaignEntity entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<bool> UpdateCampaignAsync(ObjectId id, CampaignEntity entity)
        {
            entity.ModifiedAt = DateTime.UtcNow;
            var result = await _collection.ReplaceOneAsync(c => c.Id == id, entity);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<CampaignEntity> GetCampaignByIdAsync(ObjectId id)
        {
            return await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsAsync()
        {
            return await _collection.Find(FilterDefinition<CampaignEntity>.Empty).ToListAsync();
        }

        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsByClientAsync(string clientName)
        {
            return await _collection.Find(c => c.ClientName == clientName).ToListAsync();
        }

        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsByDateAsync(DateTime start, DateTime finish)
        {
            return await _collection.Find(c => c.CreatedAt >= start && c.CreatedAt <= finish).ToListAsync();
        }

        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsByClientOrDateAsync(string clientName, DateTime start, DateTime finish)
        {
            var filter = Builders<CampaignEntity>.Filter.And(
                Builders<CampaignEntity>.Filter.Eq(c => c.ClientName, clientName),
                Builders<CampaignEntity>.Filter.Gte(c => c.CreatedAt, start),
                Builders<CampaignEntity>.Filter.Lte(c => c.CreatedAt, finish)
            );
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<CampaignEntity>> GetActiveCampaignsAsync()
        {
            return await _collection.Find(c => c.IsActive == true).ToListAsync();
        }

        public async Task<IEnumerable<CampaignEntity>> GetCampaignsByStatusAsync(CampaignStatus status)
        {
            return await _collection.Find(c => c.StatusCampaign == status).ToListAsync();
        }

        public async Task<int> CountCampaignsByClientAsync(string clientName)
        {
            return (int)await _collection.CountDocumentsAsync(c => c.ClientName == clientName);
        }

        public async Task<IEnumerable<CampaignEntity>> GetCampaignsPaginatedAsync(int page, int pageSize)
        {
            return await _collection.Find(FilterDefinition<CampaignEntity>.Empty)
                                     .Skip((page - 1) * pageSize)
                                     .Limit(pageSize)
                                     .ToListAsync();
        }

        public async Task<CampaignEntity> GetCampaignByNameAsync(string campaignName)
        {
            return await _collection.Find(c => c.Name == campaignName).FirstOrDefaultAsync();
        }

        public async Task<CampaignEntity> GetCampaignByNumberAsync(long campaignNumber)
        {
            return await _collection.Find(c => c.NumberId == campaignNumber).FirstOrDefaultAsync();
        }
    }
}