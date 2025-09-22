using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Campaign.Watch.Domain.Entities.Read
{
    [BsonIgnoreExtraElements]
    public class CampaignReadModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string Id { get; set; }

        [BsonElement("NumberId")]
        public long NumberId { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Type")]
        public int Type { get; set; }

        [BsonElement("Description")]
        public string Description { get; set; }

        [BsonElement("ProjectId")]
        public string ProjectId { get; set; }

        [BsonElement("IsActive")]
        public bool IsActive { get; set; }

        [BsonElement("CreatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }

        [BsonElement("ModifiedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ModifiedAt { get; set; }

        [BsonElement("Status")]
        public int Status { get; set; }

        [BsonElement("IsDeleted")]
        public bool IsDeleted { get; set; }

        [BsonElement("IsRestored")]
        public bool IsRestored { get; set; }

        [BsonElement("Scheduler")]
        public SchedulerReadModel Scheduler { get; set; }

        [BsonElement("Journey")]
        public JourneyReadModel Journey { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class SchedulerReadModel
    {
        public string SchedulerAPIId { get; set; }
        public string Crontab { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public bool IsRecurrent { get; set; }
        public bool IsPaused { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class JourneyReadModel
    {

        [BsonElement("Workflow")]
        public List<WorkflowReadModel> Workflow { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class WorkflowReadModel
    {
        [BsonElement("_id")]
        public string Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("ComponentType")]
        public int ComponentType { get; set; }
    }
}
