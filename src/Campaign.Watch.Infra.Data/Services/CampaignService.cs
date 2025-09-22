using Campaign.Watch.Domain.Entities.Campaign;
using Campaign.Watch.Domain.Enums;
using Campaign.Watch.Domain.Interfaces.Repositories.Campaign;
using Campaign.Watch.Domain.Interfaces.Repositories.Client;
using Campaign.Watch.Domain.Interfaces.Services.Campaign;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Infra.Data.Services
{
    /// <summary>
    /// Implementação do serviço de campanhas, contendo as regras de negócio para operações de campanha.
    /// </summary>
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IClientRepository _clientRepository;

        /// <summary>
        /// Inicializa uma nova instância da classe CampaignService.
        /// </summary>
        /// <param name="campaignRepository">O repositório de campanhas para acesso aos dados.</param>
        /// <param name="clientRepository">O repositório de clientes para validações.</param>
        public CampaignService(ICampaignRepository campaignRepository, IClientRepository clientRepository)
        {
            _campaignRepository = campaignRepository;
            _clientRepository = clientRepository;
        }

        /// <summary>
        /// Cria uma nova campanha após validar se o cliente associado existe e está ativo.
        /// </summary>
        /// <param name="entity">A entidade da campanha a ser criada.</param>
        /// <returns>A entidade da campanha após a criação.</returns>
        /// <exception cref="ArgumentException">Lançada se o cliente especificado na entidade não for encontrado.</exception>
        /// <exception cref="InvalidOperationException">Lançada se for tentado criar uma campanha para um cliente inativo.</exception>
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

        /// <summary>
        /// Atualiza uma campanha existente após validar se o cliente associado existe.
        /// </summary>
        /// <param name="id">O ObjectId da campanha a ser atualizada.</param>
        /// <param name="entity">A entidade com os dados atualizados.</param>
        /// <returns>Retorna true se a atualização foi bem-sucedida, caso contrário, false.</returns>
        /// <exception cref="ArgumentException">Lançada se o cliente especificado na entidade não for encontrado.</exception>
        public async Task<bool> UpdateCampaignAsync(ObjectId id, CampaignEntity entity)
        {
            var client = await _clientRepository.GetByNameAsync(entity.ClientName);
            if (client == null)
            {
                throw new ArgumentException($"O cliente especificado ('{entity.ClientName}') não existe.");
            }

            return await _campaignRepository.UpdateCampaignAsync(id, entity);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsAsync()
        {
            return await _campaignRepository.GetAllCampaignsAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsByClientAsync(string clientName)
        {
            return await _campaignRepository.GetAllCampaignsByClientAsync(clientName);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsByDateAsync(DateTime start, DateTime finish)
        {
            return await _campaignRepository.GetAllCampaignsByDateAsync(start, finish);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetAllCampaignsByClientOrDateAsync(string clientName, DateTime start, DateTime finish)
        {
            return await _campaignRepository.GetAllCampaignsByClientOrDateAsync(clientName, start, finish);
        }

        /// <inheritdoc />
        public async Task<CampaignEntity> GetCampaignByIdAsync(ObjectId id)
        {
            return await _campaignRepository.GetCampaignByIdAsync(id);
        }

        /// <inheritdoc />
        public async Task<CampaignEntity> GetCampaignByNameAsync(string campaignName)
        {
            return await _campaignRepository.GetCampaignByNameAsync(campaignName);
        }

        /// <inheritdoc />
        public async Task<CampaignEntity> GetCampaignByNumberAsync(long campaignNumber)
        {
            return await _campaignRepository.GetCampaignByNumberAsync(campaignNumber);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetActiveCampaignsAsync()
        {
            return await _campaignRepository.GetActiveCampaignsAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetCampaignsByStatusAsync(CampaignStatus statusCampaign)
        {
            return await _campaignRepository.GetCampaignsByStatusAsync(statusCampaign);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CampaignEntity>> GetCampaignsPaginatedAsync(int page, int pageSize)
        {
            return await _campaignRepository.GetCampaignsPaginatedAsync(page, pageSize);
        }

        /// <inheritdoc />
        public async Task<int> CountCampaignsByClientAsync(string clientName)
        {
            return await _campaignRepository.CountCampaignsByClientAsync(clientName);
        }

        /// <inheritdoc />
        public async Task<CampaignEntity> GetCampaignByIdCampaignAsync(string idCampaign)
        {
            return await _campaignRepository.GetCampaignByIdCampaignAsync(idCampaign);
        }
    }
}