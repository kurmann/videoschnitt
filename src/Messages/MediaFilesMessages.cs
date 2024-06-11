namespace Kurmann.Videoschnitt.Messages.MediaFiles;

// Event um eine Liste von unterstützten Medien-Dateien zu erhalten, die für die Metadaten-Verarbeitung verwendet werden können
public record MediaFilesForMetadataProcessingFoundEvent(List<FileInfo> MediaFiles);

// Event um eine Fehlermeldung zu erhalten, wenn die Liste von unterstützten Medien-Dateien nicht abgerufen werden konnte
public record MediaFilesForMetadataProcessingFoundErrorEvent(string Error);