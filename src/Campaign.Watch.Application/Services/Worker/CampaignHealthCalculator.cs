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

            // Se não houver erros de integração, verificamos se há um "Wait" ativo.
            if (!healthStatus.HasIntegrationErrors)
            {
                VerificarEtapaDeEsperaAtiva(campaign, healthStatus);
            }

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

        /// <summary>
        /// Verifica se a campanha está em um estado de espera legítimo devido a um componente "Wait" ativo.
        /// </summary>
        private void VerificarEtapaDeEsperaAtiva(CampaignEntity campaign, MonitoringHealthStatus healthStatus)
        {
            // Só executa essa verificação se a campanha estiver em execução e sem outros erros já detectados.
            if (campaign.StatusCampaign != CampaignStatus.Executing || healthStatus.HasIntegrationErrors)
            {
                return;
            }

            var lastExecution = campaign.Executions?.OrderBy(e => e.StartDate).LastOrDefault();
            if (lastExecution?.Steps == null) return;

            // Procura por um passo de "Wait" que esteja atualmente em execução ("Running")
            var activeWaitStep = lastExecution.Steps.FirstOrDefault(s => s.Type == "Wait" && s.Status == "Running");

            if (activeWaitStep != null && activeWaitStep.TotalExecutionTime > 0)
            {
                // ASSUMINDO que TotalExecutionTime está em segundos. Se for milissegundos, use TimeSpan.FromMilliseconds.
                // Acesso direto à propriedade, sem o .Value
                var waitDuration = TimeSpan.FromSeconds(activeWaitStep.TotalExecutionTime);

                // A melhor estimativa que temos para o início da espera é o início da própria execução.
                var expectedEndTime = lastExecution.StartDate.Add(waitDuration);

                // Atualiza a mensagem de saúde com uma informação clara e útil.
                healthStatus.LastMessage = $"Em andamento. Aguardando componente de espera ('{activeWaitStep.Name}') até aprox. {expectedEndTime:dd/MM/yyyy HH:mm}.";

                // Garante que essa condição "saudável" não seja marcada como um problema de execução pendente.
                healthStatus.HasPendingExecution = false;
            }
        }
    }
}
