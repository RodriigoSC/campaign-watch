using AutoMapper;
using Campaign.Watch.Application.Dtos;
using Campaign.Watch.Application.Dtos.Read.Campaign;
using Campaign.Watch.Application.Interfaces;
using Campaign.Watch.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Services
{
    public class CampaignMonitorFlow : ICampaignMonitorFlow
    {
        private readonly IClientApplication _clientApplication;
        private readonly ICampaignMonitorApplication _campaignMonitorApplication;
        private readonly ICampaignApplication _campaignApplication;
        private readonly ILogger<CampaignMonitorFlow> _logger;
        private readonly IMapper _mapper;

        public CampaignMonitorFlow(IClientApplication clientApplication, ICampaignMonitorApplication campaignMonitorApplication, ICampaignApplication campaignApplication, IMapper mapper, ILogger<CampaignMonitorFlow> logger)
        {
            _clientApplication = clientApplication;
            _campaignMonitorApplication = campaignMonitorApplication;
            _campaignApplication = campaignApplication;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task MonitorCampaignsAsync()
        {
            _logger.LogInformation("Iniciando monitoramento de campanhas...");

            var clients = await _clientApplication.GetAllClientsAsync() ?? Enumerable.Empty<ClientDto>();
            var activeClients = clients.Where(c => c.IsActive);

            foreach (var client in activeClients)
            {
                if (client.CampaignConfig == null || string.IsNullOrEmpty(client.CampaignConfig.Database))
                {
                    _logger.LogWarning("Cliente {ClientName} sem configuração de campanha.", client.Name);
                    continue;
                }

                _logger.LogInformation("Buscando campanhas para o cliente {ClientName}", client.Name);

                IEnumerable<CampaignReadDto> sourceCampaigns;

                try
                {
                    sourceCampaigns = await _campaignMonitorApplication
                        .GetSourceCampaignsByClientAsync(client.CampaignConfig.Database)
                        ?? Enumerable.Empty<CampaignReadDto>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao buscar campanhas do cliente {ClientName}", client.Name);
                    continue;
                }

                foreach (var source in sourceCampaigns)
                {
                    try
                    {
                        var campaignDto = _mapper.Map<CampaignDto>(source);
                        campaignDto.ClientName = client.Name;

                        await ValidateCampaignAsync(campaignDto);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao validar campanha do cliente {client.Name}: {ex}");
                    }
                }
            }

            _logger.LogInformation("Monitoramento finalizado.");
        }

        private async Task ValidateCampaignAsync(CampaignDto campaign)
        {
            var existing = await _campaignApplication.GetCampaignByIdCampaignAsync(campaign.IdCampaign);
            var now = DateTime.UtcNow;

            if (existing == null)
            {
                // --- LÓGICA PARA NOVAS CAMPANHAS ---
                campaign.LastCheckMonitoring = now;
                campaign.NextExecutionMonitoring = campaign.Scheduler?.StartDateTime;

                // Valida se a campanha já nasce atrasada
                if (campaign.StatusCampaign == CampaignStatus.Scheduled && now > campaign.Scheduler.StartDateTime)
                {
                    campaign.MonitoringStatus = MonitoringStatus.ExecutionDelayed;
                    _logger.LogWarning("Nova campanha detectada com execução atrasada: {CampaignName}", campaign.Name);
                }
                else
                {
                    campaign.MonitoringStatus = MonitoringStatus.Pending;
                }

                await _campaignApplication.CreateCampaignAsync(campaign);
                _logger.LogInformation("Campanha criada: {CampaignName} com status de monitoramento {MonitoringStatus}", campaign.Name, campaign.MonitoringStatus);
            }
            else
            {
                // --- LÓGICA PARA CAMPANHAS EXISTENTES ---
                if (campaign.ModifiedAt <= existing.ModifiedAt)
                {
                    // Se não houve modificação, apenas verificamos se está atrasada (caso ainda não tenha sido marcado)
                    if (existing.MonitoringStatus != MonitoringStatus.ExecutionDelayed &&
                        existing.StatusCampaign == CampaignStatus.Scheduled &&
                        now > existing.Scheduler.StartDateTime &&
                        (existing.Executions == null || !existing.Executions.Any()))
                    {
                        existing.MonitoringStatus = MonitoringStatus.ExecutionDelayed;
                        existing.LastCheckMonitoring = now;
                        await _campaignApplication.UpdateCampaignAsync(existing.Id, existing);
                        _logger.LogWarning("Campanha existente encontrada com execução atrasada: {CampaignName}", existing.Name);
                    }
                    return; // Pula a atualização se não houver mudanças
                }

                // Mapeia as atualizações da origem
                campaign.Id = existing.Id;
                campaign.LastCheckMonitoring = now;

                // Se a campanha foi concluída na origem
                if (campaign.StatusCampaign == CampaignStatus.Completed)
                {
                    campaign.MonitoringStatus = MonitoringStatus.Completed;
                    campaign.NextExecutionMonitoring = null;
                }
                // Se a campanha é recorrente e está agendada (esperando próximo ciclo)
                else if (campaign.Scheduler.IsRecurrent && campaign.StatusCampaign == CampaignStatus.Scheduled)
                {
                    campaign.MonitoringStatus = MonitoringStatus.WaitingForNextExecution;
                    // Aqui você precisaria de uma lógica para calcular a próxima data do crontab
                    // campaign.NextExecutionMonitoring = SchedulerHelper.GetNextExecution(campaign.Scheduler.Crontab);
                }
                else
                {
                    campaign.MonitoringStatus = existing.MonitoringStatus; // Mantém o status atual se não se encaixar nas regras acima
                }

                await _campaignApplication.UpdateCampaignAsync(existing.Id, campaign);
                _logger.LogInformation("Campanha atualizada: {CampaignName}", campaign.Name);
            }
        }
    }
}
