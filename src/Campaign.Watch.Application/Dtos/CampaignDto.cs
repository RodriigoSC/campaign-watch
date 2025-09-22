using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Campaign.Watch.Application.Dtos
{
    public class CampaignDto
    {
        public string Id { get; set; }
        public string ClientName { get; set; }
        public string IdCampaign { get; set; }
        public int NumberId { get; set; }
        public string Name { get; set; }
        public string TypeCampaign { get; set; }
        public string Description { get; set; }
        public string ProjectId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string StatusCampaign { get; set; }
        public SchedulerDto Scheduler { get; set; }
        public List<ExecutionDto> Executions { get; set; }
    }

    public class SchedulerDto
    {
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public bool IsRecurrent { get; set; }
        public string Crontab { get; set; }
    }

    public class ExecutionDto
    {
        public string ExecutionId { get; set; }
        public string CampaignName { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<WorkflowDto> Steps { get; set; }
    }

    public class WorkflowDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public long TotalUser { get; set; }
        public long TotalExecutionTime { get; set; }
        public object Error { get; set; }
        public IntegrationDataDtoBase IntegrationData { get; set; }
    }
    public abstract class IntegrationDataDtoBase
    {
        public string ChannelName { get; set; }
        public string IntegrationStatus { get; set; }
    }

    public class EmailIntegrationDataDto : IntegrationDataDtoBase
    {
        public string TemplateId { get; set; }
        public FileInfoDataDto File { get; set; }
        public LeadsDataDto Leads { get; set; }
    }

    public class SmsIntegrationDataDto : IntegrationDataDtoBase
    {
        public string MessageTemplate { get; set; }
        public int SentCount { get; set; }
        public int DeliveryErrors { get; set; }
        public int SuccessCount { get; set; }
    }

    public class PushIntegrationDataDto : IntegrationDataDtoBase
    {
        public string MessageTemplate { get; set; }
        public int SentDelivered { get; set; }
    }

    public class FileInfoDataDto
    {
        public string Name { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public long Total { get; set; }
    }

    public class LeadsDataDto
    {
        public int? Blocked { get; set; }
        public int? Deduplication { get; set; }
        public int? Error { get; set; }
        public int? Optout { get; set; }
        public int? Success { get; set; }
    }
}