using Campaign.Watch.Domain.Entities.Campaign;
using Campaign.Watch.Domain.Enums;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Domain.Interfaces.Services.Campaign
{
    /// <summary>
    /// Define a interface para o serviço de campanhas, especificando as operações de negócio.
    /// </summary>
    public interface ICampaignService
    {
        /// <summary>
        /// Cria uma nova campanha.
        /// </summary>
        /// <param name="entity">A entidade da campanha a ser criada.</param>
        /// <returns>A entidade da campanha após a criação.</returns>
        Task<CampaignEntity> CreateCampaignAsync(CampaignEntity entity);

        /// <summary>
        /// Atualiza uma campanha existente.
        /// </summary>
        /// <param name="id">O ObjectId da campanha a ser atualizada.</param>
        /// <param name="entity">A entidade com os dados atualizados.</param>
        /// <returns>Retorna true se a atualização foi bem-sucedida, caso contrário, false.</returns>
        Task<bool> UpdateCampaignAsync(ObjectId id, CampaignEntity entity);

        /// <summary>
        /// Busca todas as campanhas.
        /// </summary>
        /// <returns>Uma coleção com todas as campanhas.</returns>
        Task<IEnumerable<CampaignEntity>> GetAllCampaignsAsync();

        /// <summary>
        /// Busca todas as campanhas de um cliente específico.
        /// </summary>
        /// <param name="clientName">O nome do cliente.</param>
        /// <returns>Uma coleção de campanhas do cliente.</returns>
        Task<IEnumerable<CampaignEntity>> GetAllCampaignsByClientAsync(string clientName);

        /// <summary>
        /// Busca todas as campanhas dentro de um intervalo de datas.
        /// </summary>
        /// <param name="start">A data de início do período.</param>
        /// <param name="finish">A data de fim do período.</param>
        /// <returns>Uma coleção de campanhas no intervalo especificado.</returns>
        Task<IEnumerable<CampaignEntity>> GetAllCampaignsByDateAsync(DateTime start, DateTime finish);

        /// <summary>
        /// Busca todas as campanhas por cliente ou por um intervalo de datas.
        /// </summary>
        /// <param name="clientName">O nome do cliente (pode ser nulo ou vazio).</param>
        /// <param name="start">A data de início do período.</param>
        /// <param name="finish">A data de fim do período.</param>
        /// <returns>Uma coleção de campanhas que atendem aos critérios.</returns>
        Task<IEnumerable<CampaignEntity>> GetAllCampaignsByClientOrDateAsync(string clientName, DateTime start, DateTime finish);

        /// <summary>
        /// Busca uma campanha pelo seu ObjectId.
        /// </summary>
        /// <param name="id">O ObjectId da campanha.</param>
        /// <returns>A entidade da campanha, ou nulo se não for encontrada.</returns>
        Task<CampaignEntity> GetCampaignByIdAsync(ObjectId id);

        /// <summary>
        /// Busca uma campanha pelo seu nome.
        /// </summary>
        /// <param name="campaignName">O nome da campanha.</param>
        /// <returns>A entidade da campanha, ou nulo se não for encontrada.</returns>
        Task<CampaignEntity> GetCampaignByNameAsync(string campaignName);

        /// <summary>
        /// Busca uma campanha pelo seu número de identificação.
        /// </summary>
        /// <param name="campaignNumber">O número da campanha.</param>
        /// <returns>A entidade da campanha, ou nulo se não for encontrada.</returns>
        Task<CampaignEntity> GetCampaignByNumberAsync(long campaignNumber);

        /// <summary>
        /// Busca uma campanha pelo seu ID de origem (IdCampaign).
        /// </summary>
        /// <param name="idCampaign">O ID de origem da campanha.</param>
        /// <returns>A entidade da campanha, ou nulo se não for encontrada.</returns>
        Task<CampaignEntity> GetCampaignByIdCampaignAsync(string clientName, string idCampaign);

        /// <summary>
        /// Busca todas as campanhas ativas.
        /// </summary>
        /// <returns>Uma coleção de campanhas ativas.</returns>
        Task<IEnumerable<CampaignEntity>> GetActiveCampaignsAsync();

        /// <summary>
        /// Busca campanhas por um status específico.
        /// </summary>
        /// <param name="statusCampaign">O status para filtrar as campanhas.</param>
        /// <returns>Uma coleção de campanhas com o status fornecido.</returns>
        Task<IEnumerable<CampaignEntity>> GetCampaignsByStatusAsync(CampaignStatus statusCampaign);

        /// <summary>
        /// Busca uma lista paginada de campanhas.
        /// </summary>
        /// <param name="page">O número da página.</param>
        /// <param name="pageSize">O tamanho da página.</param>
        /// <returns>Uma coleção de campanhas da página especificada.</returns>
        Task<IEnumerable<CampaignEntity>> GetCampaignsPaginatedAsync(int page, int pageSize);

        /// <summary>
        /// Conta o total de campanhas para um cliente.
        /// </summary>
        /// <param name="clientName">O nome do cliente.</param>
        /// <returns>O número de campanhas do cliente.</returns>
        Task<int> CountCampaignsByClientAsync(string clientName);

        Task<IEnumerable<CampaignEntity>> GetCampaignsDueForMonitoringAsync();

        /// <summary>
        /// Busca todas as campanhas com erro de integração.
        /// </summary>
        /// <returns>Uma coleção de campanhas com erro de integração.</returns>
        Task<IEnumerable<CampaignEntity>> GetCampaignsWithIntegrationErrorsAsync();

        /// <summary>
        /// Busca todas as campanhas com execução atrasada.
        /// </summary>
        /// <returns>Uma coleção de campanhas com execução atrasada.</returns>
        Task<IEnumerable<CampaignEntity>> GetCampaignsWithDelayedExecutionAsync();

        /// <summary>
        /// Busca todas as campanhas monitoradas com sucesso.
        /// </summary>
        /// <returns>Uma coleção de campanhas monitoradas com sucesso.</returns>
        Task<IEnumerable<CampaignEntity>> GetSuccessfullyMonitoredCampaignsAsync();

    }
}