using AutoMapper;
using Campaign.Watch.Application.Dtos.Campaign;
using Campaign.Watch.Application.Dtos.Client;
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
    /// <summary>
    /// Orquestra o fluxo de monitoramento de campanhas, coordenando a busca,
    /// o processamento e a persistência dos dados.
    /// </summary>
    public class CampaignMonitorFlow : ICampaignMonitorFlow
    {
        private readonly IClientApplication _clientApplication;
        private readonly ICampaignMonitorApplication _campaignMonitorApplication;
        private readonly ICampaignApplication _campaignApplication;
        private readonly ICampaignDataProcessor _dataProcessor;
        private readonly ICampaignHealthCalculator _healthCalculator;
        private readonly ILogger<CampaignMonitorFlow> _logger;
        private readonly IMapper _mapper;

        public CampaignMonitorFlow(
            IClientApplication clientApplication,
            ICampaignMonitorApplication campaignMonitorApplication,
            ICampaignApplication campaignApplication,
            ICampaignDataProcessor dataProcessor,
            ICampaignHealthCalculator healthCalculator,
            IMapper mapper,
            ILogger<CampaignMonitorFlow> logger)
        {
            _clientApplication = clientApplication;
            _campaignMonitorApplication = campaignMonitorApplication;
            _campaignApplication = campaignApplication;
            _dataProcessor = dataProcessor;
            _healthCalculator = healthCalculator;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Ponto de entrada principal para o processo de monitoramento de campanhas.
        /// </summary>
        public async Task MonitorarCampanhasAsync()
        {
            _logger.LogInformation("Iniciando ciclo de monitoramento de campanhas...");
            var campanhasProcessadas = 0;
            var clientesComErro = 0;

            try
            {
                var clientes = await _clientApplication.GetAllClientsAsync() ?? Enumerable.Empty<ClientResponse>();
                var clientesAtivos = clientes.Where(c => c.IsActive).ToList();

                _logger.LogInformation("Foram encontrados {TotalClientesAtivos} clientes ativos para monitoramento.", clientesAtivos.Count);

                foreach (var cliente in clientesAtivos)
                {
                    try
                    {
                        using var clientScope = _logger.BeginScope(new Dictionary<string, object> { ["ClientName"] = cliente.Name });
                        var processadasCliente = await ProcessarCampanhasDoClienteAsync(cliente);
                        campanhasProcessadas += processadasCliente;
                    }
                    catch (Exception ex)
                    {
                        clientesComErro++;
                        _logger.LogError(ex, "Erro inesperado ao processar campanhas para o cliente {ClientName}.", cliente.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Erro fatal no fluxo de monitoramento de campanhas. O ciclo foi interrompido.");
                throw;
            }

            _logger.LogInformation(
                "Ciclo de monitoramento concluído. Total de campanhas processadas: {CampanhasProcessadas}, Clientes com erro: {ClientesComErro}",
                campanhasProcessadas, clientesComErro);
        }

        /// <summary>
        /// Processa todas as campanhas monitoráveis de um cliente específico.
        /// </summary>
        private async Task<int> ProcessarCampanhasDoClienteAsync(ClientResponse client)
        {
            if (client.CampaignConfig?.Database == null)
            {
                _logger.LogWarning("Cliente {ClientName} não possui configuração de banco de dados de campanha e será ignorado.", client.Name);
                return 0;
            }

            _logger.LogInformation("Iniciando processamento de campanhas do cliente.");

            var campanhasOrigem = await BuscarCampanhasDaOrigemAsync(client);
            if (!campanhasOrigem.Any())
            {
                _logger.LogInformation("Nenhuma campanha encontrada na origem para o cliente.");
                return 0;
            }

            _logger.LogInformation("Encontradas {TotalCampanhas} campanhas na origem.", campanhasOrigem.Count());

            var processadasCount = 0;
            foreach (var campanhaOrigem in campanhasOrigem)
            {
                try
                {
                    using var campaignScope = _logger.BeginScope(new Dictionary<string, object>
                    {
                        ["CampaignId"] = campanhaOrigem.Id,
                        ["CampaignName"] = campanhaOrigem.Name
                    });

                    await ProcessarCampanhaUnicaAsync(client, campanhaOrigem);
                    processadasCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao processar uma campanha específica. Continuando para a próxima.");
                }
            }
            return processadasCount;
        }

        /// <summary>
        /// Busca as campanhas da base de dados de origem do cliente.
        /// </summary>
        private async Task<IEnumerable<CampaignRead>> BuscarCampanhasDaOrigemAsync(ClientResponse client)
        {
            try
            {
                var campaigns = await _campaignMonitorApplication.GetSourceCampaignsByClientAsync(client.CampaignConfig.Database);
                return campaigns ?? Enumerable.Empty<CampaignRead>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar campanhas da origem do banco de dados: {DatabaseName}", client.CampaignConfig.Database);
                return Enumerable.Empty<CampaignRead>();
            }
        }

        /// <summary>
        /// Orquestra o processamento de uma única campanha, delegando tarefas para outros serviços.
        /// </summary>
        private async Task ProcessarCampanhaUnicaAsync(ClientResponse client, CampaignRead campaignSource)
        {
            _logger.LogDebug("Iniciando processamento da campanha.");

            // 1. Delega o processamento e enriquecimento dos dados
            var campaignToMonitor = await _dataProcessor.ProcessAndEnrichCampaignDataAsync(client, campaignSource);

            // 2. Delega o cálculo de saúde e status
            var healthResult = _healthCalculator.Calculate(campaignToMonitor, DateTime.UtcNow);

            // 3. Atribui os resultados calculados de volta à entidade
            campaignToMonitor.HealthStatus = healthResult.HealthStatus;
            campaignToMonitor.MonitoringStatus = healthResult.MonitoringStatus;
            campaignToMonitor.NextExecutionMonitoring = healthResult.NextExecutionTime;

            _logger.LogDebug("Tipo da campanha identificado como: {TipoCampanha}", healthResult.CampaignType);

            // 4. Orquestra a persistência dos dados
            await SalvarOuAtualizarCampanhaAsync(campaignToMonitor, healthResult.CampaignType);
        }

        /// <summary>
        /// Verifica se uma campanha já existe no banco de dados local e decide se deve criá-la ou atualizá-la.
        /// </summary>
        private async Task SalvarOuAtualizarCampanhaAsync(CampaignEntity campaign, CampaignType tipoCampanha)
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

        /// <summary>
        /// Cria uma nova campanha no banco de dados local.
        /// </summary>
        private async Task CriarNovaCampanhaAsync(CampaignEntity campaign, DateTime now)
        {
            _logger.LogInformation("Campanha não encontrada localmente. Criando novo registro.");

            campaign.LastCheckMonitoring = now;

            var campaignDto = _mapper.Map<CampaignResponse>(campaign);
            await _campaignApplication.CreateCampaignAsync(campaignDto);

            _logger.LogInformation("Nova campanha criada com status de monitoramento: {MonitoringStatus}", campaign.MonitoringStatus);
        }

        /// <summary>
        /// Atualiza uma campanha existente com novos dados da origem e status de monitoramento.
        /// </summary>
        private async Task AtualizarCampanhaExistenteAsync(CampaignResponse existente, CampaignEntity origem, DateTime now)
        {
            var paraAtualizar = _mapper.Map<CampaignEntity>(existente);

            // Atualiza a entidade local com os dados mais recentes da origem e os calculados
            paraAtualizar.Executions = origem.Executions;
            paraAtualizar.StatusCampaign = origem.StatusCampaign;
            paraAtualizar.ModifiedAt = origem.ModifiedAt;
            paraAtualizar.Scheduler = origem.Scheduler;
            paraAtualizar.HealthStatus = origem.HealthStatus;
            paraAtualizar.MonitoringStatus = origem.MonitoringStatus;
            paraAtualizar.NextExecutionMonitoring = origem.NextExecutionMonitoring;

            // Verifica se uma atualização é realmente necessária
            if (!IsUpdateRequired(existente, origem, paraAtualizar.HealthStatus))
            {
                _logger.LogDebug("Nenhuma alteração funcional detectada. A atualização da campanha não é necessária.");
                return;
            }

            _logger.LogInformation("Detectada alteração na origem ou no status de saúde. Atualizando campanha.");
            paraAtualizar.LastCheckMonitoring = now;

            var dtoParaAtualizar = _mapper.Map<CampaignResponse>(paraAtualizar);
            await _campaignApplication.UpdateCampaignAsync(existente.Id, dtoParaAtualizar);

            _logger.LogInformation("Campanha atualizada. Novo status de monitoramento: {MonitoringStatus}", paraAtualizar.MonitoringStatus);
        }

        /// <summary>
        /// Determina se uma campanha precisa ser atualizada no banco de dados.
        /// </summary>
        private bool IsUpdateRequired(CampaignResponse existente, CampaignEntity origem, MonitoringHealthStatus novoStatusSaude)
        {
            bool mudouNaOrigem = origem.ModifiedAt > existente.ModifiedAt;
            bool temNovaExecucao = (origem.Executions?.Count ?? 0) > (existente.Executions?.Count ?? 0);
            bool mudouStatusSaude = !Equals(existente.HealthStatus, novoStatusSaude);

            return mudouNaOrigem || temNovaExecucao || mudouStatusSaude;
        }
    }
}