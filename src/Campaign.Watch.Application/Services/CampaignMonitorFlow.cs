using Campaign.Watch.Application.Dtos;
using Campaign.Watch.Application.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Services
{
    public class CampaignMonitorFlow : ICampaignMonitorFlow
    {
        private readonly IClientApplication _clientApplication;
        private readonly ICampaignMonitorApplication _campaignMonitorApplication;
        private readonly ICampaignApplication _campaignApplication;

        public CampaignMonitorFlow(IClientApplication clientApplication, ICampaignMonitorApplication campaignMonitorApplication, ICampaignApplication campaignApplication)
        {
            _clientApplication = clientApplication;
            _campaignMonitorApplication = campaignMonitorApplication;
            _campaignApplication = campaignApplication;
        }

        public async Task MonitorCampaignsAsync()
        {
            //_logger.LogInformation("Iniciando monitoramento de campanhas...");

            var clients = await _clientApplication.GetAllClientsAsync();
            var activeClients = clients.Where(c => c.IsActive);

            foreach (var client in activeClients)
            {
                //_logger.LogInformation("Buscando campanhas para o cliente {ClientName}", client.Nome);

                var sourceCampaigns = await _campaignMonitorApplication.GetSourceCampaignsByClientAsync(client.CampaignConfig.Database);

                foreach (var source in sourceCampaigns)
                {
                    var mapped = new CampaignDto
                    {
                        ClientName = client.Name,
                        IdCampaign = source.Id,
                        NumberId = (int)source.NumberId,
                        Name = source.Name,
                        Description = source.Description,
                        ProjectId = source.ProjectId,
                        IsActive = source.IsActive,
                        CreatedAt = source.CreatedAt,
                        ModifiedAt = source.ModifiedAt,
                        //StatusCampaign = (Domain.Enums.CampaignStatus)source.Status,
                        //IsDeleted = source.IsDeleted,
                        //IsRestored = source.IsRestored
                    };

                    await ValidateCampaignAsync(mapped);
                }
            }

            //_logger.LogInformation("Monitoramento finalizado.");
        }

        public async Task ValidateCampaignAsync(CampaignDto campaign)
        {
            var existing = await _campaignApplication.GetCampaignByNameAsync(campaign.Name);

            if (existing == null)
            {
                await _campaignApplication.CreateCampaignAsync(campaign);
                //_logger.LogInformation("Campanha criada: {CampaignName}", campaign.Name);
            }
            else
            {
                await _campaignApplication.UpdateCampaignAsync(existing.Id, campaign);
               // _logger.LogInformation("Campanha atualizada: {CampaignName}", campaign.Name);
            }
        }
    }
}
