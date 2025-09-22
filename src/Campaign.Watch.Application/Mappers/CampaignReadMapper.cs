using AutoMapper;
using Campaign.Watch.Application.Dtos;
using Campaign.Watch.Application.Dtos.Read.Campaign;
using Campaign.Watch.Domain.Entities.Read;
using Campaign.Watch.Domain.Enums;


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

            // CampaignReadDto -> CampaignDto
            CreateMap<CampaignReadDto, CampaignDto>()
                .ForMember(dest => dest.IdCampaign, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.StatusCampaign, opt => opt.MapFrom(src => ((CampaignStatus)src.Status).ToString()))
                .ForMember(dest => dest.TypeCampaign, opt => opt.MapFrom(src => ((TypeCampaign)src.Type).ToString()));

            // Mapeamento que estava faltando para o objeto aninhado
            CreateMap<SchedulerReadDto, SchedulerDto>();
        }
    }
}
