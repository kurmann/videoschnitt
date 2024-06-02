namespace Kurmann.Videoschnitt.Messaging.Metadata
{
    public class MetadataProcessedEvent : EventMessageBase
    {
        public MetadataProcessedEvent(string message)
        {
            Message = message;
        }

        public string Message { get; }

        public List<FileInfo> ProcessedFiles { get; } = new();
    }
}