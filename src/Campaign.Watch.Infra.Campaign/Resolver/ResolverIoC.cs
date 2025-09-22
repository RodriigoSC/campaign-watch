using Campaign.Watch.Domain.Interfaces.Services.Read;
using Campaign.Watch.Infra.Campaign.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Campaign.Watch.Infra.Campaign.Resolver
{
    public static class ResolverIoC
    {
        public static IServiceCollection AddCampaignRepository(this IServiceCollection services)
        {
            services.AddTransient<ICampaignReadService, CampaignReadService>();

            return services;
        }
    }
}
