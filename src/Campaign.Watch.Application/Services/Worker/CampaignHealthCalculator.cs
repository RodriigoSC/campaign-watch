using Campaign.Watch.Application.Helpers;
using Campaign.Watch.Application.Interfaces.Worker;
using Campaign.Watch.Domain.Entities.Campaign;
using Campaign.Watch.Domain.Enums;
using System;
using System.Linq;

namespace Campaign.Watch.Application.Services.Worker
{
    public class CampaignHealthCalculator : ICampaignHealthCalculator
    {
        public CampaignHealthResult Calculate(CampaignEntity campaign, DateTime now)
        {
            var campaignType = DeterminarTipoCampanha(campaign);
            var nextExecution = CalcularProximaExecucao(campaign, campaignType, now);

            // Atribui temporariamente para o cálculo de saúde usar
            campaign.NextExecutionMonitoring = nextExecution;

            var healthStatus = CalcularStatusDeSaude(campaign, campaignType, now);
            var monitoringStatus = DeterminarStatusMonitoramento(campaign, healthStatus, campaignType);

            return new CampaignHealthResult(healthStatus, monitoringStatus, campaignType, nextExecution);
        }

        private CampaignType DeterminarTipoCampanha(CampaignEntity campaign)
        {
            return (campaign.Scheduler?.IsRecurrent == true || (campaign.Executions?.Count ?? 0) > 1)
                ? CampaignType.Recurrent
                : CampaignType.Single;
        }

        private DateTime? CalcularProximaExecucao(CampaignEntity campaign, CampaignType tipoCampanha, DateTime now)
        {
            if (tipoCampanha == CampaignType.Recurrent && campaign.Scheduler?.IsRecurrent == true && !string.IsNullOrWhiteSpace(campaign.Scheduler.Crontab))
            {
                return SchedulerHelper.GetNextExecution(campaign.Scheduler.Crontab, now);
            }
            return campaign.Scheduler?.StartDateTime;
        }

        private MonitoringHealthStatus CalcularStatusDeSaude(CampaignEntity campaign, CampaignType tipoCampanha, DateTime now)
        {
            var healthStatus = new MonitoringHealthStatus();
            var execucoesComErro = campaign.Executions?.Where(e => e.HasMonitoringErrors).ToList();

            healthStatus.HasIntegrationErrors = execucoesComErro?.Any() ?? false;
            if (healthStatus.HasIntegrationErrors)
            {
                var ultimaExecucaoComErro = execucoesComErro.OrderBy(e => e.StartDate).Last();
                healthStatus.LastExecutionWithIssueId = ultimaExecucaoComErro.ExecutionId;
                healthStatus.LastMessage = ultimaExecucaoComErro.Steps?.FirstOrDefault(s => !string.IsNullOrEmpty(s.MonitoringNotes))?.MonitoringNotes ?? "Erro de integração detectado.";
            }

            // A verificação de execução pendente agora é mais robusta com as execuções "fantasmas"
            if (!healthStatus.HasIntegrationErrors)
            {
                healthStatus.LastMessage = "Campanha monitorada sem problemas aparentes.";
            }

            return healthStatus;
        }

        private MonitoringStatus DeterminarStatusMonitoramento(CampaignEntity campaign, MonitoringHealthStatus healthStatus, CampaignType campaignType)
        {
            if (healthStatus.HasIntegrationErrors) return MonitoringStatus.Failed;
            if (healthStatus.HasPendingExecution) return MonitoringStatus.ExecutionDelayed;

            switch (campaign.StatusCampaign)
            {
                case CampaignStatus.Completed:
                    if (campaignType == CampaignType.Recurrent)
                        return MonitoringStatus.WaitingForNextExecution;
                    return healthStatus.IsFullyVerified ? MonitoringStatus.Completed : MonitoringStatus.InProgress;

                case CampaignStatus.Error:
                case CampaignStatus.Canceled:
                    return MonitoringStatus.Failed;
                case CampaignStatus.Executing:
                    return MonitoringStatus.InProgress;
                case CampaignStatus.Scheduled:
                    return MonitoringStatus.Pending;
                default:
                    return MonitoringStatus.Pending;
            }
        }
    }
}
