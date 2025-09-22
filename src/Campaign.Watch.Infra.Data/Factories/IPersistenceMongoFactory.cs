using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaign.Watch.Infra.Data.Factories
{
    public interface IPersistenceMongoFactory
    {
        IMongoDatabase GetDatabase();
    }
}
