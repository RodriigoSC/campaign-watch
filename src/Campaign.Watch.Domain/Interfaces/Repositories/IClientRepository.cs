using Campaign.Watch.Domain.Entities;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Domain.Interfaces.Repositories
{
    public interface IClientRepository
    {
        Task<ClientEntity> CreateAsync(ClientEntity client);
        Task<IEnumerable<ClientEntity>> GetAllAsync();
        Task<ClientEntity> GetByIdAsync(ObjectId id);
        Task<ClientEntity> GetByNameAsync(string clientName);
        Task<bool> UpdateAsync(ObjectId id, ClientEntity client);
        Task<bool> DeleteAsync(ObjectId id);
    }
}
