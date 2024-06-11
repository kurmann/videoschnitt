using Kurmann.Videoschnitt.Messages.Metadata;

namespace Kurmann.Videoschnitt.MetadataProcessor.Handler;

public class MetadataProcessorHandler(MetadataProcessorEngine engine)
{
    private readonly MetadataProcessorEngine _engine = engine;

    public async Task Handle(ProcessMetadataRequest _) => await _engine.StartAsync();
}