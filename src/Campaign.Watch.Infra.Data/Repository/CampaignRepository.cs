using Campaign.Watch.Domain.Entities.Campaign;
using Campaign.Watch.Domain.Enums;
using Campaign.Watch.Domain.Interfaces.Repositories.Campaign;
using Campaign.Watch.Infra.Data.Factories;
using Campaign.Watch.Infra.Data.Repository.Common;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Infra.Data.Repository
{
    /// <summary>
    /// Repositório para gerenciar as operações de dados da entidade CampaignEntity.
    /// </summary>
    public class CampaignRepository : CommonRepository<CampaignEntity>, ICampaignRepository
    {
        /// <summary>
        /// Inicializa uma nova instância da classe CampaignRepository.
        /// Configura a coleção "CampaignMonitoring" e garante a criação de um índice único para o nome da campanha.
        /// </summary>
        /// <param name="persistenceFactory">A fábrica para obter a instância do banco de dados.</param>
        public CampaignRepository(IPersistenceMongoFactory persistenceFactory) : base(persistenceFactory, "CampaignMonitoring")
        {
            var indexKeysDefinition = Builders<CampaignEntity>.IndexKeys.Ascending(x => x.IdCampaign);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<CampaignEntity>(indexKeysDefinition, indexOptions);

            // Chamada síncrona para garantir que o índice seja criado antes de qualquer operação.
            CreateIndexesAsync(new List<CreateIndexModel<CampaignEntity>> { indexModel }).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task<CampaignEntity> CreateCampaignAsync(CampaignEntity entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateCampaignAsync(ObjectId id, CampaignEntity entity)
        {
            var result = await _collection.ReplaceOneAsync(c => c.Id == id, entity);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        /// <inheritdoc />
        public async Task<CampaignEntity> GetCampaignByIdAsync(ObjectId id)
        {
            return await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsAsync()
        {
            return await _collection.Find(FilterDefinition<CampaignEntity>.Empty).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsByClientAsync(string clientName)
        {
            return await _collection.Find(c => c.ClientName == clientName).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsByDateAsync(DateTime start, DateTime finish)
        {
            var filter = Builders<CampaignEntity>.Filter.Gte(c => c.CreatedAt, start) &
                         Builders<CampaignEntity>.Filter.Lte(c => c.CreatedAt, finish);
            return await _collection.Find(filter).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsByClientOrDateAsync(string clientName, DateTime start, DateTime finish)
        {
            var filter = Builders<CampaignEntity>.Filter.And(
                Builders<CampaignEntity>.Filter.Eq(c => c.ClientName, clientName),
                Builders<CampaignEntity>.Filter.Gte(c => c.CreatedAt, start),
                Builders<CampaignEntity>.Filter.Lte(c => c.CreatedAt, finish)
            );
            return await _collection.Find(filter).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetActiveCampaignsAsync()
        {
            return await _collection.Find(c => c.IsActive == true).ToListAsync();
        }

        /// <summary>
        /// Obtém todas as campanhas com um status específico.
        /// </summary>
        /// <param name="statusCampaign">O status da campanha para filtrar.</param>
        /// <returns>Uma coleção de campanhas com o status especificado.</returns>
        public async Task<IEnumerable<CampaignEntity>> GetCampaignsByStatusAsync(CampaignStatus statusCampaign)
        {
            return await _collection.Find(c => c.StatusCampaign == statusCampaign).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<int> CountCampaignsByClientAsync(string clientName)
        {
            return (int)await _collection.CountDocumentsAsync(c => c.ClientName == clientName);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetCampaignsPaginatedAsync(int page, int pageSize)
        {
            return await _collection.Find(FilterDefinition<CampaignEntity>.Empty)
                                      .Skip((page - 1) * pageSize)
                                      .Limit(pageSize)
                                      .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<CampaignEntity> GetCampaignByNameAsync(string campaignName)
        {
            return await _collection.Find(c => c.Name == campaignName).FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<CampaignEntity> GetCampaignByNumberAsync(long campaignNumber)
        {
            return await _collection.Find(c => c.NumberId == campaignNumber).FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<CampaignEntity> GetCampaignByIdCampaignAsync(string idCampaign)
        {
            return await _collection.Find(c => c.IdCampaign == idCampaign).FirstOrDefaultAsync();
        }
    }
}