using AutoMapper;
using Campaign.Watch.Application.Dtos.Campaign;
using Campaign.Watch.Application.Dtos.Client;
using Campaign.Watch.Application.Dtos.Read.Campaign;
using Campaign.Watch.Application.Helpers;
using Campaign.Watch.Application.Interfaces.Campaign;
using Campaign.Watch.Application.Interfaces.Client;
using Campaign.Watch.Application.Interfaces.Read;
using Campaign.Watch.Application.Interfaces.Worker;
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

        public CampaignMonitorFlow(
            IClientApplication clientApplication,
            ICampaignMonitorApplication campaignMonitorApplication,
            ICampaignApplication campaignApplication,
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
                var clientes = await _clientApplication.GetAllClientsAsync() ?? Enumerable.Empty<ClientDto>();
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

        private async Task<int> ProcessarCampanhasDoClienteAsync(ClientDto client)
        {
            if (client.CampaignConfig?.Database == null)
            {
                _logger.LogWarning("Cliente sem configuração de banco de dados.");
                return 0;
            }

            _logger.LogInformation("Processando campanhas do cliente {cliente}.",client.Name);

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

        private async Task<IEnumerable<CampaignReadDto>> BuscarCampanhasDaOrigemAsync(ClientDto client)
        {
            try
            {
                return await _campaignMonitorApplication.GetSourceCampaignsByClientAsync(client.CampaignConfig.Database)
                       ?? Enumerable.Empty<CampaignReadDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar campanhas da origem: {DatabaseName}", client.CampaignConfig.Database);
                return Enumerable.Empty<CampaignReadDto>();
            }
        }

        private async Task ProcessarCampanhaUnicaAsync(ClientDto client, CampaignReadDto campaignSource)
        {
            _logger.LogDebug("Iniciando processamento da campanha.");

            // 1. Converter para DTO de monitoramento
            var campanhaMonitoring = _mapper.Map<CampaignMonitoringDto>(campaignSource);
            campanhaMonitoring.ClientName = client.Name;

            // 2. Enriquecer com execuções e análise detalhada
            await EnriquecerCampanhaComDadosComplementaresAsync(client, campanhaMonitoring);

            // 3. Determinar tipo de campanha (pontual vs recorrente)
            var tipoCampanha = DeterminarTipoCampanha(campanhaMonitoring);
            _logger.LogDebug("Tipo identificado: {TipoCampanha}", tipoCampanha);

            // 4. Processar baseado no tipo
            await ValidarEProcessarCampanhaAsync(campanhaMonitoring, tipoCampanha);
        }

        private async Task EnriquecerCampanhaComDadosComplementaresAsync(ClientDto client, CampaignMonitoringDto campaign)
        {
            try
            {
                // Buscar execuções
                var execucoesOrigem = await _campaignMonitorApplication.GetSourceExecutionsByCampaignAsync(
                    client.CampaignConfig.Database, campaign.IdCampaign);

                if (execucoesOrigem?.Any() == true)
                {
                    campaign.Executions = execucoesOrigem.Select(exec =>
                    {
                        var execDto = _mapper.Map<ExecutionMonitoringDto>(exec);

                        // Enriquecer cada workflow com informações adicionais
                        if (execDto.WorkflowSteps?.Any() == true)
                        {
                            EnriquecerWorkflowSteps(execDto.WorkflowSteps);
                        }

                        return execDto;
                    }).ToList();

                    _logger.LogDebug("Mapeadas {TotalExecucoes} execuções com {TotalSteps} steps total.",
                        campaign.Executions.Count,
                        campaign.Executions.Sum(e => e.WorkflowSteps?.Count ?? 0));
                }
                else
                {
                    campaign.Executions = new List<ExecutionMonitoringDto>();
                }

                // TODO: Buscar dados de integração com canais
                // await EnriquecerComDadosDosCanaisAsync(client, campaign);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enriquecer campanha com dados complementares.");
                campaign.Executions = new List<ExecutionMonitoringDto>();
            }
        }

        private void EnriquecerWorkflowSteps(List<WorkflowMonitoringDto> workflowSteps)
        {
            for (int i = 0; i < workflowSteps.Count; i++)
            {
                var step = workflowSteps[i];
                step.StepOrder = i + 1;

                // Identificar componentes de espera
                step.IsWaitingComponent = step.Type.Equals("Wait", StringComparison.OrdinalIgnoreCase);

                // Gerar mensagem de status personalizada
                if (string.IsNullOrEmpty(step.StatusMessage))
                {
                    step.StatusMessage = GerarMensagemStatus(step);
                }

                // Adicionar ao histórico
                if (!string.IsNullOrEmpty(step.StatusMessage))
                {
                    step.StatusHistory.Add($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {step.StatusMessage}");
                }
            }
        }

        private string GerarMensagemStatus(WorkflowMonitoringDto step)
        {
            return step.Type switch
            {
                "Filter" => $"Filtro processado com {step.TotalUsers:N0} usuários - Status: {step.Status}",
                "Channel" when step.Status == "Completed" => $"Canal integrado com sucesso - {step.TotalUsers:N0} usuários processados",
                "Channel" when step.Status == "Running" => "Integração com canal em andamento",
                "Channel" when step.Status == "Failed" => "Falha na integração com canal",
                "Wait" when step.Status == "Running" => "Aguardando período de espera configurado",
                "Wait" when step.Status == "Completed" => "Período de espera concluído",
                "DecisionSplit" => $"Decisão processada - {step.TotalUsers:N0} usuários direcionados",
                "RandomSplit" => $"Divisão aleatória realizada - {step.TotalUsers:N0} usuários distribuídos",
                "End" => "Fluxo finalizado com sucesso",
                _ when step.Status == "Failed" => $"Erro no step {step.Type}: {step.Error}",
                _ when step.Status == "Running" => $"Step {step.Type} em execução",
                _ => $"Step {step.Type} - Status: {step.Status}"
            };
        }

        private CampaignType DeterminarTipoCampanha(CampaignMonitoringDto campaign)
        {
            if (campaign.Scheduler?.IsRecurrent == true)
            {
                return CampaignType.Recurrent;
            }

            // Verificar pela presença de múltiplas execuções
            if (campaign.Executions?.Count > 1)
            {
                return CampaignType.Recurrent;
            }

            return CampaignType.Single;
        }

        private async Task ValidarEProcessarCampanhaAsync(CampaignMonitoringDto campaign, CampaignType tipoCampanha)
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

        private async Task CriarNovaCampanhaAsync(CampaignMonitoringDto campaign, CampaignType tipoCampanha, DateTime now)
        {
            _logger.LogInformation("Criando nova campanha ({TipoCampanha}).", tipoCampanha);

            var campanhaDto = _mapper.Map<CampaignDto>(campaign);
            campanhaDto.LastCheckMonitoring = now;
            campanhaDto.MonitoringStatus = DeterminarStatusMonitoramentoInicial(campaign, tipoCampanha, now);

            // Configurar próxima execução baseada no tipo
            if (tipoCampanha == CampaignType.Recurrent && campaign.Scheduler?.IsRecurrent == true)
            {
                campanhaDto.NextExecutionMonitoring = SchedulerHelper.GetNextExecution(campaign.Scheduler.Crontab);
            }
            else
            {
                campanhaDto.NextExecutionMonitoring = campaign.Scheduler?.StartDateTime;
            }

            await _campaignApplication.CreateCampaignAsync(campanhaDto);

            _logger.LogInformation("Campanha criada com status: {MonitoringStatus}", campanhaDto.MonitoringStatus);
        }

        private async Task AtualizarCampanhaExistenteAsync(
            CampaignDto existente,
            CampaignMonitoringDto campanhaDaOrigem,
            CampaignType tipoCampanha,
            DateTime now)
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

            var campanhaAtualizada = _mapper.Map<CampaignDto>(campanhaDaOrigem);
            campanhaAtualizada.Id = existente.Id;
            campanhaAtualizada.LastCheckMonitoring = now;
            campanhaAtualizada.MonitoringStatus = DeterminarStatusMonitoramentoAtualizado(
                existente, campanhaDaOrigem, tipoCampanha, now);

            // Atualizar próxima execução para campanhas recorrentes
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

        private MonitoringStatus DeterminarStatusMonitoramentoInicial(
            CampaignMonitoringDto campaign,
            CampaignType tipoCampanha,
            DateTime now)
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

        private MonitoringStatus DeterminarStatusMonitoramentoAtualizado(
            CampaignDto existente,
            CampaignMonitoringDto campanhaDaOrigem,
            CampaignType tipoCampanha,
            DateTime now)
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

        private bool DeveVerificarAtrasos(CampaignDto existente, CampaignType tipoCampanha, DateTime now)
        {
            // Para campanhas recorrentes, sempre verificar se há atraso na próxima execução
            if (tipoCampanha == CampaignType.Recurrent)
            {
                return existente.NextExecutionMonitoring.HasValue &&
                       now > existente.NextExecutionMonitoring.Value &&
                       existente.MonitoringStatus == MonitoringStatus.WaitingForNextExecution;
            }

            // Para campanhas pontuais
            return existente.MonitoringStatus != MonitoringStatus.ExecutionDelayed &&
                   existente.StatusCampaign == CampaignStatus.Scheduled &&
                   existente.Scheduler != null &&
                   now > existente.Scheduler.StartDateTime &&
                   (existente.Executions == null || !existente.Executions.Any());
        }

        private bool TemNovaExecucao(CampaignDto existente, CampaignMonitoringDto campanhaDaOrigem)
        {
            var execucoesExistentes = existente.Executions?.Count ?? 0;
            var execucoesAtuais = campanhaDaOrigem.Executions?.Count ?? 0;

            return execucoesAtuais > execucoesExistentes;
        }

        public enum CampaignType
        {
            Single,      // Pontual - uma única execução
            Recurrent    // Recorrente - múltiplas execuções baseadas no scheduler
        }
    }
}