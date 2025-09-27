using AutoMapper;
using Campaign.Watch.Application.Dtos.Campaign;
using Campaign.Watch.Domain.Entities.Campaign;
using Campaign.Watch.Domain.Entities.Read.Campaign;
using Campaign.Watch.Domain.Entities.Read.Effmail;
using Campaign.Watch.Domain.Entities.Read.Effpush;
using Campaign.Watch.Domain.Entities.Read.Effsms;
using Campaign.Watch.Domain.Entities.Read.Effwhatsapp;
using Campaign.Watch.Domain.Enums;
using Campaign.Watch.Domain.Extensions;
using MongoDB.Bson;

namespace Campaign.Watch.Application.Mappers.Campaign
{
    public class CampaignMapper : Profile
    {
        public CampaignMapper()
        {       
            CreateMap<CampaignEntity, CampaignDetailResponse>()
             .ForMember(dest => dest.CampaignType, opt => opt.MapFrom(src => src.CampaignType.GetDescription()))
             .ForMember(dest => dest.MonitoringStatus, opt => opt.MapFrom(src => src.MonitoringStatus.GetDescription()))
             .ForMember(dest => dest.StatusCampaign, opt => opt.MapFrom(src => src.StatusCampaign.GetDescription()));


            CreateMap<CampaignEntity, CampaignStatusResponse>()
             .ForMember(dest => dest.CampaignType, opt => opt.MapFrom(src => src.CampaignType.GetDescription()))
             .ForMember(dest => dest.StatusCampaign, opt => opt.MapFrom(src => src.StatusCampaign.GetDescription()));




            CreateMap<MonitoringHealthStatus, MonitoringHealthStatusDto>();
            CreateMap<Scheduler, SchedulerResponse>();
            CreateMap<Execution, ExecutionResponse>();
            CreateMap<Workflows, WorkflowResponse>();
            CreateMap<FileInfoData, FileInfoDataResponse>();
            CreateMap<LeadsData, LeadsDataResponse>();

            CreateMap<IntegrationDataBase, IntegrationDataResponseBase>()
                .Include<EmailIntegrationData, EmailIntegrationDataResponse>()
                .Include<SmsIntegrationData, SmsIntegrationDataResponse>()
                .Include<PushIntegrationData, PushIntegrationDataResponse>()
                .Include<WhatsAppIntegrationData, WhatsAppIntegrationDataResponse>();

            CreateMap<EmailIntegrationData, EmailIntegrationDataResponse>();
            CreateMap<SmsIntegrationData, SmsIntegrationDataResponse>();
            CreateMap<PushIntegrationData, PushIntegrationDataResponse>();
            CreateMap<WhatsAppIntegrationData, WhatsAppIntegrationDataResponse>();

            // === MAPEAMENTOS DE DTO PARA DOMÍNIO (VOLTA) ===
            CreateMap<CampaignDetailResponse, CampaignEntity>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Id) ? ObjectId.Parse(src.Id) : ObjectId.Empty));
            CreateMap<MonitoringHealthStatusDto, MonitoringHealthStatus>();
            CreateMap<SchedulerResponse, Scheduler>();
            CreateMap<ExecutionResponse, Execution>();
            CreateMap<WorkflowResponse, Workflows>();
            CreateMap<FileInfoDataResponse, FileInfoData>();
            CreateMap<LeadsDataResponse, LeadsData>();

            CreateMap<IntegrationDataResponseBase, IntegrationDataBase>()
                .Include<EmailIntegrationDataResponse, EmailIntegrationData>()
                .Include<SmsIntegrationDataResponse, SmsIntegrationData>()
                .Include<PushIntegrationDataResponse, PushIntegrationData>()
                .Include<WhatsAppIntegrationDataResponse, WhatsAppIntegrationData>();

            CreateMap<EmailIntegrationDataResponse, EmailIntegrationData>();
            CreateMap<SmsIntegrationDataResponse, SmsIntegrationData>();
            CreateMap<PushIntegrationDataResponse, PushIntegrationData>();
            CreateMap<WhatsAppIntegrationDataResponse, WhatsAppIntegrationData>();

            // === MAPEAMENTOS DE DADOS DE LEITURA (ORIGEM) PARA DOMÍNIO ===
            CreateMap<CampaignRead, CampaignEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IdCampaign, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.StatusCampaign, opt => opt.MapFrom(src => (CampaignStatus)src.Status))
                .ForMember(dest => dest.CampaignType, opt => opt.MapFrom(src => (CampaignType)src.Type))
                .ForMember(dest => dest.Executions, opt => opt.Ignore());

            // ##### ESTA É A LINHA QUE CORRIGE O ERRO ATUAL #####
            CreateMap<SchedulerReadModel, Scheduler>();

            CreateMap<ExecutionRead, Execution>()
                .ForMember(dest => dest.ExecutionId, opt => opt.MapFrom(src => src.ExecutionId.ToString()))
                .ForMember(dest => dest.Steps, opt => opt.MapFrom(src => src.WorkflowExecution));

            CreateMap<WorkflowExecutionReadModel, Workflows>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.TotalUser, opt => opt.MapFrom(src => src.TotalUsers))
                .ForMember(dest => dest.ChannelName, opt => {
                    opt.MapFrom(src =>
                        (src.ExecutionData != null && src.ExecutionData.Contains("ChannelName"))
                            ? src.ExecutionData["ChannelName"].AsString
                            : null);
                });

            // Mapeamentos de sub-tipos de Leitura para Domínio
            CreateMap<Domain.Entities.Read.Effmail.FileInfo, FileInfoData>();
            CreateMap<Domain.Entities.Read.Effsms.FileInfo, FileInfoData>();
            CreateMap<Domain.Entities.Read.Effpush.FileInfo, FileInfoData>();
            CreateMap<Domain.Entities.Read.Effwhatsapp.ArchiveInfo, FileInfoData>()
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total));

            CreateMap<Domain.Entities.Read.Effmail.Leads, LeadsData>();
            CreateMap<Domain.Entities.Read.Effsms.Leads, LeadsData>();
            CreateMap<Domain.Entities.Read.Effpush.Leads, LeadsData>();
            CreateMap<Domain.Entities.Read.Effwhatsapp.Leads, LeadsData>();

            // Mapeamentos de Leitura de Canais para Entidades de Integração
            CreateMap<EffmailRead, EmailIntegrationData>();
            CreateMap<EffsmsRead, SmsIntegrationData>();
            CreateMap<EffpushRead, PushIntegrationData>();
            CreateMap<EffwhatsappRead, WhatsAppIntegrationData>()
                .ForMember(dest => dest.File, opt => opt.MapFrom(src => src.Archive));
        }
    }
}