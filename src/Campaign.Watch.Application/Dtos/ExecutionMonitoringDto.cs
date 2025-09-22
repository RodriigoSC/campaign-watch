using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Dtos
{
    public class ExecutionMonitoringDto
    {
        public string ExecutionId { get; set; }
        public string CampaignName { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<WorkflowMonitoringDto> WorkflowSteps { get; set; }
    }
}
