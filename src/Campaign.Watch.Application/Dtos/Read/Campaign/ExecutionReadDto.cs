using System;
using System.Collections.Generic;

namespace Campaign.Watch.Application.Dtos.Read.Campaign
{
    public class ExecutionReadDto
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string CampaignId { get; set; }
        public string CampaignName { get; set; }
        public string ExecutionId { get; set; }
        public string LastExecutionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public bool IsCanceled { get; set; }
        public string QueryExternal { get; set; }
        public List<WorkflowExecutionReadDto> WorkflowExecution { get; set; }
    }

    public class WorkflowExecutionReadDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public int TotalUsers { get; set; }
        public int TotalExecutionTime { get; set; }
        public object Error { get; set; }
    }
}
