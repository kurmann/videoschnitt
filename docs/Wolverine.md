# Wolverine

Wolverine ist ein leistungsstarkes Messaging-Framework für .NET-Anwendungen, das sowohl In-Memory- als auch verteilte Messaging-Szenarien unterstützt. Es ermöglicht die einfache Integration und Verwaltung von Nachrichtenverarbeitung, Fehlerbehandlung und Nachrichtenrouting.

## Nachrichtentypen und deren Verwendung

### 1. Definieren von Nachrichtentypen

In Wolverine definierst du Nachrichtentypen als einfache Klassen. Diese Typen repräsentieren die Nachrichten, die zwischen verschiedenen Komponenten deiner Anwendung gesendet und empfangen werden.

#### Beispiel für Nachrichtentypen

```csharp
public class ProcessMetadataRequest
{
    public string RequestId { get; set; }
}

public class MetadataProcessedEvent
{
    public string Message { get; }

    public MetadataProcessedEvent(string message)
    {
        Message = message;
    }
}
```

### 2. Senden und Veröffentlichen von Nachrichten

Wolverine bietet verschiedene Methoden zum Senden und Veröffentlichen von Nachrichten:

#### SendAsync

`SendAsync` wird verwendet, um eine Nachricht an genau einen Handler zu senden. Wenn kein Handler registriert ist, wird eine Ausnahme geworfen.

```csharp
public async Task SendImportantCommandAsync(IMessageBus bus)
{
    try
    {
        await bus.SendAsync(new ImportantCommand { CommandId = "12345" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception: {ex.Message}");
    }
}
```

#### PublishAsync

`PublishAsync` wird verwendet, um eine Nachricht an alle interessierten Abonnenten zu senden. Wenn kein Abonnent vorhanden ist, wird die Nachricht einfach ignoriert.

```csharp
public async Task PublishInformationalEventAsync(IMessageBus bus)
{
    await bus.PublishAsync(new InformationalEvent { EventId = "54321" });
}
```

#### InvokeAsync

`InvokeAsync` ermöglicht es dir, eine Nachricht zu senden und auf eine Antwort zu warten. Dies ist besonders nützlich für synchrone Kommunikationsmuster, bei denen du das Ergebnis sofort benötigst.

```csharp
public async Task<MetadataProcessedEvent> ProcessAndReturnAsync(IMessageBus bus)
{
    var response = await _bus.InvokeAsync<MetadataProcessedEvent>(new ProcessMetadataRequest
    {
        RequestId = Guid.NewGuid().ToString()
    });

    return response;
}
```

#### ScheduleAsync

`ScheduleAsync` wird verwendet, um die Ausführung einer Nachricht zu einem späteren Zeitpunkt zu planen.

```csharp
public async Task ScheduleMessageAsync(IMessageBus bus)
{
    await _bus.ScheduleAsync(new ProcessMetadataRequest { RequestId = "12345" }, TimeSpan.FromMinutes(10));
}
```

## Fehlerbehandlung

Wolverine bietet robuste Mechanismen zur Fehlerbehandlung, die konfiguriert werden können, um Wiederholungsversuche, Fehlerwarteschlangen und andere Strategien zu unterstützen.

### Globale Fehlerbehandlungsrichtlinien

Globale Richtlinien gelten für alle Nachrichtenhandler in der Anwendung.

```csharp
var host = await Host.CreateDefaultBuilder(args)
    .UseWolverine(opts =>
    {
        opts.OnException<TimeoutException>().RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());
        opts.OnException<SqlException>().MoveToErrorQueue();
        opts.OnException<InvalidMessageYouWillNeverBeAbleToProcessException>().Discard();
    })
    .StartAsync();
```

### Fehlerbehandlungsrichtlinien pro Nachrichtentyp

Du kannst auch Fehlerbehandlungsrichtlinien für spezifische Nachrichtentypen oder Handler festlegen.

#### Verwendung von Attributen

```csharp
public class MetadataProcessedEventHandler
{
    [RetryWithCooldown(typeof(SqlException), 50, 100, 250)]
    [MoveToErrorQueueOn(typeof(DivideByZeroException))]
    public void Handle(MetadataProcessedEvent message)
    {
        Console.WriteLine($"Metadata processed: {message.Message}");
    }
}
```

## Konfiguration in `Program.cs`

Stelle sicher, dass die Handler und Fehlerbehandlungsrichtlinien in der `Program.cs`-Datei konfiguriert sind.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Hinzufügen von Wolverine
builder.Host.UseWolverine(opts =>
{
    // Fügen Sie so viele andere Assemblys hinzu, wie Sie benötigen
    opts.Discovery.IncludeAssembly(typeof(MetadataProcessingService).Assembly);
});
```

## Fazit

Wolverine bietet eine flexible und leistungsstarke Plattform für das Messaging in .NET-Anwendungen. Durch die Verwendung verschiedener Nachrichtentypen und Fehlerbehandlungsstrategien kannst du robuste und skalierbare Messaging-Lösungen implementieren. Egal ob du synchrone oder asynchrone Kommunikationsmuster benötigst, Wolverine stellt die notwendigen Werkzeuge zur Verfügung.
