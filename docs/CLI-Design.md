## CLI-Tool-Konzept: `kurmann-videoschnitt`

### Übersicht

Das `kurmann-videoschnitt` CLI-Tool ist ein flexibles und erweiterbares Werkzeug zur Verwaltung und Verarbeitung von Videos. Es ist modular aufgebaut und ermöglicht durch Subkommandos die einfache Erweiterung und Organisation von Funktionen.

### Struktur und Hierarchie

Das CLI-Tool folgt einer klar definierten Hierarchie:

1. **Hauptkommando (`kurmann-videoschnitt`)**: Dies ist der Einstiegspunkt für alle Befehle. Es dient als zentraler Hub, von dem aus alle Subkommandos aufgerufen werden können.

2. **Subkommandos**: Diese gruppieren verwandte Kommandos logisch zusammen. Beispiele:
   - **`compressor`**: Kommandos für den Apple Compressor Manager, wie `compress-prores` und `cleanup-prores`.
   - **`original-media-manager`**: Kommandos für die Verwaltung von Originalmedien, wie `integrate-new-media`.

3. **Kommandos**: Die eigentlichen Aktionen, die vom Benutzer ausgeführt werden. Diese können spezifische Parameter und Optionen haben.

Beispiel:

```bash
kurmann-videoschnitt compressor compress-prores --output-dir /path/to/output
```

### Interaktiver Modus

Das `kurmann-videoschnitt` CLI-Tool unterstützt **zukünftig** einen **interaktiven Modus**, in dem Benutzer Schritt für Schritt durch die Eingabe der notwendigen Parameter geführt werden. Dies ist besonders nützlich, wenn Benutzer die Parameter nicht auswendig kennen oder wenn komplexe Eingaben erforderlich sind.

- **Einfache Interaktivität mit `Click.prompt`**: Der interaktive Modus wird durch die Verwendung von `Click.prompt` umgesetzt, um Benutzereingaben abzufragen. Diese Methode ist einfach und direkt in das `Click`-Framework integriert.

- **Andere Optionen**: Für erweiterte interaktive Erfahrungen könnten theoretisch auch spezialisierte Bibliotheken wie `questionary` oder `InquirerPy` verwendet werden. In diesem Projekt wird jedoch ausschließlich `Click.prompt` verwendet, um den interaktiven Modus umzusetzen.

Beispiel für den Aufruf im interaktiven Modus:

```bash
kurmann-videoschnitt compressor compress-prores --interactive
```

In diesem Modus werden Benutzer durch Eingabeaufforderungen geleitet, um alle erforderlichen Informationen für die Ausführung des Kommandos einzugeben.

### Konfigurationsmanagement

Das CLI-Tool unterstützt eine Konfigurationsdatei im TOML-Format, die Standardwerte für Kommandos definiert. Diese Konfigurationsdatei wird standardmäßig im **Application Support-Verzeichnis** unter `~/Library/Application Support/Kurmann/Videoschnitt/config.toml` gespeichert. 

Die Konfigurationsdatei kann:

- **Standardmäßig geladen werden**: Das CLI lädt die `config.toml`-Datei aus dem Standardverzeichnis.
- **Überschrieben werden**: Der Pfad zu einer alternativen Konfigurationsdatei kann mit `--config` angegeben werden.
- **Dynamisch durch Parameter überschrieben werden**: Kommandospezifische Optionen, die auf der Kommandozeile angegeben werden, überschreiben die in der Konfigurationsdatei festgelegten Werte.

Beispiel:

```bash
kurmann-videoschnitt --config /pfad/zur/anderen/config.toml compressor compress-prores --output-dir /pfad/zum/anderen/output
```

### Dokumentation

Die Dokumentation des CLI-Tools wird größtenteils automatisch aus den `Click`-Dekoratoren und Docstrings generiert:

1. **Automatische Hilfe-Dokumentation**: `Click` generiert automatisch kontextbezogene Hilfeseiten für das Hauptkommando, Subkommandos und individuelle Kommandos.

2. **Markdown-Dokumentation**: Ein Python-Skript generiert aus den `Click`-Dekoratoren und Docstrings Markdown-Dateien, die für die offizielle Dokumentation verwendet werden können. Dies ermöglicht eine nahtlose Integration in GitHub oder andere Plattformen, die Markdown unterstützen.

3. **Automatische Aktualisierung der README.md**: Das gleiche Skript kann genutzt werden, um ein spezifisches Kapitel in der `README.md` automatisch zu aktualisieren. Dies stellt sicher, dass die Dokumentation immer auf dem neuesten Stand ist.

Beispiel für einen automatisierten Task in VSCode:

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "Generate CLI Documentation",
      "type": "shell",
      "command": "python3",
      "args": [
        "scripts/generate_cli_docs.py"
      ],
      "group": {
        "kind": "build",
        "isDefault": true
      },
      "problemMatcher": []
    },
    {
      "label": "Publish Project",
      "type": "shell",
      "command": "Publish",
      "dependsOn": ["Generate CLI Documentation"],
      "problemMatcher": []
    }
  ]
}
```
