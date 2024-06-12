using Kurmann.Videoschnitt.Messages.Metadata;

namespace Kurmann.Videoschnitt.MetadataProcessor.Handler;

public class MetadataProcessorHandler
{
    private readonly MetadataProcessorEngine _engine;

    public MetadataProcessorHandler(MetadataProcessorEngine engine) => _engine = engine;

    public async Task Handle(ProcessMetadataRequest _) => await _engine.StartAsync();
}