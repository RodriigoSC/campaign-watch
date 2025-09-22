using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Dtos.Read.Campaign
{
    public class CampaignReadDto
    {
        public string Id { get; set; }
        public long NumberId { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string Description { get; set; }
        public string ProjectId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int Status { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsRestored { get; set; }
        public SchedulerReadDto Scheduler { get; set; }
        public JourneyReadDto Journey { get; set; }
    }

    public class SchedulerReadDto
    {
        public string SchedulerAPIId { get; set; }
        public string Crontab { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public bool IsRecurrent { get; set; }
        public bool IsPaused { get; set; }
    }

    public class JourneyReadDto
    {
        public List<WorkflowReadDto> Workflow { get; set; }
    }

    public class WorkflowReadDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int ComponentType { get; set; }
    }
}
