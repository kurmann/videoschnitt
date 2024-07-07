namespace Kurmann.Videoschnitt.ConfigurationModule.Services;

public interface IConfigurationService
{
    T GetSettings<T>();
}