using Campaign.Watch.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Dtos
{
    public class CampaignMonitoringDto
    {
        // Dados da campanha original
        public string IdCampaign { get; set; }
        public string ClientName { get; set; }
        public long NumberId { get; set; }
        public string Name { get; set; }
        public TypeCampaign TypeCampaign { get; set; }
        public string Description { get; set; }
        public string ProjectId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public CampaignStatus StatusCampaign { get; set; }

        // Dados de monitoramento
        public MonitoringStatus MonitoringStatus { get; set; }
        public DateTime? NextExecutionMonitoring { get; set; }
        public DateTime? LastCheckMonitoring { get; set; }

        // Dados relacionados
        public SchedulerDto Scheduler { get; set; }
        public List<ExecutionMonitoringDto> Executions { get; set; }
    }
}
