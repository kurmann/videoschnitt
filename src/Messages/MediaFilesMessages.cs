using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kurmann.Videoschnitt.Messages.MediaFiles;

// Event um eine Liste von unterstützten Medien-Dateien zu erhalten, die für die Metadaten-Verarbeitung verwendet werden können
public class SupportedMediaFilesForMetadataProcessingEvent
{
    public List<FileInfo> MediaFiles { get; }

    public SupportedMediaFilesForMetadataProcessingEvent(IEnumerable<FileInfo> mediaFiles)
    {
        MediaFiles = mediaFiles.ToList();
    }
}