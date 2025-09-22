using Campaign.Watch.Domain.Entities;
using Campaign.Watch.Domain.Enums;
using Campaign.Watch.Domain.Interfaces.Repositories;
using Campaign.Watch.Domain.Interfaces.Services;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Infra.Data.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IClientRepository _clientRepository;


        public CampaignService(ICampaignRepository campaignRepository, IClientRepository clientRepository)
        {
            _campaignRepository = campaignRepository;
            _clientRepository = clientRepository;
        }

        public async Task<CampaignEntity> CreateCampaignAsync(CampaignEntity entity)
        {
            var client = await _clientRepository.GetByNameAsync(entity.ClientName);
            if (client == null)
            {
                throw new ArgumentException($"O cliente especificado ('{entity.ClientName}') não existe.");
            }
            if (!client.IsActive)
            {
                 throw new InvalidOperationException($"Não é possível criar campanhas para um cliente inativo.");
            }

            return await _campaignRepository.CreateCampaignAsync(entity);
        }

        public async Task<bool> UpdateCampaignAsync(ObjectId id, CampaignEntity entity)
        {
            var client = await _clientRepository.GetByNameAsync(entity.ClientName);
            if (client == null)
            {
                throw new ArgumentException($"O cliente especificado ('{entity.ClientName}') não existe.");
            }

            return await _campaignRepository.UpdateCampaignAsync(id, entity);
        }

        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsAsync()
        {
            return await _campaignRepository.GetAllCampaignsAsync();
        }

        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsByClientAsync(string clientName)
        {
            return await _campaignRepository.GetAllCampaignsByClientAsync(clientName);
        }

        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsByDateAsync(DateTime start, DateTime finish)
        {
            return await _campaignRepository.GetAllCampaignsByDateAsync(start, finish);
        }

        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsByClientOrDateAsync(string clientName, DateTime start, DateTime finish)
        {
            return await _campaignRepository.GetAllCampaignsByClientOrDateAsync(clientName, start, finish);
        }

        public async Task<CampaignEntity> GetCampaignByIdAsync(ObjectId id)
        {
            return await _campaignRepository.GetCampaignByIdAsync(id);
        }

        public async Task<CampaignEntity> GetCampaignByNameAsync(string campaignName)
        {
            return await _campaignRepository.GetCampaignByNameAsync(campaignName);
        }

        public async Task<CampaignEntity> GetCampaignByNumberAsync(long campaignNumber)
        {
            return await _campaignRepository.GetCampaignByNumberAsync(campaignNumber);
        }

        public async Task<IEnumerable<CampaignEntity>> GetActiveCampaignsAsync()
        {
            return await _campaignRepository.GetActiveCampaignsAsync();
        }

        public async Task<IEnumerable<CampaignEntity>> GetCampaignsByStatusAsync(CampaignStatus statusCampaign)
        {
            return await _campaignRepository.GetCampaignsByStatusAsync(statusCampaign);
        }

        public async Task<IEnumerable<CampaignEntity>> GetCampaignsPaginatedAsync(int page, int pageSize)
        {
            return await _campaignRepository.GetCampaignsPaginatedAsync(page, pageSize);
        }

        public async Task<int> CountCampaignsByClientAsync(string clientName)
        {
            return await _campaignRepository.CountCampaignsByClientAsync(clientName);
        }

        public async Task<CampaignEntity> GetCampaignByIdCampaignAsync(string idCampaign)
        {
            return await _campaignRepository.GetCampaignByIdCampaignAsync(idCampaign);
        }
    }
}
