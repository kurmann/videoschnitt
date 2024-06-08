namespace Kurmann.Videoschnitt.Messages.Metadata;

public record MetadataProcessedEvent(DirectoryInfo InputDirectory, List<FileInfo>? ProcessedFiles = null);