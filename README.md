# Kurmann Videoschnitt

## Einleitung

**Kurmann Videoschnitt** ist eine umfassende CLI-basierte Anwendung für macOS, die entwickelt wurde, um die Prozesse des Videoschnitts und der Medienverwaltung zu automatisieren und zu optimieren. Durch eine modulare Architektur, die in verschiedene Pakete mit jeweils eigener CLI unterteilt ist, bietet die Anwendung Flexibilität und Erweiterbarkeit für unterschiedliche Anwendungsfälle im Bereich des Videoschnitts. Die Anwendung steigert die Effizienz und Produktivität, indem sie verschiedene Aufgaben im Videoschnitt automatisiert und optimiert. Neue Pakete werden kontinuierlich hinzugefügt, um den sich entwickelnden Anforderungen gerecht zu werden und zusätzliche Funktionalitäten bereitzustellen.

## Hauptfunktionen

Die Anwendung bietet eine Vielzahl von Funktionen, die speziell für den Videoschnitt und die Medienverwaltung entwickelt wurden:

- **Verwaltung Originalmedien**: Organisiert und katalogisiert Originalvideos.
- **Automatische Komprimierung**: Reduziert die Dateigröße von Videos und spart Speicherplatz, ohne signifikanten Qualitätsverlust.
- **Metadaten-Synchronisation**: Aktualisiert und pflegt Videometadaten.
- **Bereinigungsfunktionen**: Automatische Entfernung doppelter oder unnötiger Dateien.
- **Medienbereitstellung**: Unterstützt verschiedene Bereitstellungsvarianten, wie lokale Medienbibliotheken und Cloud-Streaming.

## Architektur und Erweiterbarkeit

**Kurmann Videoschnitt** ist in verschiedene Pakete unterteilt, von denen jedes eine eigene CLI bietet. Diese Struktur ermöglicht es, einzelne Komponenten unabhängig voneinander zu entwickeln, zu warten und zu erweitern. Neue Pakete können hinzugefügt werden, wenn neue Anwendungsfälle entstehen, wodurch die Anwendung stetig ausgebaut und an die Bedürfnisse der Benutzer angepasst wird.

## CLI

- [Apple Compressor Manager](/docs/cli/apple_compressor_manager.md)
- [Emby Integrator](/docs/cli/emby_integrator.md)
- [Metdaten-Manager](docs/cli/metadata_manager.md)
- [Online Medialibrary Manager](/docs/cli/online_medialibrary_manager.md)
- [Original Media Integrator](/docs/cli/original_media_integrator.md)
- [Configuration Manager](/docs/cli/config_manager.md)

Die Grundlage für die CLI ist das [CLI-Design](/docs/CLI-Design.md).

## Mitwirken

Dieses Projekt wird von mir privat in meiner Freizeit in der Schweiz entwickelt. Ein aktives Mitwirken von ausserhalb ist derzeit nicht vorgesehen. Falls du jedoch Interesse daran hast, die CLI-Anwendung weiter auszubauen oder Verbesserungsvorschläge hast, kannst du gerne ein Issue im GitHub-Repository eröffnen. Bug-Reports sind ebenfalls herzlich willkommen und helfen dabei, die Anwendung weiter zu optimieren.

## Lizenz

Dieses Projekt steht unter der Apache-2.0-Lizenz. Details sind in der Datei [LICENSE](LICENSE) im Repository zu finden.
