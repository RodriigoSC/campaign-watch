using AutoMapper;
using Campaign.Watch.Application.Dtos;
using Campaign.Watch.Domain.Entities;

namespace Campaign.Watch.Application.Mappers
{
    public class CampaignMapper : Profile
    {
        public CampaignMapper()
        {
            // Mapeamento da Entidade de Domínio para o DTO
            CreateMap<CampaignEntity, CampaignDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.StatusCampaign, opt => opt.MapFrom(src => src.StatusCampaign.ToString()))
                .ForMember(dest => dest.TypeCampaign, opt => opt.MapFrom(src => src.TypeCampaign.ToString()));

            // Mapeamentos simples
            CreateMap<Scheduler, SchedulerDto>();
            CreateMap<Execution, ExecutionDto>();
            CreateMap<Workflows, WorkflowDto>();
            CreateMap<FileInfoData, FileInfoDataDto>();
            CreateMap<LeadsData, LeadsDataDto>();

            // Mapeamento polimórfico direto entre a entidade base e o DTO base
            CreateMap<IntegrationDataBase, IntegrationDataDtoBase>()
                .Include<EmailIntegrationData, EmailIntegrationDataDto>()
                .Include<SmsIntegrationData, SmsIntegrationDataDto>()
                .Include<PushIntegrationData, PushIntegrationDataDto>();

            // Mapeamentos diretos para cada tipo concreto
            CreateMap<EmailIntegrationData, EmailIntegrationDataDto>();
            CreateMap<SmsIntegrationData, SmsIntegrationDataDto>();
            CreateMap<PushIntegrationData, PushIntegrationDataDto>();
        }
    }
}