namespace Kurmann.Videoschnitt.Features.MetadataProcessor.Handler
{
    public class MetadataProcessedEvent
    {
        public MetadataProcessedEvent(string message)
        {
            Message = message;
        }

        public string Message { get; }

        public List<FileInfo> ProcessedFiles { get; } = new();
    }
}