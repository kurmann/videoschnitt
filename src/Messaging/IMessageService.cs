namespace Kurmann.Videoschnitt.Messaging;

public interface IMessageService
{
    // Sendet eine Nachricht eines beliebigen Typs.
    Task Publish<TMessage>(TMessage message) where TMessage : IEventMessage;

    // Abonniert eine Nachricht eines beliebigen Typs mit einem Handler.
    void Subscribe<TMessage>(Func<TMessage, Task> handler) where TMessage : IEventMessage;

    // Deabonniert eine Nachricht eines beliebigen Typs mit einem Handler.
    void Unsubscribe<TMessage>(Func<TMessage, Task> handler) where TMessage : IEventMessage;
}