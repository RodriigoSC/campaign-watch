using AutoMapper;
using Campaign.Watch.Application.Dtos.Client;
using Campaign.Watch.Domain.Entities.Client;
using Campaign.Watch.Domain.Enums;
using System;

namespace Campaign.Watch.Application.Mappers.Client
{
    public class ClientMapper : Profile
    {
        public ClientMapper()
        {
            // Mapeamento da Entrada (DTO) para a Entidade de Domínio
            CreateMap<ClientInputDto, ClientEntity>();
            CreateMap<CampaignConfigInputDto, CampaignConfig>();
            CreateMap<EffectiveChannelInputDto, EffectiveChannel>()
                .ConvertUsing((src, dest, context) =>
                {
                    // Com base no enum do DTO, decidimos qual objeto concreto criar
                    switch (src.TypeChannel)
                    {
                        case TypeChannels.EffectiveMail:
                            return context.Mapper.Map<EffectiveMail>(src);
                        case TypeChannels.EffectiveSms:
                            return context.Mapper.Map<EffectiveSms>(src);
                        case TypeChannels.EffectivePush:
                            return context.Mapper.Map<EffectivePush>(src);
                        case TypeChannels.EffectivePages:
                            return context.Mapper.Map<EffectivePages>(src);
                        case TypeChannels.EffectiveSocial:
                            return context.Mapper.Map<EffectiveSocial>(src);
                        case TypeChannels.EffectiveWhatsApp:
                            return context.Mapper.Map<EffectiveWhastApp>(src);
                        default:
                            throw new NotImplementedException($"Mapeamento não implementado para TypeChannel: {src.TypeChannel}");
                    }
                });

            CreateMap<EffectiveChannelInputDto, EffectiveMail>();
            CreateMap<EffectiveChannelInputDto, EffectiveSms>();
            CreateMap<EffectiveChannelInputDto, EffectivePush>();
            CreateMap<EffectiveChannelInputDto, EffectivePages>();
            CreateMap<EffectiveChannelInputDto, EffectiveSocial>();
            CreateMap<EffectiveChannelInputDto, EffectiveWhastApp>();


            // Mapeamento da Entidade de Domínio para a Saída (DTO)
            CreateMap<ClientEntity, ClientDto>();
            CreateMap<CampaignConfig, CampaignConfigDto>();
            CreateMap<EffectiveChannel, EffectiveChannelDto>();

        }
    }
}
