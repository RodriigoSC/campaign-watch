﻿using Campaign.Watch.Infra.Data.Factories.Common;
using MongoDB.Driver;

namespace Campaign.Watch.Infra.Effwhatsapp.Factories
{
    public class EffwhatsappMongoFactory : IEffwhatsappMongoFactory
    {
        private readonly IMongoDbFactory _factory;

        public EffwhatsappMongoFactory(IMongoDbFactory factory)
        {
            _factory = factory;
        }

        public IMongoDatabase GetDatabase(string dbName)
        {
            return _factory.GetDatabase("MongoDB.Effwhatsapp", dbName);
        }
    }
}
