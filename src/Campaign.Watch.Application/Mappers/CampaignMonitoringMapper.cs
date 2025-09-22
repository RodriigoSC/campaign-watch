using AutoMapper;
using Campaign.Watch.Application.Dtos.Campaign;
using Campaign.Watch.Application.Dtos.Read.Campaign;
using Campaign.Watch.Domain.Entities.Campaign;
using Campaign.Watch.Domain.Enums;

namespace Campaign.Watch.Application.Mappers
{
    public class CampaignMonitoringMapper : Profile
    {
        public CampaignMonitoringMapper()
        {
            // Read -> Monitoring
            CreateMap<CampaignReadDto, CampaignMonitoringDto>()
                .ForMember(dest => dest.IdCampaign, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.StatusCampaign, opt => opt.MapFrom(src => (CampaignStatus)src.Status))
                .ForMember(dest => dest.TypeCampaign, opt => opt.MapFrom(src => (TypeCampaign)src.Type))
                .ForMember(dest => dest.MonitoringStatus, opt => opt.Ignore())
                .ForMember(dest => dest.NextExecutionMonitoring, opt => opt.Ignore())
                .ForMember(dest => dest.LastCheckMonitoring, opt => opt.Ignore())
                .ForMember(dest => dest.Executions, opt => opt.Ignore());

            CreateMap<SchedulerReadDto, SchedulerDto>();

            CreateMap<ExecutionReadDto, ExecutionMonitoringDto>()
                .ForMember(dest => dest.WorkflowSteps,
                    opt => opt.MapFrom(src => src.WorkflowExecution));

            CreateMap<WorkflowExecutionReadDto, WorkflowMonitoringDto>();

            // Monitoring -> Entity
            CreateMap<CampaignMonitoringDto, CampaignEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
}
