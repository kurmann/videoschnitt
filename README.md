# Kurmann Videoschnitt

## Einleitung

**Kurmann Videoschnitt** ist eine umfassende CLI-basierte Anwendung für macOS, die darauf abzielt, die Prozesse des Videoschnitts und der Medienverwaltung zu automatisieren und zu optimieren. Durch eine modulare Architektur, die in verschiedene Pakete mit jeweils eigener CLI unterteilt ist, bietet die Anwendung Flexibilität und Erweiterbarkeit für unterschiedliche Anwendungsfälle im Bereich des Videoschnitts. Neue Pakete werden kontinuierlich hinzugefügt, um den sich entwickelnden Anforderungen gerecht zu werden und zusätzliche Funktionalitäten bereitzustellen.

## Überblick

**Kurmann Videoschnitt** ist eine leistungsstarke CLI-Anwendung für macOS, die sich auf die Automatisierung und Verwaltung von Videoschnittprozessen konzentriert. Sie steigert die Effizienz und Produktivität, indem verschiedene Aufgaben im Videoschnitt automatisiert und optimiert werden. Die Anwendung ist in mehrere spezialisierte Pakete organisiert, die jeweils eigene CLI-Interfaces bieten, wodurch eine klare Trennung der Verantwortlichkeiten und eine einfache Erweiterbarkeit gewährleistet ist. Dies ermöglicht es, gezielt neue Funktionen hinzuzufügen, wenn neue Anwendungsfälle entstehen.

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

Die Grundlage für die CLI ist das [CLI-Design](/docs/CLI-Design.md).

### CLI-Dokumentation

- [Apple Compressor Manager](/docs/cli/apple_compressor_manager.md)
- [Emby Integrator](/docs/cli/emby_integrator.md)
- [Online Medialibrary Manager](/docs/cli/online_medialibrary_manager.md)
- [Original Media Integrator](/docs/cli/original_media_integrator.md)
- [Configuration Manager](/docs/cli/config_manager.md)

## Mitwirken

1. **Issue einreichen**: Bei Fehlern oder Funktionswünschen können GitHub-Issues eröffnet werden.
2. **Pull Requests**: Vorschläge für Änderungen oder Verbesserungen können per Pull Request eingereicht werden.

## Lizenz

Dieses Projekt steht unter der Apache-2.0-Lizenz. Details sind in der Datei [LICENSE](LICENSE) im Repository zu finden.

## Kontakt

Für Fragen oder Unterstützung kann ein Issue im GitHub-Repository erstellt werden.
