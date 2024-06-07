using Microsoft.AspNetCore.SignalR;
using Kurmann.Videoschnitt.Messaging;
using Kurmann.Videoschnitt.Messaging.Metadata;
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
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Bei Bedarf Abonnement abmelden
            return Task.CompletedTask;
        }
    }
}