using Kurmann.Videoschnitt.Features.MetadataProcessor.Events;
using Wolverine;

namespace Kurmann.Videoschnitt.Features.MetadataProcessor.Handler;

public class MetadataProcessedEventHandler
{
    public void Handle(MetadataProcessedEvent message)
    {
        Console.WriteLine($"Metadata processed: {message.Message}");
        // Weitere Logik zur Verarbeitung der Nachricht
    }

    public void Handle(ProcessMetadataRequest request)
    {
        Console.WriteLine("Processing metadata...");
        // Weitere Logik zur Verarbeitung der Nachricht
    } 
}