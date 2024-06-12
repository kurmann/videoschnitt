namespace Kurmann.Videoschnitt.Application.Workflows;

public record WorkflowStatusUpdate(string Message, int Progress, bool IsError = false);