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

            CreateMap<WorkflowExecutionReadDto, WorkflowMonitoringDto>()
                .ForMember(dest => dest.StepOrder, opt => opt.Ignore())
                .ForMember(dest => dest.IsWaitingComponent, opt => opt.MapFrom(src => src.Type == "Wait"))
                .ForMember(dest => dest.WaitingUntil, opt => opt.Ignore())
                .ForMember(dest => dest.StatusHistory, opt => opt.Ignore())
                .ForMember(dest => dest.StatusMessage, opt => opt.MapFrom(src => GenerateStatusMessage(src)));

            // Monitoring -> CampaignDto (para persistência)
            CreateMap<CampaignMonitoringDto, CampaignDto>()
                .ForMember(dest => dest.IdCampaign, opt => opt.MapFrom(src => src.IdCampaign))
                .ForMember(dest => dest.StatusCampaign, opt => opt.MapFrom(src => src.StatusCampaign))
                .ForMember(dest => dest.TypeCampaign, opt => opt.MapFrom(src => src.TypeCampaign.ToString()))
                .ForMember(dest => dest.Executions, opt => opt.MapFrom(src => MapExecutionsToDto(src.Executions)));

            CreateMap<ExecutionMonitoringDto, ExecutionDto>()
                .ForMember(dest => dest.Steps, opt => opt.MapFrom(src => MapWorkflowsToDto(src.WorkflowSteps)));

            CreateMap<WorkflowMonitoringDto, WorkflowDto>()
                .ForMember(dest => dest.TotalUser, opt => opt.MapFrom(src => src.TotalUsers))
                .ForMember(dest => dest.IntegrationData, opt => opt.MapFrom(src => src.IntegrationData));

            // Entity -> DTO
            CreateMap<CampaignEntity, CampaignDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        }

        private string GenerateStatusMessage(WorkflowExecutionReadDto workflow)
        {
            return workflow.Type switch
            {
                "Filter" => $"Filtro processado: {workflow.TotalUsers} usuários",
                "Channel" => $"Canal integrado: {workflow.Status}",
                "Wait" => $"Aguardando: {workflow.Status}",
                "DecisionSplit" => $"Decisão processada: {workflow.TotalUsers} usuários",
                "RandomSplit" => $"Divisão aleatória: {workflow.TotalUsers} usuários",
                "End" => "Fluxo finalizado",
                _ => $"Step {workflow.Type}: {workflow.Status}"
            };
        }

        private object MapExecutionsToDto(object executions)
        {
            // Implementar lógica de mapeamento se necessário
            return executions;
        }

        private object MapWorkflowsToDto(object workflows)
        {
            // Implementar lógica de mapeamento se necessário
            return workflows;
        }
    }
}
