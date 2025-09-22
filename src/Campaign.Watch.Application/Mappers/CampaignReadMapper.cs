using AutoMapper;
using Campaign.Watch.Application.Dtos.Read.Campaign;
using Campaign.Watch.Domain.Entities.Read;


namespace Campaign.Watch.Application.Mappers
{
    public class CampaignReadMapper : Profile
    {
        public CampaignReadMapper()
        {
            // CampaignReadModel -> CampaignReadDto
            CreateMap<CampaignReadModel, CampaignReadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.ModifiedAt, opt => opt.MapFrom(src => src.ModifiedAt));

            // SchedulerReadModel -> SchedulerReadDto
            CreateMap<SchedulerReadModel, SchedulerReadDto>();

            // JourneyReadModel -> JourneyReadDto
            CreateMap<JourneyReadModel, JourneyReadDto>();

            // WorkflowReadModel -> WorkflowReadDto
            CreateMap<WorkflowReadModel, WorkflowReadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        }
    }
}
