using Campaign.Watch.Application.Dtos.Campaign;
using System;
using System.Collections.Generic;


namespace Campaign.Watch.Application.Dtos.Read.Campaign
{
    public class WorkflowMonitoringDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public long TotalUsers { get; set; }
        public long TotalExecutionTime { get; set; }
        public object Error { get; set; }
        public string StatusMessage { get; set; }
        public IntegrationDataDtoBase IntegrationData { get; set; }

        // Dados específicos para controle de steps
        public int StepOrder { get; set; }
        public bool IsWaitingComponent { get; set; }
        public DateTime? WaitingUntil { get; set; }
        public List<string> StatusHistory { get; set; } = new List<string>();
    }
}
