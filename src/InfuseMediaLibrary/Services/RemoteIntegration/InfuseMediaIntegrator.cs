using CSharpFunctionalExtensions;

namespace Kurmann.Videoschnitt.InfuseMediaLibrary.Services.RemoteIntegration;

/// <summary>
/// Verwantwortlich für die Integration von Infuse-Medien aus der lokalen Infuse-Mediathek in die Infuse-Mediathek auf dem Medienserver (Netzwerkspeicher, bspw. NAS)
/// </summary>
internal class InfuseMediaIntegrator
{
    public Task<Result> IntegrateInfuseMediaAsync(LocalInfuseMediaDirectory localInfuseMediaDirectory)
    {
        return Task.FromResult(Result.Success());
    }
}

internal record LocalInfuseMediaDirectory;