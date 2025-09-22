using AutoMapper;
using Campaign.Watch.Application.Dtos.Read.Campaign;
using Campaign.Watch.Domain.Entities.Read.Campaign;

namespace Campaign.Watch.Application.Mappers.Read.Campaign
{
    public class ExecutionReadMapper : Profile
    {
        public ExecutionReadMapper()
        {
            CreateMap<ExecutionRead, ExecutionReadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId.ToString()))
                .ForMember(dest => dest.CampaignId, opt => opt.MapFrom(src => src.CampaignId.ToString()))
                .ForMember(dest => dest.ExecutionId, opt => opt.MapFrom(src => src.ExecutionId.ToString()))
                .ForMember(dest => dest.LastExecutionId, opt => opt.MapFrom(src => src.LastExecutionId.ToString()));

            CreateMap<WorkflowExecutionReadModel, WorkflowExecutionReadDto>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        }
    }
}
