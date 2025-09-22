using AutoMapper;
using Campaign.Watch.Application.Dtos.Campaign;
using Campaign.Watch.Application.Dtos.Client;
using Campaign.Watch.Application.Helpers;
using Campaign.Watch.Application.Interfaces.Campaign;
using Campaign.Watch.Application.Interfaces.Client;
using Campaign.Watch.Application.Interfaces.Read.Campaign;
using Campaign.Watch.Application.Interfaces.Worker;
using Campaign.Watch.Domain.Entities.Campaign;
using Campaign.Watch.Domain.Entities.Read.Campaign;
using Campaign.Watch.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Campaign.Watch.Application.Services.Worker
{
    public class CampaignMonitorFlow : ICampaignMonitorFlow
    {
        private readonly IClientApplication _clientApplication;
        private readonly ICampaignMonitorApplication _campaignMonitorApplication;
        private readonly ICampaignApplication _campaignApplication;
        private readonly ILogger<CampaignMonitorFlow> _logger;
        private readonly IMapper _mapper;

        public CampaignMonitorFlow(IClientApplication clientApplication,ICampaignMonitorApplication campaignMonitorApplication,ICampaignApplication campaignApplication,
            IMapper mapper,
            ILogger<CampaignMonitorFlow> logger)
        {
            _clientApplication = clientApplication;
            _campaignMonitorApplication = campaignMonitorApplication;
            _campaignApplication = campaignApplication;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task MonitorarCampanhasAsync()
        {
            _logger.LogInformation("Iniciando ciclo de monitoramento de campanhas...");
            var campanhasProcessadas = 0;
            var totalErros = 0;

            try
            {
                var clientes = await _clientApplication.GetAllClientsAsync() ?? Enumerable.Empty<ClientResponse>();
                var clientesAtivos = clientes.Where(c => c.IsActive).ToList();

                _logger.LogInformation("Foram encontrados {TotalClientesAtivos} clientes ativos para monitoramento.", clientesAtivos.Count);

                foreach (var cliente in clientesAtivos)
                {
                    using (_logger.BeginScope(new Dictionary<string, object> { ["ClientName"] = cliente.Name }))
                    {
                        try
                        {
                            var processadasCliente = await ProcessarCampanhasDoClienteAsync(cliente);
                            campanhasProcessadas += processadasCliente;
                        }
                        catch (Exception ex)
                        {
                            totalErros++;
                            _logger.LogError(ex, "Erro ao processar campanhas do cliente.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Erro fatal no fluxo de monitoramento.");
                throw;
            }

            _logger.LogInformation(
                "Ciclo concluído. Campanhas processadas: {CampanhasProcessadas}, Erros: {TotalErros}",
                campanhasProcessadas, totalErros);
        }

        private async Task<int> ProcessarCampanhasDoClienteAsync(ClientResponse client)
        {
            if (client.CampaignConfig?.Database == null)
            {
                _logger.LogWarning("Cliente sem configuração de banco de dados.");
                return 0;
            }

            _logger.LogInformation("Processando campanhas do cliente {cliente}.", client.Name);

            var campanhasOrigem = await BuscarCampanhasDaOrigemAsync(client);
            if (!campanhasOrigem.Any())
            {
                _logger.LogInformation("Nenhuma campanha encontrada.");
                return 0;
            }

            _logger.LogInformation("Encontradas {TotalCampanhas} campanhas.", campanhasOrigem.Count());

            var processadas = 0;
            foreach (var campanhaOrigem in campanhasOrigem)
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    ["CampaignId"] = campanhaOrigem.Id,
                    ["CampaignName"] = campanhaOrigem.Name
                }))
                {
                    try
                    {
                        await ProcessarCampanhaUnicaAsync(client, campanhaOrigem);
                        processadas++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Falha ao processar campanha.");
                    }
                }
            }

            return processadas;
        }

        private async Task<IEnumerable<CampaignRead>> BuscarCampanhasDaOrigemAsync(ClientResponse client)
        {
            try
            {
                return await _campaignMonitorApplication.GetSourceCampaignsByClientAsync(client.CampaignConfig.Database)
                       ?? Enumerable.Empty<CampaignRead>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar campanhas da origem: {DatabaseName}", client.CampaignConfig.Database);
                return Enumerable.Empty<CampaignRead>();
            }
        }

        private async Task ProcessarCampanhaUnicaAsync(ClientResponse client, CampaignRead campaignSource)
        {
            _logger.LogDebug("Iniciando processamento da campanha.");

            var campaignToMonitor = _mapper.Map<CampaignEntity>(campaignSource);
            campaignToMonitor.ClientName = client.Name;

            await EnriquecerCampanhaComExecucoesAsync(client, campaignToMonitor);

            var tipoCampanha = DeterminarTipoCampanha(campaignToMonitor);
            _logger.LogDebug("Tipo identificado: {TipoCampanha}", tipoCampanha);

            await ValidarEProcessarCampanhaAsync(campaignToMonitor, tipoCampanha);
        }

        private async Task EnriquecerCampanhaComExecucoesAsync(ClientResponse client, CampaignEntity campaign)
        {
            try
            {
                var execucoesOrigem = await _campaignMonitorApplication.GetSourceExecutionsByCampaignAsync(
                    client.CampaignConfig.Database, campaign.IdCampaign);

                if (execucoesOrigem?.Any() == true)
                {
                    campaign.Executions = _mapper.Map<List<Execution>>(execucoesOrigem);
                }
                else
                {
                    campaign.Executions = new List<Execution>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enriquecer campanha com dados de execuções.");
                campaign.Executions = new List<Execution>();
            }
        }

        private CampaignType DeterminarTipoCampanha(CampaignEntity campaign)
        {
            if (campaign.Scheduler?.IsRecurrent == true)
            {
                return CampaignType.Recurrent;
            }

            if (campaign.Executions?.Count > 1)
            {
                return CampaignType.Recurrent;
            }

            return CampaignType.Single;
        }

        private async Task ValidarEProcessarCampanhaAsync(CampaignEntity campaign, CampaignType tipoCampanha)
        {
            var campanhaExistente = await _campaignApplication.GetCampaignByIdCampaignAsync(campaign.IdCampaign);
            var agora = DateTime.UtcNow;

            if (campanhaExistente == null)
            {
                await CriarNovaCampanhaAsync(campaign, tipoCampanha, agora);
            }
            else
            {
                await AtualizarCampanhaExistenteAsync(campanhaExistente, campaign, tipoCampanha, agora);
            }
        }

        private async Task CriarNovaCampanhaAsync(CampaignEntity campaign, CampaignType tipoCampanha, DateTime now)
        {
            _logger.LogInformation("Criando nova campanha ({TipoCampanha}).", tipoCampanha);

            campaign.LastCheckMonitoring = now;
            campaign.MonitoringStatus = DeterminarStatusMonitoramentoInicial(campaign, tipoCampanha, now);

            if (tipoCampanha == CampaignType.Recurrent && campaign.Scheduler?.IsRecurrent == true)
            {
                campaign.NextExecutionMonitoring = SchedulerHelper.GetNextExecution(campaign.Scheduler.Crontab);
            }
            else
            {
                campaign.NextExecutionMonitoring = campaign.Scheduler?.StartDateTime;
            }

            var campaignDto = _mapper.Map<CampaignResponse>(campaign);
            await _campaignApplication.CreateCampaignAsync(campaignDto);

            _logger.LogInformation("Campanha criada com status: {MonitoringStatus}", campaign.MonitoringStatus);
        }

        private async Task AtualizarCampanhaExistenteAsync(CampaignResponse existente,CampaignEntity campanhaDaOrigem,CampaignType tipoCampanha,DateTime now)
        {
            bool mudouNaOrigem = campanhaDaOrigem.ModifiedAt > existente.ModifiedAt;
            bool precisaVerificarAtraso = DeveVerificarAtrasos(existente, tipoCampanha, now);
            bool temNovaExecucao = TemNovaExecucao(existente, campanhaDaOrigem);

            if (!mudouNaOrigem && !precisaVerificarAtraso && !temNovaExecucao)
            {
                _logger.LogDebug("Nenhuma atualização necessária.");
                return;
            }

            var motivoAtualizacao = new List<string>();
            if (mudouNaOrigem) motivoAtualizacao.Add("Modificação na origem");
            if (precisaVerificarAtraso) motivoAtualizacao.Add("Verificação de atraso");
            if (temNovaExecucao) motivoAtualizacao.Add("Nova execução detectada");

            _logger.LogInformation("Atualizando campanha. Motivos: {Motivos}", string.Join(", ", motivoAtualizacao));

            var campanhaAtualizada = _mapper.Map<CampaignResponse>(campanhaDaOrigem);
            campanhaAtualizada.Id = existente.Id;
            campanhaAtualizada.LastCheckMonitoring = now;
            campanhaAtualizada.MonitoringStatus = DeterminarStatusMonitoramentoAtualizado(
                existente, campanhaDaOrigem, tipoCampanha, now);

            if (tipoCampanha == CampaignType.Recurrent &&
                campanhaDaOrigem.Scheduler?.IsRecurrent == true)
            {
                campanhaAtualizada.NextExecutionMonitoring =
                    SchedulerHelper.GetNextExecution(campanhaDaOrigem.Scheduler.Crontab);
            }

            await _campaignApplication.UpdateCampaignAsync(existente.Id, campanhaAtualizada);

            _logger.LogInformation("Campanha atualizada para status: {MonitoringStatus}",
                campanhaAtualizada.MonitoringStatus);
        }

        private MonitoringStatus DeterminarStatusMonitoramentoInicial(CampaignEntity campaign,CampaignType tipoCampanha,DateTime now)
        {
            switch (campaign.StatusCampaign)
            {
                case CampaignStatus.Completed:
                    return tipoCampanha == CampaignType.Recurrent ?
                           MonitoringStatus.WaitingForNextExecution :
                           MonitoringStatus.Completed;

                case CampaignStatus.Error:
                case CampaignStatus.Canceled:
                    return MonitoringStatus.Failed;

                case CampaignStatus.Executing:
                    return MonitoringStatus.InProgress;

                case CampaignStatus.Scheduled:
                    if (campaign.Scheduler != null &&
                        now > campaign.Scheduler.StartDateTime &&
                        (campaign.Executions == null || !campaign.Executions.Any()))
                    {
                        _logger.LogWarning("Campanha agendada detectada como atrasada.");
                        return MonitoringStatus.ExecutionDelayed;
                    }
                    return MonitoringStatus.Pending;

                default:
                    return MonitoringStatus.Pending;
            }
        }

        private MonitoringStatus DeterminarStatusMonitoramentoAtualizado(CampaignResponse existente,CampaignEntity campanhaDaOrigem,CampaignType tipoCampanha,DateTime now)
        {
            switch (campanhaDaOrigem.StatusCampaign)
            {
                case CampaignStatus.Completed:
                    return tipoCampanha == CampaignType.Recurrent ?
                           MonitoringStatus.WaitingForNextExecution :
                           MonitoringStatus.Completed;

                case CampaignStatus.Error:
                case CampaignStatus.Canceled:
                    return MonitoringStatus.Failed;

                case CampaignStatus.Executing:
                    return MonitoringStatus.InProgress;

                case CampaignStatus.Scheduled:
                    if (campanhaDaOrigem.Scheduler != null &&
                        now > campanhaDaOrigem.Scheduler.StartDateTime &&
                        (campanhaDaOrigem.Executions == null || !campanhaDaOrigem.Executions.Any()))
                    {
                        return MonitoringStatus.ExecutionDelayed;
                    }

                    if (tipoCampanha == CampaignType.Recurrent &&
                        existente.MonitoringStatus == MonitoringStatus.WaitingForNextExecution)
                    {
                        return MonitoringStatus.WaitingForNextExecution;
                    }

                    return MonitoringStatus.Pending;

                default:
                    return existente.MonitoringStatus;
            }
        }

        private bool DeveVerificarAtrasos(CampaignResponse existente, CampaignType tipoCampanha, DateTime now)
        {
            if (tipoCampanha == CampaignType.Recurrent)
            {
                return existente.NextExecutionMonitoring.HasValue &&
                       now > existente.NextExecutionMonitoring.Value &&
                       existente.MonitoringStatus == MonitoringStatus.WaitingForNextExecution;
            }

            return existente.MonitoringStatus != MonitoringStatus.ExecutionDelayed &&
                   existente.StatusCampaign == CampaignStatus.Scheduled &&
                   existente.Scheduler != null &&
                   now > existente.Scheduler.StartDateTime &&
                   (existente.Executions == null || !existente.Executions.Any());
        }

        private bool TemNovaExecucao(CampaignResponse existente, CampaignEntity campanhaDaOrigem)
        {
            var execucoesExistentes = existente.Executions?.Count ?? 0;
            var execucoesAtuais = campanhaDaOrigem.Executions?.Count ?? 0;

            return execucoesAtuais > execucoesExistentes;
        }

        public enum CampaignType
        {
            Single,
            Recurrent
        }
    }
}