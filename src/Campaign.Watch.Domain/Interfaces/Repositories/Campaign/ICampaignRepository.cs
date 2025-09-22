using Campaign.Watch.Domain.Entities.Campaign;
using Campaign.Watch.Domain.Enums;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campaign.Watch.Domain.Interfaces.Repositories.Campaign
{
    /// <summary>
    /// Define a interface para o repositório de campanhas, especificando os métodos de acesso a dados.
    /// </summary>
    public interface ICampaignRepository
    {
        /// <summary>
        /// Cria uma nova campanha no banco de dados.
        /// </summary>
        /// <param name="entity">A entidade da campanha a ser criada.</param>
        /// <returns>A entidade da campanha após ser criada.</returns>
        Task<CampaignEntity> CreateCampaignAsync(CampaignEntity entity);

        /// <summary>
        /// Atualiza uma campanha existente com base em seu ID.
        /// </summary>
        /// <param name="id">O ObjectId da campanha a ser atualizada.</param>
        /// <param name="entity">A entidade com os dados atualizados.</param>
        /// <returns>Retorna true se a atualização foi bem-sucedida, caso contrário, false.</returns>
        Task<bool> UpdateCampaignAsync(ObjectId id, CampaignEntity entity);

        /// <summary>
        /// Obtém todas as campanhas cadastradas.
        /// </summary>
        /// <returns>Uma coleção de todas as campanhas.</returns>
        Task<IEnumerable<CampaignEntity>> GetAllCampaignsAsync();

        /// <summary>
        /// Obtém todas as campanhas associadas a um cliente específico.
        /// </summary>
        /// <param name="clientName">O nome do cliente.</param>
        /// <returns>Uma coleção de campanhas do cliente especificado.</returns>
        Task<IEnumerable<CampaignEntity>> GetAllCampaignsByClientAsync(string clientName);

        /// <summary>
        /// Obtém todas as campanhas dentro de um intervalo de datas.
        /// </summary>
        /// <param name="start">A data de início do período.</param>
        /// <param name="finish">A data de fim do período.</param>
        /// <returns>Uma coleção de campanhas dentro do intervalo de datas.</returns>
        Task<IEnumerable<CampaignEntity>> GetAllCampaignsByDateAsync(DateTime start, DateTime finish);

        /// <summary>
        /// Obtém todas as campanhas filtrando por cliente ou por um intervalo de datas.
        /// </summary>
        /// <param name="clientName">O nome do cliente (pode ser nulo ou vazio).</param>
        /// <param name="start">A data de início do período.</param>
        /// <param name="finish">A data de fim do período.</param>
        /// <returns>Uma coleção de campanhas que atendem aos critérios de filtro.</returns>
        Task<IEnumerable<CampaignEntity>> GetAllCampaignsByClientOrDateAsync(string clientName, DateTime start, DateTime finish);

        /// <summary>
        /// Obtém uma campanha específica pelo seu ObjectId.
        /// </summary>
        /// <param name="id">O ObjectId da campanha.</param>
        /// <returns>A entidade da campanha correspondente ao ID, ou nulo se não encontrada.</returns>
        Task<CampaignEntity> GetCampaignByIdAsync(ObjectId id);

        /// <summary>
        /// Obtém uma campanha específica pelo seu nome.
        /// </summary>
        /// <param name="campaignName">O nome da campanha.</param>
        /// <returns>A entidade da campanha correspondente ao nome, ou nulo se não encontrada.</returns>
        Task<CampaignEntity> GetCampaignByNameAsync(string campaignName);

        /// <summary>
        /// Obtém uma campanha específica pelo seu número de identificação.
        /// </summary>
        /// <param name="campaignNumber">O número da campanha.</param>
        /// <returns>A entidade da campanha correspondente ao número, ou nulo se não encontrada.</returns>
        Task<CampaignEntity> GetCampaignByNumberAsync(long campaignNumber);

        /// <summary>
        /// Obtém uma campanha pelo seu ID de origem (IdCampaign).
        /// </summary>
        /// <param name="idCampaign">O ID de origem da campanha.</param>
        /// <returns>A entidade da campanha correspondente, ou nulo se não encontrada.</returns>
        Task<CampaignEntity> GetCampaignByIdCampaignAsync(string idCampaign);

        /// <summary>
        /// Obtém todas as campanhas ativas.
        /// </summary>
        /// <returns>Uma coleção de campanhas ativas.</returns>
        Task<IEnumerable<CampaignEntity>> GetActiveCampaignsAsync();

        /// <summary>
        /// Obtém todas as campanhas com um status específico.
        /// </summary>
        /// <param name="statusCampaign">O status da campanha para filtrar.</param>
        /// <returns>Uma coleção de campanhas com o status especificado.</returns>
        Task<IEnumerable<CampaignEntity>> GetCampaignsByStatusAsync(CampaignStatus statusCampaign);

        /// <summary>
        /// Obtém uma lista paginada de campanhas.
        /// </summary>
        /// <param name="page">O número da página a ser retornada.</param>
        /// <param name="pageSize">O número de itens por página.</param>
        /// <returns>Uma coleção de campanhas para a página especificada.</returns>
        Task<IEnumerable<CampaignEntity>> GetCampaignsPaginatedAsync(int page, int pageSize);

        /// <summary>
        /// Conta o número total de campanhas para um cliente específico.
        /// </summary>
        /// <param name="clientName">O nome do cliente.</param>
        /// <returns>O número total de campanhas do cliente.</returns>
        Task<int> CountCampaignsByClientAsync(string clientName);
    }
}