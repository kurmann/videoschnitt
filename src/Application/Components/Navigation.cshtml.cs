using Microsoft.AspNetCore.Components;
using Kurmann.Videoschnitt.Workflows;
using Kurmann.Videoschnitt.Messages.HealthCheck;
using Wolverine;

namespace YourNamespace.Components;

public partial class Navigation : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; }
    [Inject] private IMessageBus bus { get; set; }
    [Inject] private FinalCutProWorkflow FinalCutProWorkflow { get; set; }

    [Parameter]
    public EventCallback<string> OnLogAdded { get; set; }

    private async Task ExecuteMetadataProcessing()
    {
        var progress = new Progress<string>(async statusUpdate =>
        {
            await OnLogAdded.InvokeAsync(statusUpdate);
        });

        await FinalCutProWorkflow.ExecuteAsync(progress);
    }

    private async Task RequestHealthCheck()
    {
        await bus.SendAsync(new HealthCheckRequest());
    }
}