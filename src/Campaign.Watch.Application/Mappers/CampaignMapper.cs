using AutoMapper;
using Campaign.Watch.Application.Dtos;
using Campaign.Watch.Domain.Entities;
using Campaign.Watch.Domain.Enums;
using System;

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

            CreateMap<Scheduler, SchedulerDto>().ReverseMap();
            CreateMap<Execution, ExecutionDto>().ReverseMap();
            CreateMap<Workflows, WorkflowDto>().ReverseMap();
            CreateMap<FileInfoData, FileInfoDataDto>().ReverseMap();
            CreateMap<LeadsData, LeadsDataDto>().ReverseMap();

            CreateMap<IntegrationDataBase, IntegrationDataDtoBase>()
                .Include<EmailIntegrationData, EmailIntegrationDataDto>()
                .Include<SmsIntegrationData, SmsIntegrationDataDto>()
                .Include<PushIntegrationData, PushIntegrationDataDto>()
                .ReverseMap();

            CreateMap<EmailIntegrationData, EmailIntegrationDataDto>().ReverseMap();
            CreateMap<SmsIntegrationData, SmsIntegrationDataDto>().ReverseMap();
            CreateMap<PushIntegrationData, PushIntegrationDataDto>().ReverseMap();

            // Mapeamento do DTO para a Entidade de Domínio            
            CreateMap<CampaignDto, CampaignEntity>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Id) ? MongoDB.Bson.ObjectId.Parse(src.Id) : MongoDB.Bson.ObjectId.Empty)
            )
            .ForMember(dest => dest.TypeCampaign, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.TypeCampaign) && Enum.IsDefined(typeof(TypeCampaign), src.TypeCampaign)
                    ? Enum.Parse<TypeCampaign>(src.TypeCampaign, true)
                    : default(TypeCampaign)));
        }
    }
}