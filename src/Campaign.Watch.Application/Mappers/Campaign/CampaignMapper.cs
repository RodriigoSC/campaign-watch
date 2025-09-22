using AutoMapper;
using Campaign.Watch.Application.Dtos.Campaign;
using Campaign.Watch.Domain.Entities.Campaign;
using Campaign.Watch.Domain.Entities.Read.Campaign;
using Campaign.Watch.Domain.Enums;
using System;

namespace Campaign.Watch.Application.Mappers.Campaign
{
    public class CampaignMapper : Profile
    {
        public CampaignMapper()
        {
            // Entidade de Domínio -> DTO de Resposta (Response)
            CreateMap<CampaignEntity, CampaignResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.StatusCampaign, opt => opt.MapFrom(src => src.StatusCampaign))
                .ForMember(dest => dest.TypeCampaign, opt => opt.MapFrom(src => src.TypeCampaign.ToString()));

            CreateMap<Scheduler, SchedulerResponse>().ReverseMap();
            CreateMap<Execution, ExecutionResponse>().ReverseMap();
            CreateMap<Workflows, WorkflowResponse>().ReverseMap();
            CreateMap<FileInfoData, FileInfoDataResponse>().ReverseMap();
            CreateMap<LeadsData, LeadsDataResponse>().ReverseMap();

            CreateMap<IntegrationDataBase, IntegrationDataResponseBase>()
                .Include<EmailIntegrationData, EmailIntegrationDataResponse>()
                .Include<SmsIntegrationData, SmsIntegrationDataResponse>()
                .Include<PushIntegrationData, PushIntegrationDataResponse>()
                .ReverseMap();

            CreateMap<EmailIntegrationData, EmailIntegrationDataResponse>().ReverseMap();
            CreateMap<SmsIntegrationData, SmsIntegrationDataResponse>().ReverseMap();
            CreateMap<PushIntegrationData, PushIntegrationDataResponse>().ReverseMap();

            // DTO de Resposta (Response) -> Entidade de Domínio
            CreateMap<CampaignResponse, CampaignEntity>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Id) ? MongoDB.Bson.ObjectId.Parse(src.Id) : MongoDB.Bson.ObjectId.Empty))
                .ForMember(dest => dest.TypeCampaign, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.TypeCampaign) && Enum.IsDefined(typeof(TypeCampaign), src.TypeCampaign) ? Enum.Parse<TypeCampaign>(src.TypeCampaign, true) : default));

            // Entidade de Leitura (Origem) -> Entidade de Domínio
            CreateMap<CampaignRead, CampaignEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IdCampaign, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.StatusCampaign, opt => opt.MapFrom(src => (CampaignStatus)src.Status))
                .ForMember(dest => dest.TypeCampaign, opt => opt.MapFrom(src => (TypeCampaign)src.Type))
                .ForMember(dest => dest.Scheduler, opt => opt.MapFrom(src => src.Scheduler))
                .ForMember(dest => dest.Executions, opt => opt.Ignore());

            CreateMap<SchedulerReadModel, Scheduler>();

            CreateMap<ExecutionRead, Execution>()
                .ForMember(dest => dest.ExecutionId, opt => opt.MapFrom(src => src.ExecutionId.ToString()))
                .ForMember(dest => dest.Steps, opt => opt.MapFrom(src => src.WorkflowExecution));

            CreateMap<WorkflowExecutionReadModel, Workflows>()
                .ForMember(dest => dest.TotalUser, opt => opt.MapFrom(src => src.TotalUsers));
        }
    }
}