using Campaign.Watch.Domain.Entities;
using Campaign.Watch.Domain.Interfaces.Repositories;
using Campaign.Watch.Domain.Interfaces.Services;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Campaign.Watch.Infra.Data.Services
{
    public class ClientService : IClientService
    {

        private readonly IClientRepository _clientRepository;

        public ClientService(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<ClientEntity> CreateClientAsync(ClientEntity client)
        {
            var existingClient = await _clientRepository.GetByNameAsync(client.Name);
            if (existingClient != null)
            {
                throw new InvalidOperationException("Já existe um cliente com este nome.");
            }
            return await _clientRepository.CreateAsync(client);
        }

        public async Task<IEnumerable<ClientEntity>> GetAllClientsAsync()
        {
            return await _clientRepository.GetAllAsync();
        }

        public async Task<ClientEntity> GetClientByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return null;

            return await _clientRepository.GetByIdAsync(objectId);
        }

        public async Task<ClientEntity> GetClientByNameAsync(string clientName)
        {
            return await _clientRepository.GetByNameAsync(clientName);
        }

        public async Task<bool> UpdateClientAsync(string id, ClientEntity client)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return false;

            return await _clientRepository.UpdateAsync(objectId, client);
        }

        public async Task<bool> DeleteClientAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return false;

            return await _clientRepository.DeleteAsync(objectId);
        }
    }
}
