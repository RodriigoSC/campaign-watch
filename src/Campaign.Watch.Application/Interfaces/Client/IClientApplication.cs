using Campaign.Watch.Application.Dtos.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Interfaces.Client
{
    public interface IClientApplication
    {
        Task<ClientDto> CreateClientAsync(ClientInputDto clientDto);
        Task<IEnumerable<ClientDto>> GetAllClientsAsync();
        Task<ClientDto> GetClientByIdAsync(string id);
        Task<ClientDto> GetClientByNameAsync(string clientName);
        Task<bool> UpdateClientAsync(string id, ClientInputDto clientDto);
        Task<bool> DeleteClientAsync(string id);
    }
}
