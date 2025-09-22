using Campaign.Watch.Application.Interfaces;
using Campaign.Watch.Application.Mappers;
using Campaign.Watch.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Campaign.Watch.Application.Resolver
{
    public static class ResolverIoC
    {
        public static IServiceCollection AddApplications(this IServiceCollection services)
        {
            services.AddTransient<IClientApplication, ClientApplication>();
            services.AddTransient<ICampaignApplication, CampaignApplication>();

            services.AddTransient<ICampaignMonitorApplication, CampaignMonitorApplication>();
            services.AddTransient<ICampaignMonitorFlow, CampaignMonitorFlow>();



            services.AddAutoMapper(typeof(ClientMapper));
            services.AddAutoMapper(typeof(CampaignMapper));

            services.AddAutoMapper(typeof(CampaignReadMapper));



            return services;
        }
    }
}
