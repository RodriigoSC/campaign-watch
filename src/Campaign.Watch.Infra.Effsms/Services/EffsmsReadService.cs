using Campaign.Watch.Domain.Entities.Read.Effsms;
using Campaign.Watch.Domain.Interfaces.Services.Read.Effsms;
using Campaign.Watch.Infra.Effsms.Factories;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Campaign.Watch.Infra.Effsms.Services
{
    public class EffsmsReadService : IEffsmsReadService
    {
        private readonly IEffsmsMongoFactory _factory;

        public EffsmsReadService(IEffsmsMongoFactory factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<EffsmsRead>> GetTriggerEffsms(string dbName, string workflowId)
        {
            var database = _factory.GetDatabase(dbName);
            var triggerCollection = database.GetCollection<EffsmsRead>("Trigger");

            var pipeline = triggerCollection.Aggregate()
                // ETAPA 1: Filtrar as triggers de SMS pelo WorkflowId
                .Match(t => t.Parameters.WorkflowId == workflowId)

                // ETAPA 2: Fazer a junção (lookup) com a collection de Leads
                .Lookup<EffsmsRead, LeadDocument, EffsmsRead>(
                    foreignCollection: database.GetCollection<LeadDocument>("Lead"),
                    localField: t => t.Id,
                    foreignField: l => l.TriggerId,
                    @as: (EffsmsRead e) => e.Leads.Items
                )

                // ETAPA 3: Projetar o resultado final, montando a entidade
                .Project(trigger => new EffsmsRead
                {
                    // Mapeia os campos da Trigger de SMS
                    Id = trigger.Id,
                    Status = trigger.Status,
                    Name = trigger.Name,
                    AppointmentDate = trigger.AppointmentDate,
                    StatusTrigger = trigger.StatusTrigger,
                    Parameters = trigger.Parameters,
                    CreatedAt = trigger.CreatedAt,
                    ModifiedAt = trigger.ModifiedAt,
                    TemplateId = trigger.TemplateId,
                    SchedulerId = trigger.SchedulerId,
                    BrokerId = trigger.BrokerId,
                    Transactional = trigger.Transactional,
                    BrokerName = trigger.BrokerName,
                    WebhookEnabled = trigger.WebhookEnabled,
                    WebhookAPIs = trigger.WebhookAPIs,
                    File = trigger.File,
                    Error = trigger.Error,

                    // Monta o objeto 'Leads' com as contagens agregadas
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

            return await pipeline.ToListAsync();
        }
    }
}
