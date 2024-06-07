using Kurmann.Videoschnitt.TimerService.Services;
using Kurmann.Videoschnitt.Messages.Timer;

namespace Kurmann.Videoschnitt.TimerService.Handler;

public class StartTimerRequestHandler
{
    private readonly TimerTriggerService _timerTriggerService;

    public StartTimerRequestHandler(TimerTriggerService timerTriggerService) => _timerTriggerService = timerTriggerService;

    public void Handle(StartTimerRequest _)
    {
        // Konkrete Schritte zur Verarbeitung des Timer-Starts
        _timerTriggerService.StartTimer();
    }
}