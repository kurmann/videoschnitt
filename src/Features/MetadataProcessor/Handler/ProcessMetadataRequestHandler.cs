using System.Threading.Tasks;
using Kurmann.Videoschnitt.Features.MetadataProcessor.Events;

namespace Kurmann.Videoschnitt.Features.MetadataProcessor.Handler;

public class ProcessMetadataRequestHandler
{
    private readonly MetadataProcessingService _metadataProcessingService;

    public ProcessMetadataRequestHandler(MetadataProcessingService metadataProcessingService) => _metadataProcessingService = metadataProcessingService;

    public async Task Handle(ProcessMetadataRequest message)
    {
        // Konkrete Schritte zur Verarbeitung der Metadaten
        await _metadataProcessingService.ProcessMetadataAsync();
    }
}