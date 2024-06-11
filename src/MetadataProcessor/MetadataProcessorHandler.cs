using Kurmann.Videoschnitt.Messages.Metadata;

namespace Kurmann.Videoschnitt.MetadataProcessor.Handler;

public class MetadataProcessorHandler(Engine engine)
{
    private readonly Engine _engine = engine;

    public async Task Handle(ProcessMetadataRequest _) => await _engine.StartAsync();
}