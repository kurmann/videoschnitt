using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Kurmann.Videoschnitt.Messaging;
using Kurmann.Videoschnitt.Messaging.MetadataProcessor;
using Kurmann.Videoschnitt.Messaging.Timer;

namespace Kurmann.Videoschnitt.Application
{
    public class MessageLogHubService : IHostedService
    {
        private readonly IMessageService _messageService;
        private readonly IHubContext<LogHub> _hubContext;
        private readonly ILogger<MessageLogHubService> _logger;

        public MessageLogHubService(IMessageService messageService, IHubContext<LogHub> hubContext, ILogger<MessageLogHubService> logger)
        {
            _messageService = messageService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Abonniere die gewünschten Nachrichten
            _messageService.Subscribe<StartTimerRequest>(HandleStartTimerRequest);
            _messageService.Subscribe<StopTimerRequest>(HandleStopTimerRequest);
            _messageService.Subscribe<ProcessMetadataRequest>(HandleProcessMetadataRequest);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Bei Bedarf Abonnement abmelden
            _messageService.Unsubscribe<StartTimerRequest>(HandleStartTimerRequest);
            _messageService.Unsubscribe<StopTimerRequest>(HandleStopTimerRequest);
            _messageService.Unsubscribe<ProcessMetadataRequest>(HandleProcessMetadataRequest);
            return Task.CompletedTask;
        }

        private async Task HandleStartTimerRequest(StartTimerRequest request)
        {
            // Nachricht an den LogHub senden
            await _hubContext.Clients.All.SendAsync("ReceiveLogMessage", "Anfrage um Timer zu starten: " + request.Timestamp);
        }

        private async Task HandleStopTimerRequest(StopTimerRequest request)
        {
            // Nachricht an den LogHub senden
            await _hubContext.Clients.All.SendAsync("ReceiveLogMessage", "Anfrage um Timer zu stoppen: " + request.Timestamp);
        }

        private async Task HandleProcessMetadataRequest(ProcessMetadataRequest request)
        {
            // Nachricht an den LogHub senden
            await _hubContext.Clients.All.SendAsync("ReceiveLogMessage", "Anfrage für Metadatenverarbeitung: " + request.Timestamp);
        }
    }
}