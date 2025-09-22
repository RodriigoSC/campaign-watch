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
            // Mapeamento da Entrada (Request) para a Entidade de Domínio
            CreateMap<SaveClientRequest, ClientEntity>();
            CreateMap<CampaignConfigDto, CampaignConfig>();
            CreateMap<EffectiveChannelDto, EffectiveChannel>()
                .ConvertUsing((src, dest, context) =>
                {
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

            CreateMap<EffectiveChannelDto, EffectiveMail>();
            CreateMap<EffectiveChannelDto, EffectiveSms>();
            CreateMap<EffectiveChannelDto, EffectivePush>();
            CreateMap<EffectiveChannelDto, EffectivePages>();
            CreateMap<EffectiveChannelDto, EffectiveSocial>();
            CreateMap<EffectiveChannelDto, EffectiveWhastApp>();


            // Mapeamento da Entidade de Domínio para a Saída (Response)
            CreateMap<ClientEntity, ClientResponse>();
            CreateMap<CampaignConfig, CampaignConfigDto>();
            CreateMap<EffectiveChannel, EffectiveChannelDto>();

        }
    }
}
