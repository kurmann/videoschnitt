## CLI-Tool-Konzept: `kurmann-videoschnitt`

### Übersicht

Das `kurmann-videoschnitt` CLI-Tool ist ein flexibles und erweiterbares Werkzeug zur Verwaltung und Verarbeitung von Videos. Es ist modular aufgebaut und ermöglicht durch Subkommandos die einfache Erweiterung und Organisation von Funktionen. 

### Struktur und Hierarchie

Das CLI-Tool folgt einer klar definierten Hierarchie:

1. **Hauptkommando (`kurmann-videoschnitt`)**: Dies ist der Einstiegspunkt für alle Befehle. Es dient als zentraler Hub, von dem aus alle Subkommandos aufgerufen werden können.

2. **Subkommandos**: Diese gruppieren verwandte Kommandos logisch zusammen. Beispiele:
   - **`compressor`**: Kommandos für den Apple Compressor Manager, wie `compress-prores` und `cleanup-prores`.
   - **`integrator`**: Kommandos für den Original Media Integrator, wie `integrate`.

3. **Kommandos**: Die eigentlichen Aktionen, die vom Benutzer ausgeführt werden. Diese können spezifische Parameter und Optionen haben.

Beispiel:

```bash
kurmann-videoschnitt compressor compress-prores --output-dir /path/to/output
```

### Konfigurationsmanagement

Das CLI-Tool unterstützt eine Konfigurationsdatei im TOML-Format, die Standardwerte für Kommandos definiert. Diese Konfigurationsdatei kann:

- **Standardmäßig geladen werden**: Das CLI sucht nach einer `kurmann_videoschnitt_config.toml` im aktuellen Verzeichnis und verwendet diese als Basis.
- **Überschrieben werden**: Der Pfad zu einer alternativen Konfigurationsdatei kann mit `--config` angegeben werden.
- **Dynamisch durch Parameter überschrieben werden**: Kommandospezifische Optionen, die auf der Kommandozeile angegeben werden, überschreiben die in der Konfigurationsdatei festgelegten Werte.

Beispiel:

```bash
kurmann-videoschnitt --config /pfad/zur/config.toml compressor compress-prores --output-dir /pfad/zum/anderen/output
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
      "command": "your-publish-command-here",
      "dependsOn": ["Generate CLI Documentation"],
      "problemMatcher": []
    }
  ]
}
```

### Erweiterbarkeit

Das Tool ist darauf ausgelegt, leicht erweiterbar zu sein. Neue Subkommandos und Kommandos können einfach hinzugefügt werden, indem neue Module erstellt und in die bestehende CLI-Struktur integriert werden. Dies ermöglicht es, das Tool nach Bedarf zu skalieren und neue Funktionen hinzuzufügen, ohne die bestehende Struktur zu beeinträchtigen.

### Qualitätssicherung

- **Unit-Tests**: Tests für jedes Kommando und Subkommando gewährleisten die Korrektheit und Stabilität des Tools.
- **Integrationstests**: Tests des gesamten Workflows stellen sicher, dass alle Komponenten zusammenarbeiten und die Konfigurationsdatei korrekt verarbeitet wird.

### Versionsverwaltung und Veröffentlichung

- **Semantische Versionierung**: Das Tool verwendet eine semantische Versionierung, um Änderungen klar zu kommunizieren.
- **Veröffentlichung auf PyPI**: Das Tool kann auf PyPI veröffentlicht werden, um die Installation und Verbreitung zu erleichtern.
- **Automatische Changelog-Erstellung**: Ein Changelog wird bei jedem Release automatisch erstellt, basierend auf den Git-Commits.

### Fazit

Das `kurmann-videoschnitt` CLI-Tool ist ein durchdachtes, modulares und benutzerfreundliches Werkzeug, das durch klare Hierarchien, flexible Konfigurationsmöglichkeiten und eine robuste Dokumentation überzeugt. Es ist leicht erweiterbar und kann nahtlos in verschiedene Entwicklungsumgebungen integriert werden.
