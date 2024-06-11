namespace Kurmann.Videoschnitt.Messages.MediaFiles;

// Event um eine Liste von unterstützten Medien-Dateien zu erhalten, die für die Metadaten-Verarbeitung verwendet werden können
public record SupportedMediaFilesForMetadataProcessingFoundEvent(List<FileInfo> MediaFiles);

// Event um eine Fehlermeldung zu erhalten, wenn die Liste von unterstützten Medien-Dateien nicht abgerufen werden konnte
public record SupportedMediaFilesForMetadataProcessingFoundErrorEvent(string Error);