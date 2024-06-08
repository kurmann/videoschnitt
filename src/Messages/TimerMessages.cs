namespace Kurmann.Videoschnitt.Messages.Timer;

public record TimerTriggeredEvent(DateTimeOffset TriggeredAt, TimeSpan? Interval = null);