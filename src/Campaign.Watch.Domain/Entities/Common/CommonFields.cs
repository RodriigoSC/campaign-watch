using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Campaign.Watch.Domain.Entities.Common
{
    public class CommonFields
    {
        [BsonId]
        public ObjectId Id { get; set; }       
    }
}
