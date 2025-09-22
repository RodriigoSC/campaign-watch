using AutoMapper;
using Campaign.Watch.Application.Dtos.Client;
using Campaign.Watch.Application.Interfaces.Client;
using Campaign.Watch.Domain.Entities.Client;
using Campaign.Watch.Domain.Interfaces.Services.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Services.Client
{
    public class ClientApplication : IClientApplication
    {
        private readonly IClientService _clientService;
        private readonly IMapper _mapper;

        public ClientApplication(IClientService clientService, IMapper mapper)
        {
            _clientService = clientService;
            _mapper = mapper;
        }

        public async Task<ClientDto> CreateClientAsync(ClientInputDto clientInputDto)
        {
            var clientEntity = _mapper.Map<ClientEntity>(clientInputDto);
            var createdEntity = await _clientService.CreateClientAsync(clientEntity);
            return _mapper.Map<ClientDto>(createdEntity);
        }

        public async Task<IEnumerable<ClientDto>> GetAllClientsAsync()
        {
            var clientEntities = await _clientService.GetAllClientsAsync();
            return _mapper.Map<IEnumerable<ClientDto>>(clientEntities);
        }

        public async Task<ClientDto> GetClientByIdAsync(string id)
        {
            var clientEntity = await _clientService.GetClientByIdAsync(id);
            return _mapper.Map<ClientDto>(clientEntity);
        }

        public async Task<ClientDto> GetClientByNameAsync(string clientName)
        {
            var clientEntity = await _clientService.GetClientByNameAsync(clientName);
            return _mapper.Map<ClientDto>(clientEntity);
        }

        public async Task<bool> UpdateClientAsync(string id, ClientInputDto clientInputDto)
        {
            var clientEntity = _mapper.Map<ClientEntity>(clientInputDto);
            return await _clientService.UpdateClientAsync(id, clientEntity);
        }

        public async Task<bool> DeleteClientAsync(string id)
        {
            return await _clientService.DeleteClientAsync(id);
        }
    }
}
