namespace Kurmann.Videoschnitt.Messages.HealthCheck;

public record HealthCheckRequest();

public record HealthCheckResponse(string FFmpegVersionInfo);

public record HealthCheckFailedResponse(string ErrorMessage);