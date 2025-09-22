using AutoMapper;
using Campaign.Watch.Application.Dtos;
using Campaign.Watch.Application.Dtos.Read.Campaign;
using Campaign.Watch.Application.Helpers;
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
                    // Cria um escopo de log para este cliente. Todos os logs dentro deste 'using'
                    // terão o ClientName anexado automaticamente.
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
                            _logger.LogError(ex, "Ocorreu um erro não esperado ao processar as campanhas do cliente.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Erro fatal no orquestrador do fluxo de monitoramento. O ciclo será interrompido.");
                throw; // Propaga a exceção para o worker que o chamou.
            }

            _logger.LogInformation(
                "Ciclo de monitoramento concluído. Total de campanhas processadas: {CampanhasProcessadas}, Total de erros: {TotalErros}",
                campanhasProcessadas, totalErros);
        }

        private async Task<int> ProcessarCampanhasDoClienteAsync(ClientDto client)
        {
            if (client.CampaignConfig == null || string.IsNullOrEmpty(client.CampaignConfig.Database))
            {
                _logger.LogWarning("Cliente não possui configuração de banco de dados de campanha. Nenhuma campanha será processada.");
                return 0;
            }

            _logger.LogInformation("Iniciando processamento de campanhas para o cliente.");

            var campanhasOrigem = await BuscarCampanhasDaOrigemAsync(client);
            if (!campanhasOrigem.Any())
            {
                _logger.LogInformation("Nenhuma campanha encontrada na origem para o cliente.");
                return 0;
            }

            _logger.LogInformation("Encontradas {TotalCampanhas} campanhas na origem.", campanhasOrigem.Count());

            var contadorProcessadas = 0;
            foreach (var campanhaOrigem in campanhasOrigem)
            {
                // Cria um escopo de log para esta campanha específica.
                using (_logger.BeginScope(new Dictionary<string, object> { ["CampaignId"] = campanhaOrigem.Id, ["CampaignName"] = campanhaOrigem.Name }))
                {
                    try
                    {
                        await ProcessarCampanhaUnicaAsync(client, campanhaOrigem);
                        contadorProcessadas++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Falha ao processar a campanha.");
                    }
                }
            }
            return contadorProcessadas;
        }

        private async Task<IEnumerable<CampaignReadDto>> BuscarCampanhasDaOrigemAsync(ClientDto client)
        {
            try
            {
                return await _campaignMonitorApplication.GetSourceCampaignsByClientAsync(client.CampaignConfig.Database) ?? Enumerable.Empty<CampaignReadDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar a lista de campanhas do banco de dados de origem: {DatabaseName}", client.CampaignConfig.Database);
                return Enumerable.Empty<CampaignReadDto>(); 
            }
        }

        private async Task ProcessarCampanhaUnicaAsync(ClientDto client, CampaignReadDto campaignSource)
        {
            _logger.LogDebug("Mapeando e enriquecendo dados da campanha.");
            var campanhaDto = _mapper.Map<CampaignDto>(campaignSource);
            campanhaDto.ClientName = client.Name;

            await EnriquecerCampanhaComExecucoesAsync(client, campanhaDto);
            await ValidarEProcessarCampanhaAsync(campanhaDto);
        }

        private async Task EnriquecerCampanhaComExecucoesAsync(ClientDto client, CampaignDto campaign)
        {
            try
            {
                var execucoesOrigem = await _campaignMonitorApplication.GetSourceExecutionsByCampaignAsync(client.CampaignConfig.Database, campaign.IdCampaign);

                if (execucoesOrigem?.Any() != true)
                {
                    campaign.Executions = new List<ExecutionDto>();
                    _logger.LogDebug("Nenhuma execução encontrada para a campanha.");
                    return;
                }

                _logger.LogDebug("{TotalExecucoes} execuções encontradas, realizando mapeamento.", execucoesOrigem.Count());
                campaign.Executions = execucoesOrigem.Select(exec => _mapper.Map<ExecutionDto>(exec)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar ou mapear execuções para a campanha. A campanha será processada sem dados de execução.");
                campaign.Executions = new List<ExecutionDto>();
            }
        }

        private async Task ValidarEProcessarCampanhaAsync(CampaignDto campaign)
        {
            var campanhaExistente = await _campaignApplication.GetCampaignByIdCampaignAsync(campaign.IdCampaign);
            var agora = DateTime.UtcNow;

            if (campanhaExistente == null)
            {
                await CriarNovaCampanhaAsync(campaign, agora);
            }
            else
            {
                await AtualizarCampanhaExistenteAsync(campanhaExistente, campaign, agora);
            }
        }

        private async Task CriarNovaCampanhaAsync(CampaignDto campaign, DateTime now)
        {
            _logger.LogInformation("A campanha é nova em nosso sistema. Criando novo registro...");
            campaign.LastCheckMonitoring = now;
            campaign.NextExecutionMonitoring = campaign.Scheduler?.StartDateTime;
            campaign.MonitoringStatus = DeterminarStatusMonitoramentoInicial(campaign, now);

            await _campaignApplication.CreateCampaignAsync(campaign);

            _logger.LogInformation("Novo registro da campanha criado com status de monitoramento: {MonitoringStatus}", campaign.MonitoringStatus);
        }

        private async Task AtualizarCampanhaExistenteAsync(CampaignDto existente, CampaignDto campanhaDaOrigem, DateTime now)
        {
            bool mudouNaOrigem = campanhaDaOrigem.ModifiedAt > existente.ModifiedAt;
            bool precisaVerificarAtraso = DeveVerificarAtrasos(existente, now);

            // Condição para pular a atualização:
            // Se não mudou na origem E não é um caso de verificação de atraso, pula.
            // A verificação de campanhas em execução foi removida pois o status dela mudaria o ModifiedAt.
            if (!mudouNaOrigem && !precisaVerificarAtraso)
            {
                _logger.LogDebug("Campanha sem modificações na origem e sem verificação de atraso pendente. Nenhuma atualização necessária.");
                return;
            }

            _logger.LogInformation("Detectada necessidade de atualização. Motivo: {Motivo}", mudouNaOrigem ? "Modificação na origem" : "Verificação de atraso");

            campanhaDaOrigem.Id = existente.Id;
            campanhaDaOrigem.LastCheckMonitoring = now;
            campanhaDaOrigem.MonitoringStatus = DeterminarStatusMonitoramentoAtualizado(existente, campanhaDaOrigem, now);

            if (campanhaDaOrigem.Scheduler?.IsRecurrent == true && !string.IsNullOrEmpty(campanhaDaOrigem.Scheduler.Crontab))
            {
                campanhaDaOrigem.NextExecutionMonitoring = SchedulerHelper.GetNextExecution(campanhaDaOrigem.Scheduler.Crontab);
            }

            await _campaignApplication.UpdateCampaignAsync(existente.Id, campanhaDaOrigem);

            _logger.LogInformation("Campanha atualizada para o status de monitoramento: {MonitoringStatus}", campanhaDaOrigem.MonitoringStatus);
        }

        private MonitoringStatus DeterminarStatusMonitoramentoInicial(CampaignDto campaign, DateTime now)
        {
            switch (campaign.StatusCampaign)
            {
                case CampaignStatus.Completed:
                    return MonitoringStatus.Completed;
                case CampaignStatus.Error:
                case CampaignStatus.Canceled:
                    return MonitoringStatus.Failed;
                case CampaignStatus.Executing:
                    return MonitoringStatus.InProgress;
                case CampaignStatus.Scheduled:
                    // Verifica se já deveria ter começado e não começou
                    if (campaign.Scheduler != null && now > campaign.Scheduler.StartDateTime && (campaign.Executions == null || !campaign.Executions.Any()))
                    {
                        _logger.LogWarning("Campanha agendada detectada como atrasada na sua criação.");
                        return MonitoringStatus.ExecutionDelayed;
                    }
                    return MonitoringStatus.Pending;
                default:
                    return MonitoringStatus.Pending;
            }
        }

        private MonitoringStatus DeterminarStatusMonitoramentoAtualizado(CampaignDto existente, CampaignDto campaign, DateTime now)
        {
            switch (campaign.StatusCampaign)
            {
                case CampaignStatus.Completed:
                    return existente.Scheduler?.IsRecurrent == true ? MonitoringStatus.WaitingForNextExecution : MonitoringStatus.Completed;
                case CampaignStatus.Error:
                case CampaignStatus.Canceled:
                    return MonitoringStatus.Failed;
                case CampaignStatus.Executing:
                    return MonitoringStatus.InProgress;
                case CampaignStatus.Scheduled:
                    if (campaign.Scheduler != null && now > campaign.Scheduler.StartDateTime && (campaign.Executions == null || !campaign.Executions.Any()))
                    {
                        _logger.LogWarning("Campanha agendada agora está atrasada.");
                        return MonitoringStatus.ExecutionDelayed;
                    }
                    // Se não está atrasada, pode estar aguardando a próxima execução de uma recorrência
                    if (existente.MonitoringStatus == MonitoringStatus.WaitingForNextExecution)
                    {
                        return MonitoringStatus.WaitingForNextExecution;
                    }
                    return MonitoringStatus.Pending;
                default:
                    return existente.MonitoringStatus; // Mantém o status anterior se nenhuma regra se aplicar
            }
        }

        private bool DeveVerificarAtrasos(CampaignDto existente, DateTime now)
        {
            // A verificação só é necessária para campanhas agendadas que ainda não foram marcadas como atrasadas
            return existente.MonitoringStatus != MonitoringStatus.ExecutionDelayed &&
                   existente.StatusCampaign == CampaignStatus.Scheduled &&
                   existente.Scheduler != null &&
                   now > existente.Scheduler.StartDateTime &&
                   (existente.Executions == null || !existente.Executions.Any());
        }
    }
}
