using Microsoft.EntityFrameworkCore;
using OmniWeigh.Core.Data;
using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services
{
    public class DocumentSequenceService : IDocumentSequenceService
    {
        private readonly OmniDbContext _dbContext;

        public DocumentSequenceService(OmniDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateDocumentNumberAsync(DocumentType type)
        {
            string entityType = type.ToString();
            string prefix = type switch
            {
                DocumentType.BonDeLivraison => "BL",
                DocumentType.BonDeSortie => "BS",
                DocumentType.BonDeReception => "BR",
                DocumentType.BonDeRetour => "BT",
                DocumentType.Facture => "FA",
                _ => "DOC"
            };

            var tracker = await _dbContext.SequenceTrackers
                .FirstOrDefaultAsync(t => t.EntityType == entityType);

            if (tracker == null)
            {
                tracker = new SequenceTracker
                {
                    EntityType = entityType,
                    NextValue = 1,
                    Prefix = prefix
                };
                _dbContext.SequenceTrackers.Add(tracker);
            }

            int nextVal = tracker.NextValue;
            tracker.NextValue++;

            await _dbContext.SaveChangesAsync();

            // Format: BL-000000000001
            return $"{tracker.Prefix}-{nextVal:D12}";
        }
    }
}
