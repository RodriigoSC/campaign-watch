using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Campaign.Watch.Domain.Entities.Read.Campaign
{
    /// <summary>
    /// Representa os dados brutos de uma execução de campanha lidos da fonte de dados de origem.
    /// A anotação BsonIgnoreExtraElements permite que campos não mapeados na origem sejam ignorados.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class ExecutionRead
    {
        /// <summary>
        /// O ID único do documento de execução na origem.
        /// </summary>
        [JsonPropertyName("_id")]
        public ObjectId Id { get; set; }

        /// <summary>
        /// ID do projeto ao qual a execução pertence.
        /// </summary>
        [JsonPropertyName("ProjectId")]
        public ObjectId ProjectId { get; set; }

        /// <summary>
        /// ID da campanha pai desta execução.
        /// </summary>
        [JsonPropertyName("CampaignId")]
        public ObjectId CampaignId { get; set; }

        /// <summary>
        /// Nome da campanha pai.
        /// </summary>
        [JsonPropertyName("CampaignName")]
        public string CampaignName { get; set; }

        /// <summary>
        /// ID que identifica unicamente esta instância de execução.
        /// </summary>
        [JsonPropertyName("ExecutionId")]
        public ObjectId ExecutionId { get; set; }

        /// <summary>
        /// ID da execução anterior (relevante para campanhas recorrentes).
        /// </summary>
        [JsonPropertyName("LastExecutionId")]
        public ObjectId LastExecutionId { get; set; }

        /// <summary>
        /// Data e hora de início da execução.
        /// </summary>
        [JsonPropertyName("StartDate")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Data e hora de término da execução. Pode ser nulo se ainda estiver em andamento.
        /// </summary>
        [JsonPropertyName("EndDate")]
        public object EndDate { get; set; }

        /// <summary>
        /// Status atual da execução (ex: "Running", "Completed").
        /// </summary>
        [JsonPropertyName("Status")]
        public string Status { get; set; }

        /// <summary>
        /// Indica se a execução foi cancelada.
        /// </summary>
        [JsonPropertyName("IsCanceled")]
        public bool IsCanceled { get; set; }

        /// <summary>
        /// Query externa associada a esta execução, se houver.
        /// </summary>
        [JsonPropertyName("QueryExternal")]
        public string QueryExternal { get; set; }

        /// <summary>
        /// Flag para indicar se uma contagem deve ser realizada.
        /// </summary>
        [JsonPropertyName("FlagCount")]
        public bool FlagCount { get; set; }

        /// <summary>
        /// Lista das etapas do workflow que foram executadas.
        /// </summary>
        [JsonPropertyName("WorkflowExecution")]
        public List<WorkflowExecutionReadModel> WorkflowExecution { get; set; }
    }

    /// <summary>
    /// Representa uma etapa individual dentro do fluxo de uma execução de campanha.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class WorkflowExecutionReadModel
    {
        /// <summary>
        /// ID único da etapa do workflow.
        /// </summary>
        [JsonPropertyName("_id")]
        public ObjectId Id { get; set; }

        /// <summary>
        /// Nome da etapa.
        /// </summary>
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Tipo da etapa (ex: "Filter", "Channel", "Wait").
        /// </summary>
        [JsonPropertyName("Type")]
        public string Type { get; set; }

        /// <summary>
        /// Status da etapa (ex: "Completed", "Running").
        /// </summary>
        [JsonPropertyName("Status")]
        public string Status { get; set; }

        /// <summary>
        /// Data e hora de início da execução desta etapa.
        /// </summary>
        [JsonPropertyName("StartDate")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Número total de usuários processados nesta etapa.
        /// </summary>
        [JsonPropertyName("TotalUsers")]
        public int TotalUsers { get; set; }

        /// <summary>
        /// Tempo total de execução desta etapa em milissegundos.
        /// </summary>
        [JsonPropertyName("TotalExecutionTime")]
        public int TotalExecutionTime { get; set; }

        /// <summary>
        /// Objeto que armazena informações de erro, se houver.
        /// </summary>
        [JsonPropertyName("Error")]
        public object Error { get; set; }
    }
}