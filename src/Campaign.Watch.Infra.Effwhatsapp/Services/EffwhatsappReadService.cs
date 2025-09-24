using Campaign.Watch.Domain.Entities.Read.Effwhatsapp;
using Campaign.Watch.Domain.Interfaces.Services.Read.Effwhatsapp;
using Campaign.Watch.Infra.Effwhatsapp.Factories;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Campaign.Watch.Infra.Effwhatsapp.Services
{
    public class EffwhatsappReadService : IEffwhatsappReadService
    {
        private readonly IEffwhatsappMongoFactory _factory;

        public EffwhatsappReadService(IEffwhatsappMongoFactory factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<EffwhatsappRead>> GetTriggerEffwhatsapp(string dbName, string workflowId)
        {
            var database = _factory.GetDatabase(dbName);
            var triggerCollection = database.GetCollection<EffwhatsappRead>("Trigger");

            var pipeline = triggerCollection.Aggregate()
                .Match(t => t.Parameters.WorkflowId == workflowId)
                .Lookup<EffwhatsappRead, LeadDocument, EffwhatsappRead>(
                    foreignCollection: database.GetCollection<LeadDocument>("Lead"),
                    localField: t => t.Id,
                    foreignField: l => l.TriggerId,
                    @as: (EffwhatsappRead e) => e.Leads.Items
                )
                .Project(trigger => new EffwhatsappRead
                {
                    // Mapeia os campos da Trigger de WhatsApp
                    Id = trigger.Id,
                    Status = trigger.Status,
                    Name = trigger.Name,
                    Parameters = trigger.Parameters,
                    CreatedAt = trigger.CreatedAt,
                    ModifiedAt = trigger.ModifiedAt,
                    TemplateId = trigger.TemplateId,
                    Archive = trigger.Archive,
                    BrokerType = trigger.BrokerType,
                    Error = trigger.Error,

                    // Monta o objeto 'Leads' com as contagens agregadas
                    Leads = new Leads
                    {
                        TriggerId = trigger.Id.ToString(),
                        Success = trigger.Leads.Items.Count(lead => lead.Status == "Success"),
                        Error = trigger.Leads.Items.Count(lead => lead.Status == "Error"),
                        Blocked = trigger.Leads.Items.Count(lead => lead.Status == "Blocked"),
                        Optout = trigger.Leads.Items.Count(lead => lead.Status == "Optout"),
                        Deduplication = trigger.Leads.Items.Count(lead => lead.Status == "Deduplication")
                    }
                });

            return await pipeline.ToListAsync();
        }
    }
}
