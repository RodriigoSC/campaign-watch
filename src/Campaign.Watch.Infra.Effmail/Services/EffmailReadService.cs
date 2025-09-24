using Campaign.Watch.Domain.Entities.Read.Effmail;
using Campaign.Watch.Domain.Interfaces.Services.Read.Effmail;
using Campaign.Watch.Infra.Effmail.Factories;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Campaign.Watch.Infra.Effmail.Services
{
    public class EffmailReadService : IEffmailReadService
    {
        private readonly IEffmailMongoFactory _factory;

        public EffmailReadService(IEffmailMongoFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Busca as triggers de Effmail associadas a um workflowId, 
        /// agregando as estatísticas da collection de Leads de forma performática.
        /// </summary>
        /// <param name="dbName">O nome do banco de dados do cliente.</param>
        /// <param name="workflowId">O ID do workflow a ser buscado.</param>
        /// <returns>Uma coleção de entidades EffmailRead montadas com os dados da Trigger e as estatísticas dos Leads.</returns>
        public async Task<IEnumerable<EffmailRead>> GetTriggerEffmail(string dbName, string workflowId)
        {
            // 1. Obtém a instância do banco de dados através da factory
            var database = _factory.GetDatabase(dbName);
            var triggerCollection = database.GetCollection<EffmailRead>("Trigger");

            // 2. Constrói o Pipeline de Agregação do MongoDB
            var pipeline = triggerCollection.Aggregate()
                // ETAPA 1: Filtra ($match) as triggers pelo WorkflowId.
                // É crucial ter um índice em "Parameters.WorkflowId" na collection Trigger para performance.
                .Match(t => t.Parameters.WorkflowId == workflowId)

                // ETAPA 2: Realiza a junção ($lookup) com a collection de Leads.
                // Para cada trigger, busca os leads correspondentes e os agrupa em um array temporário.
                .Lookup<EffmailRead, LeadDocument, EffmailRead>(
                    foreignCollection: database.GetCollection<LeadDocument>("Lead"),
                    localField: t => t.Id,
                    foreignField: l => l.TriggerId,
                    @as: (EffmailRead e) => e.Leads.Items
                )

                // ETAPA 3: Projeta ($project) o resultado final.
                // Mapeia os campos da trigger e calcula as contagens de status dos leads.
                .Project(trigger => new EffmailRead
                {
                    // Mapeamento dos campos da Trigger
                    Id = trigger.Id,
                    Status = trigger.Status,
                    Name = trigger.Name,
                    AppointmentDate = trigger.AppointmentDate,
                    StatusTrigger = trigger.StatusTrigger,
                    Parameters = trigger.Parameters,
                    IsTest = trigger.IsTest,
                    ReplyTo = trigger.ReplyTo,
                    CreatedAt = trigger.CreatedAt,
                    ModifiedAt = trigger.ModifiedAt,
                    TemplateId = trigger.TemplateId,
                    SchedulerId = trigger.SchedulerId,
                    Transactional = trigger.Transactional,
                    WebhookEnabled = trigger.WebhookEnabled,
                    ExistsExternalId = trigger.ExistsExternalId,
                    WebhookAPIs = trigger.WebhookAPIs,
                    File = trigger.File,
                    Error = trigger.Error,

                    // Cálculo e montagem do objeto 'Leads' com as estatísticas
                    Leads = new Leads
                    {
                        TriggerId = trigger.Id.ToString(),                        
                        Success = trigger.Leads.Items.Count(lead => lead.LastStatus == "Success"),
                        Error = trigger.Leads.Items.Count(lead => lead.LastStatus == "Error"),
                        Blocked = trigger.Leads.Items.Count(lead => lead.LastStatus == "Blocked"),
                        Optout = trigger.Leads.Items.Count(lead => lead.LastStatus == "Optout"),                        
                        Deduplication = trigger.Leads.Items.Count(lead => lead.LastStatus == "Deduplication")
                    }
                });

            // 3. Executa a agregação e retorna a lista
            return await pipeline.ToListAsync();
        }
    }
}