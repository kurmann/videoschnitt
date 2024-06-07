namespace Kurmann.Videoschnitt.Messages.Timer
{
    public record TimerElapsedEvent
    {
        public TimerElapsedEvent(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }

    public record StartTimerRequest();
}