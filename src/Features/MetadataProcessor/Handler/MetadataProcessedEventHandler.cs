using Wolverine;

namespace Kurmann.Videoschnitt.Features.MetadataProcessor.Handler;

public class MetadataProcessedEventHandler
{
    public void Handle(MetadataProcessedEvent message)
    {
        Console.WriteLine($"Metadata processed: {message.Message}");
        // Weitere Logik zur Verarbeitung der Nachricht
    }
}