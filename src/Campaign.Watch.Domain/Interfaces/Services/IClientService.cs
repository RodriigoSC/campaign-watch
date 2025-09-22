using Campaign.Watch.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Domain.Interfaces.Services
{
    public interface IClientService
    {
        Task<ClientEntity> CreateClientAsync(ClientEntity client);
        Task<IEnumerable<ClientEntity>> GetAllClientsAsync();
        Task<ClientEntity> GetClientByIdAsync(string id);
        Task<ClientEntity> GetClientByNameAsync(string clientName);
        Task<bool> UpdateClientAsync(string id, ClientEntity client);
        Task<bool> DeleteClientAsync(string id);
    }
}
