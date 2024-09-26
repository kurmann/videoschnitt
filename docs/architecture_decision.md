
# Entscheidung für die Architektur meines Videoschnitt-Workflows

Patrick Kurmann, 19. Aug. 2024

## Überblick

In meinem Videoschnitt-Workflow spielen sowohl macOS als auch ein Synology NAS eine zentrale Rolle. Nach intensiven Überlegungen und Tests habe ich mich entschieden, macOS als zentrale Steuereinheit für den gesamten Workflow zu verwenden und das NAS primär als Speicher- und Archivlösung zu nutzen. In diesem Dokument wird die Entscheidungsfindung sowie die technische Herangehensweise an diese hybride Architektur erläutert.

## Ausgangslage

Mein Workflow umfasst verschiedene Phasen der Medienverarbeitung:

1. **Videoproduktion und -schnitt**: Der Hauptteil der Videobearbeitung findet auf einem Mac Mini statt, auf dem Final Cut Pro und andere Tools wie Apple Compressor eingesetzt werden.
2. **Integration mit iCloud**: Da meine Medien auch über iCloud synchronisiert werden müssen, ist eine enge Anbindung an macOS notwendig.
3. **Speicherung und Archivierung**: Das Synology NAS dient als zentrales Speichersystem und verwaltet die Langzeitarchivierung der Medien sowie deren Struktur.
4. **Automatisierung und Trigger**: Verschiedene Automatisierungen (z.B. zur Metadatenverwaltung, zur NFO-Erstellung oder zur Archivierung) werden durch Python-Skripte gesteuert.

## Evaluierte Optionen

### 1. **Docker-Container auf dem Synology NAS für Automatisierung**

Ein initialer Ansatz war es, die Automatisierung vollständig über Docker-Container auf dem NAS laufen zu lassen. Dies hätte eine klare Trennung der Aufgaben ermöglicht und wäre sehr portabel gewesen. Allerdings ergaben sich folgende Herausforderungen:

- **Komplexität**: Für einige Aufgaben ist es notwendig, diese auf macOS durchzuführen, da dort der eigentliche Videoschnitt stattfindet. Ein vollständiger Wechsel zu einer Docker-basierten Lösung auf dem NAS hätte zu einer fragmentierten Arbeitsumgebung geführt.
- **Integration mit iCloud**: Die enge Integration mit iCloud ist entscheidend, da Medien teilweise automatisch synchronisiert und weiterverarbeitet werden müssen. Diese Integration ist auf dem NAS nicht nativ möglich.

### 2. **Hybrider Ansatz: macOS als zentrale Steuereinheit, NAS als Speicher**

Dieser Ansatz kombiniert die Stärken beider Plattformen und ermöglicht eine flexible, skalierbare und wartbare Lösung:

- **macOS als zentrale Steuereinheit**: Der Mac Mini dient als Hauptplattform für die Automatisierung, inklusive Videoschnitt, iCloud-Synchronisierung und Trigger von Verarbeitungsprozessen.
- **Verzeichnisüberwachung mit `launchd`**: Erste Tests haben gezeigt, dass `launchd` zuverlässig Änderungen in gemounteten NAS-Verzeichnissen erkennt und entsprechend Trigger auslösen kann. Dies ersetzt die Notwendigkeit von `watchdog` oder eines separaten Steuerungsskripts.
- **NAS als Speicherlösung**: Das NAS bleibt der zentrale Speicherort für Originalmedien und Archive. Automatisierungen wie die Erstellung von Playlists oder die Metadatenverwaltung können weiterhin über Docker-Container auf dem NAS erfolgen.

### 3. **macOS als zentraler Server, NAS als reine Speicherlösung**

Ein weiterer Ansatz wäre, das NAS lediglich als Speicherlösung zu verwenden und alle Automatisierungen ausschließlich auf dem Mac zu belassen. Hierbei wäre `launchd` für die gesamte Steuerung zuständig. Der Vorteil liegt in der klaren, zentralisierten Umgebung auf macOS. Allerdings würde dies die Last und Abhängigkeit vom Mac erhöhen, was die Skalierbarkeit einschränkt.

## Entscheidungsfindung

Nach einer Abwägung der Optionen habe ich mich für die zweite Variante entschieden: **macOS als zentrale Steuereinheit und das NAS als Speicherlösung**.

### Begründung

- **Integration und Kompatibilität**: Da der Videoschnitt und die iCloud-Integration auf macOS stattfinden, bleibt die Arbeitsumgebung konsistent. Die Automatisierung wird direkt in das bestehende Setup integriert.
- **Flexibilität durch `launchd`**: `launchd` ist eine native macOS-Lösung und ermöglicht die Überwachung von Änderungen in NAS-Verzeichnissen. Dies reduziert die Komplexität, da keine zusätzlichen Tools wie `watchdog` benötigt werden.
- **Skalierbarkeit und Portabilität**: Docker-Container auf dem NAS bleiben weiterhin eine Option für spezifische Automatisierungen, die unabhängig vom Videoschnitt sind. Dies ermöglicht eine flexible und modulare Erweiterung des Workflows.
- **Wartbarkeit und Debugging**: macOS bietet eine zentrale Kontrolle über alle Prozesse. Da alle Logs und Automatisierungen auf einer Plattform verwaltet werden, wird das Debugging und die Wartung vereinfacht.

## Nächste Schritte

1. **Weiterführende Tests mit `launchd` und gemounteten NAS-Verzeichnissen**: Ich werde sicherstellen, dass die Verzeichnisüberwachung unter realen Bedingungen stabil bleibt.
2. **Implementierung der Automatisierungen auf macOS**: Python-Skripte und `launchd`-Konfigurationen werden schrittweise implementiert, um eine nahtlose Integration in den bestehenden Workflow zu gewährleisten.
3. **Langzeitarchivierung und Automatisierung auf dem NAS**: Für Aufgaben wie die Metadatenverwaltung und das Erstellen von Playlists werde ich Docker-Container auf dem NAS einrichten, die unabhängig vom Mac laufen können.

## Fazit

Dieser hybride Ansatz vereint die Vorteile beider Systeme und bietet eine flexible, skalierbare Lösung für meinen Videoschnitt-Workflow. Durch die zentrale Steuerung auf macOS bleiben die Arbeitsprozesse konsistent, während das NAS als zuverlässige Speicher- und Archivlösung dient.

---

Mit dieser Architektur habe ich eine zukunftssichere Lösung, die sich an neue Anforderungen anpassen lässt und gleichzeitig die Komplexität reduziert.
