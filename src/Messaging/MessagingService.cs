using System.Collections.Concurrent;
using System;
using Microsoft.Extensions.Logging;

namespace Kurmann.Videoschnitt.Messaging;

public interface IEventMessage { }

public abstract class EventMessageBase : IEventMessage
{
    protected Ulid Ulid { get; }

    public string Id => Ulid.ToString();

    public DateTimeOffset Timestamp { get; }

    protected EventMessageBase()
    {
        Ulid = Ulid.NewUlid();
        Timestamp = Ulid.Time.ToLocalTime();
    }
}

public class MessageService : IMessageService
{
    private readonly ConcurrentDictionary<Type, List<Func<IEventMessage, Task>>> _handlers = new();
    private readonly ILogger<MessageService> _logger;

    public MessageService(ILogger<MessageService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Veröffentlicht eine Nachricht an alle abonnierten Handler des Nachrichtentyps.
    /// </summary>
    /// <typeparam name="TMessage">Der Typ der Nachricht, die veröffentlicht wird.</typeparam>
    /// <param name="message">Die Nachricht, die an die Handler gesendet wird.</param>
    /// <returns>Eine Task, die die asynchrone Operation darstellt.</returns>
    public async Task Publish<TMessage>(TMessage message) where TMessage : IEventMessage
    {
        // Ermittelt den Nachrichtentyp des zu veröffentlichenden Ereignisses.
        Type messageType = typeof(TMessage);

        // Versucht, eine Liste von Handlern basierend auf dem Nachrichtentyp zu erhalten.
        if (_handlers.TryGetValue(messageType, out var subscribers))
        {
            // Konvertiert die Liste der Abonnenten in eine neue Liste, um Thread-Sicherheit während der Iteration zu gewährleisten.
            // Dies verhindert Änderungen an der Liste während des Durchlaufs der Handler.
            var safeSubscribers = subscribers?.ToList();
            
            // Überprüft, ob die Liste der Abonnenten nicht null ist, bevor fortfahren wird.
            if (safeSubscribers != null)
            {
                // Loggt Informationen über das Veröffentlichen einer Nachricht.
                _logger.LogInformation("Publishing message of type {messageTypeName} to {safeSubscribersCount} subscribers.", messageType.Name, safeSubscribers.Count);

                // Iteriert durch die sichere Liste von Handlern und ruft jeden Handler auf.
                foreach (var handler in safeSubscribers)
                {
                    try
                    {
                        // Ruft den Handler asynchron auf und wartet auf dessen Fertigstellung.
                        await handler(message).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        // Loggt Fehler, die während der Verarbeitung durch den Handler auftreten.
                        _logger.LogError(ex, "Error during message handling by {HandlerMethod}.", handler.Method.Name);
                    }
                }
            }
        }
        else
        {
            // Loggt eine Warnung, falls für den Nachrichtentyp keine Handler abonniert wurden.
            _logger.LogWarning("No subscribers found for message type {messageType}.", messageType.Name);
        }
    }

    /// <summary>
    /// Abonniert einen Handler für einen spezifischen Nachrichtentyp.
    /// </summary>
    /// <typeparam name="TMessage">Der Typ der Nachricht, für den der Handler abonniert wird.</typeparam>
    /// <param name="handler">Der Handler, der aufgerufen wird, wenn eine Nachricht dieses Typs veröffentlicht wird.</param>
    public void Subscribe<TMessage>(Func<TMessage, Task> handler) where TMessage : IEventMessage
    {
        // Bestimmt den Typ der Nachricht, auf die abonniert wird.
        Type messageType = typeof(TMessage);

        // Erstellt einen allgemeinen Handler, der das typisierte Handler-Funktion umschließt.
        Task genericHandler(IEventMessage message) => handler((TMessage)message);

        // Fügt den Handler der ConcurrentDictionary hinzu oder aktualisiert sie, wenn sie bereits existiert.
        _handlers.AddOrUpdate(messageType,
            // Fügt eine neue Liste mit dem gegebenen Handler hinzu, falls noch kein Eintrag existiert.
            new List<Func<IEventMessage, Task>> { genericHandler },
            // Aktualisiert den bestehenden Eintrag, indem der neue Handler zur Liste hinzugefügt wird.
            (_, existingHandlers) =>
            {
                // Synchronisiert den Zugriff auf die Liste, um Thread-Sicherheit zu gewährleisten.
                lock (existingHandlers)
                {
                    // Überprüft, ob die vorhandene Liste von Handlern nicht null ist.
                    if (existingHandlers == null) // Null-Check hinzugefügt als Absicherung gegen Race-Conditions.
                    {
                        _logger.LogError("ExistingHandlers list was null for message type {messageTypeName}", messageType.Name);
                        // Gibt eine neue Liste zurück, falls existingHandlers aus irgendeinem Grund null ist.
                        return new List<Func<IEventMessage, Task>> { genericHandler };
                    }
                    
                    // Erstellt eine Kopie der vorhandenen Liste, um Manipulationen sicher durchzuführen.
                    var newHandlersList = new List<Func<IEventMessage, Task>>(existingHandlers)
                    {
                        // Fügt den neuen Handler der Kopie hinzu.
                        genericHandler
                    };
                    // Loggt die Hinzufügung des neuen Handlers.
                    _logger.LogInformation("Subscriber {handlerMethodName} added for message type {messageTypeName}.", handler.Method.Name, messageType.Name);
                    // Gibt die aktualisierte Liste zurück.
                    return newHandlersList;
                }
            }
        );
    }

    /// <summary>
    /// Hebt das Abonnement eines Handlers für einen spezifischen Nachrichtentyp auf.
    /// </summary>
    /// <typeparam name="TMessage">Der Typ der Nachricht, für den das Abonnement aufgehoben wird.</typeparam>
    /// <param name="handler">Der Handler, dessen Abonnement aufgehoben wird.</param>
    public void Unsubscribe<TMessage>(Func<TMessage, Task> handler) where TMessage : IEventMessage
    {
        // Bestimmt den Typ der Nachricht, von dem der Handler abgemeldet wird.
        Type messageType = typeof(TMessage);
        // Erstellt einen generischen Handler basierend auf dem spezifischen typisierten Handler.
        Func<IEventMessage, Task> genericHandler = (message) => handler((TMessage)message);

        // Versucht, die Liste der Handler für den Nachrichtentyp abzurufen.
        if (_handlers.TryGetValue(messageType, out var subscribers))
        {
            // Synchronisiert den Zugriff auf die Handlerliste.
            lock (subscribers)
            {
                // Überprüft, ob die subscribers-Liste tatsächlich Handler enthält.
                if (subscribers != null)
                {
                    // Entfernt den Handler aus der Liste und prüft, ob die Entfernung erfolgreich war.
                    bool removed = subscribers.RemoveAll(h => h.Equals(genericHandler)) > 0;

                    // Loggt die Entfernung des Handlers, falls erfolgreich.
                    if (removed)
                    {
                        _logger.LogInformation("Subscriber {handlerMethodName} removed from message type {messageTypeName}.", handler.Method.Name, messageType.Name);
                    }

                    // Entfernt den Eintrag aus dem Dictionary, wenn keine Handler mehr vorhanden sind.
                    if (subscribers.Count == 0)
                    {
                        _handlers.TryRemove(messageType, out _);
                    }
                }
                else
                {
                    // Loggt eine Warnung, falls die Liste der Handler nicht existiert.
                    _logger.LogWarning("Handler list for message type {messageTypeName} was already null on unsubscribe.", messageType.Name);
                }
            }
        }
        else
        {
            // Loggt eine Information, falls für den Nachrichtentyp keine Handler gefunden wurden.
            _logger.LogInformation("No handlers found for message type {messageTypeName} to unsubscribe.", messageType.Name);
        }
    }

}