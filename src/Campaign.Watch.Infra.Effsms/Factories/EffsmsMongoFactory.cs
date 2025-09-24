﻿using Campaign.Watch.Infra.Data.Factories.Common;
using MongoDB.Driver;

namespace Campaign.Watch.Infra.Effsms.Factories
{
    public class EffsmsMongoFactory : IEffsmsMongoFactory
    {
        private readonly IMongoDbFactory _factory;

        public EffsmsMongoFactory(IMongoDbFactory factory)
        {
            _factory = factory;
        }

        public IMongoDatabase GetDatabase(string dbName)
        {
            return _factory.GetDatabase("MongoDB.Effsms", dbName);
        }
    }
}
