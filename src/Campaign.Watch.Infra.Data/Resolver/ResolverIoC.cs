using Campaign.Watch.Domain.Interfaces.Repositories;
using Campaign.Watch.Domain.Interfaces.Services;
using Campaign.Watch.Infra.Data.Repository;
using Campaign.Watch.Infra.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Campaign.Watch.Infra.Data.Resolver
{
    public static class ResolverIoC
    {
        public static IServiceCollection AddDataRepository(this IServiceCollection services)
        {
            services.AddTransient<IClientRepository, ClientRepository>();
            services.AddTransient<IClientService, ClientService>();

            services.AddTransient<ICampaignRepository, CampaignRepository>();
            services.AddTransient<ICampaignService, CampaignService>();

            return services;
        }
    }
}
