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
            var uniqueIndexKeys = Builders<CampaignEntity>.IndexKeys
                .Ascending(x => x.ClientName)
                .Ascending(x => x.IdCampaign);
            var uniqueIndexModel = new CreateIndexModel<CampaignEntity>(
                uniqueIndexKeys,
                new CreateIndexOptions { Unique = true, Name = "Client_IdCampaign_Unique" });

            
            var workerIndexKeys = Builders<CampaignEntity>.IndexKeys
                .Ascending(x => x.IsActive)
                .Ascending(x => x.NextExecutionMonitoring);
            var workerIndexModel = new CreateIndexModel<CampaignEntity>(
                workerIndexKeys,
                new CreateIndexOptions { Name = "Worker_Monitoring_Query" });

            CreateIndexesAsync(new List<CreateIndexModel<CampaignEntity>> { uniqueIndexModel, workerIndexModel }).GetAwaiter().GetResult();
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
            var filter = Builders<CampaignEntity>.Filter.Eq(c => c.IdCampaign, entity.IdCampaign);
            var result = await _collection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = true });
            return result.IsAcknowledged && (result.ModifiedCount > 0 || result.UpsertedId != null);
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
        public async Task<CampaignEntity> GetCampaignByIdCampaignAsync(string clientName, string idCampaign)
        {
            var filter = Builders<CampaignEntity>.Filter.And(
                Builders<CampaignEntity>.Filter.Eq(c => c.ClientName, clientName),
                Builders<CampaignEntity>.Filter.Eq(c => c.IdCampaign, idCampaign)
            );
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetCampaignsDueForMonitoringAsync()
        {
            var now = DateTime.UtcNow;

            var filter = Builders<CampaignEntity>.Filter.And(
                Builders<CampaignEntity>.Filter.Eq(c => c.IsActive, true),
                Builders<CampaignEntity>.Filter.Lte(c => c.NextExecutionMonitoring, now)
            );
            
            return await _collection.Find(filter).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetCampaignsWithIntegrationErrorsAsync()
        {
            return await _collection.Find(c => c.HealthStatus.HasIntegrationErrors == true).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetCampaignsWithDelayedExecutionAsync()
        {
            return await _collection.Find(c => c.MonitoringStatus == MonitoringStatus.ExecutionDelayed).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetSuccessfullyMonitoredCampaignsAsync()
        {
            return await _collection.Find(c => c.MonitoringStatus == MonitoringStatus.Completed || c.MonitoringStatus == MonitoringStatus.WaitingForNextExecution).ToListAsync();
        }
    }
}