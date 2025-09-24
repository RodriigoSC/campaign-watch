using Campaign.Watch.Domain.Entities.Read.Effpush;
using Campaign.Watch.Domain.Interfaces.Services.Read.Effpush;
using Campaign.Watch.Infra.Effpush.Factories;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Campaign.Watch.Infra.Effpush.Services
{
    public class EffpushReadService : IEffpushReadService
    {
        private readonly IEffpushMongoFactory _factory;

        public EffpushReadService(IEffpushMongoFactory factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<EffpushRead>> GetTriggerEffpush(string dbName, string workflowId)
        {
            var database = _factory.GetDatabase(dbName);
            var triggerCollection = database.GetCollection<EffpushRead>("Trigger");

            var pipeline = triggerCollection.Aggregate()
                // ETAPA 1: Filtrar as triggers de Push pelo WorkflowId
                .Match(t => t.Parameters.WorkflowId == workflowId)

                // ETAPA 2: Fazer a junção (lookup) com a collection de Leads
                .Lookup<EffpushRead, LeadDocument, EffpushRead>(
                    foreignCollection: database.GetCollection<LeadDocument>("Lead"),
                    localField: t => t.Id,
                    foreignField: l => l.TriggerId,
                    @as: (EffpushRead e) => e.Leads.Items
                )

                // ETAPA 3: Projetar o resultado final, montando a entidade
                .Project(trigger => new EffpushRead
                {
                    // Mapeia os campos da Trigger de Push
                    Id = trigger.Id,
                    Status = trigger.Status,
                    Name = trigger.Name,
                    AppointmentDate = trigger.AppointmentDate,
                    StatusTrigger = trigger.StatusTrigger,
                    Parameters = trigger.Parameters,
                    CreatedAt = trigger.CreatedAt,
                    ModifiedAt = trigger.ModifiedAt,
                    TemplateId = trigger.TemplateId,
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
                        Deduplication = 0
                    }
                });

            return await pipeline.ToListAsync();
        }
    }
}
