using Campaign.Watch.Domain.Entities.Common;
using Campaign.Watch.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Campaign.Watch.Domain.Entities
{
    public class CampaignEntity : CommonFields
    {
        public string ClientName { get; set; }
        public string IdCampaign { get; set; }
        public long NumberId { get; set; }
        public string Name { get; set; }
        public TypeCampaign TypeCampaign { get; set; }
        public string Description { get; set; }
        public string ProjectId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public CampaignStatus StatusCampaign { get; set; }
        public MonitoringStatus MonitoringStatus { get; set; }
        public DateTime? NextExecutionMonitoring { get; set; }
        public DateTime? LastCheckMonitoring { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsRestored { get; set; }
        public Scheduler Scheduler { get; set; }
        public List<Execution> Executions { get; set; }
    }

    public class Scheduler
    {
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public bool IsRecurrent { get; set; }
        public string Crontab { get; set; }
    }

    public class Execution
    {
        public string ExecutionId { get; set; }
        public string CampaignName { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<Workflows> Steps { get; set; }
    }

    public class Workflows
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public long TotalUser { get; set; }
        public long TotalExecutionTime { get; set; }
        public object Error { get; set; }
        public IntegrationDataBase IntegrationData { get; set; }
    }

    [BsonKnownTypes(
        typeof(EmailIntegrationData),
        typeof(SmsIntegrationData),
        typeof(PushIntegrationData)
    )]
    public abstract class IntegrationDataBase
    {
        public string ChannelName { get; set; }
        public string IntegrationStatus { get; set; }
    }

    public class EmailIntegrationData : IntegrationDataBase
    {
        public string TemplateId { get; set; }
        public FileInfoData File { get; set; }
        public LeadsData Leads { get; set; }
    }

    public class SmsIntegrationData : IntegrationDataBase
    {
        public string MessageTemplate { get; set; }
        public int SentCount { get; set; }
        public int DeliveryErrors { get; set; }
        public int SuccessCount { get; set; }
    }

    public class PushIntegrationData : IntegrationDataBase
    {
        public string MessageTemplate { get; set; }
        public int SentDelivered { get; set; }
    }

    public class FileInfoData
    {
        public string Name { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public long Total { get; set; }
    }

    public class LeadsData
    {
        public int? Blocked { get; set; }
        public int? Deduplication { get; set; }
        public int? Error { get; set; }
        public int? Optout { get; set; }
        public int? Success { get; set; }
    }
}