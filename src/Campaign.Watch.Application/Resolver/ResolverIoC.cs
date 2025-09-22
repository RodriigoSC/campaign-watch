using Campaign.Watch.Application.Interfaces.Campaign;
using Campaign.Watch.Application.Interfaces.Client;
using Campaign.Watch.Application.Interfaces.Read;
using Campaign.Watch.Application.Interfaces.Worker;
using Campaign.Watch.Application.Mappers.Campaign;
using Campaign.Watch.Application.Mappers.Client;
using Campaign.Watch.Application.Mappers.Read.Campaign;
using Campaign.Watch.Application.Services.Campaign;
using Campaign.Watch.Application.Services.Client;
using Campaign.Watch.Application.Services.Read.Campaign;
using Campaign.Watch.Application.Services.Worker;
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
            services.AddAutoMapper(typeof(ExecutionReadMapper));

            return services;
        }
    }
}
