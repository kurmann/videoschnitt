using System.Threading.Tasks;
using Kurmann.Videoschnitt.Features.MetadataProcessor.Events;

namespace Kurmann.Videoschnitt.Features.MetadataProcessor.Handler;

public class MetadataProcessedEventHandler
{
    private readonly MetadataProcessingService _metadataProcessingService;

    public MetadataProcessedEventHandler(MetadataProcessingService metadataProcessingService) => _metadataProcessingService = metadataProcessingService;

    public async Task Handle(MetadataProcessedEvent message)
    {
        // Konkrete Schritte zur Verarbeitung der Metadaten
    }
}
