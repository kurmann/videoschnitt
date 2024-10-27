# FCP Integrator – Automatisierte Integration von Videos in iCloud Drive und Emby Mediathek

## Inhaltsverzeichnis

1. [Einleitung](#einleitung)
2. [Voraussetzungen](#voraussetzungen)
3. [Installation](#installation)
4. [Konfiguration](#konfiguration)
5. [Verwendung](#verwendung)
6. [Automatisierung mit Apple Automator](#automatisierung-mit-apple-automator)
7. [Fehlerbehebung](#fehlerbehebung)
8. [Lizenz](#lizenz)
9. [Kontakt](#kontakt)

## Einleitung

Der FCP Integrator ist ein Shell-Skript, das den Workflow nach dem Exportieren von Videos aus Final Cut Pro (FCP) automatisiert. Es integriert die exportierten Videos nahtlos in iCloud Drive und die Emby Mediathek, wodurch manuelle Eingriffe minimiert und die Effizienz gesteigert werden.

## Voraussetzungen

Bevor du das Skript nutzen kannst, stelle sicher, dass die folgenden Voraussetzungen erfüllt sind:

- Betriebssystem: macOS
- Python: Version 3.12 oder höher
- Virtuelle Umgebung: venv
- ExifTool: Installiert und im PATH verfügbar
- Homebrew: (optional, zur einfachen Installation von ExifTool)
- Final Cut Pro: Installiert und eingerichtet
- Emby Server: Installiert und konfiguriert

## Installation

### 1. ExifTool installieren

ExifTool ist ein wesentliches Werkzeug zur Extraktion von Metadaten aus Mediendateien. Installiere es über Homebrew:

```sh
brew install exiftool
```

### 2. Projektstruktur einrichten

Erstelle die erforderlichen Verzeichnisse und Dateien für das Skript:

```sh
mkdir -p ~/Scripts
mkdir -p ~/Code/videoschnitt
cd ~/Code/videoschnitt
python3.12 -m venv .venv
source .venv/bin/activate
pip install typer
```

### 3. FCP Integrator Skript erstellen

Erstelle das Shell-Skript `run_fcp_integrator.sh` im Verzeichnis `~/Scripts`:

```sh
nano ~/Scripts/run_fcp_integrator.sh
```

Füge den folgenden Inhalt ein und passe die Pfade entsprechend an:

```sh
#!/bin/bash

# Log-Datei
LOG_FILE="/Users/username/Scripts/fcp_integrator.log"
exec > "$LOG_FILE" 2>&1

echo "Starte FCP Integrator am $(date)"

# PATH anpassen, um exiftool zu finden
export PATH="/opt/homebrew/bin:$PATH"
echo "Aktualisierter PATH: $PATH"

# Pfad zur virtuellen Umgebung
VENV_PATH="/Users/username/Code/videoschnitt/.venv"

# Aktiviert die virtuelle Umgebung
echo "Aktiviere virtuelle Umgebung: $VENV_PATH"
source "$VENV_PATH/bin/activate"

# Überprüfe, ob die virtuelle Umgebung aktiviert wurde
if [ -z "$VIRTUAL_ENV" ]; then
    echo "Fehler: Virtuelle Umgebung konnte nicht aktiviert werden."
    osascript -e 'display notification "Virtuelle Umgebung konnte nicht aktiviert werden." with title "FCP Workflow"'
    exit 1
fi
echo "Virtuelle Umgebung aktiviert."

# Pfade zu den Verzeichnissen
SEARCH_DIR="/Users/username/Movies/Final Cut Export"
ADDITIONAL_DIR="/Users/username/Movies/Compressed Media"
ICLOUD_DIR="/Users/username/Library/Mobile Documents/com~apple~CloudDocs/Familienfilme"
EMBY_DIR="/Volumes/Familienfilme/Test"

echo "Starte FCP-Integrator..."
echo "Suchverzeichnis: $SEARCH_DIR"
echo "Zusätzliches Verzeichnis: $ADDITIONAL_DIR"
echo "iCloud-Verzeichnis: $ICLOUD_DIR"
echo "Emby-Verzeichnis: $EMBY_DIR"

# Führe den FCP-Integrator aus
fcp-integrator "$SEARCH_DIR" --additional-dir "$ADDITIONAL_DIR" "$ICLOUD_DIR" "$EMBY_DIR" --overwrite-existing --force

# Überprüfe den Exit-Status des Befehls
if [ $? -eq 0 ]; then
    echo "Integration abgeschlossen."
    # Sende eine macOS-Systemmeldung bei Erfolg
    osascript -e 'display notification "Integration abgeschlossen." with title "FCP Workflow"'
else
    echo "Integration fehlgeschlagen."
    # Sende eine macOS-Systemmeldung bei Fehler
    osascript -e 'display notification "Integration fehlgeschlagen." with title "FCP Workflow"'
fi

echo "Ende des Skripts am $(date)"
```

**Hinweis:** Ersetze `username` durch deinen tatsächlichen macOS-Benutzernamen und passe die Pfade entsprechend deiner Umgebung an.

### 4. Skript ausführbar machen

Mache das Skript ausführbar:

```sh
chmod +x ~/Scripts/run_fcp_integrator.sh
```

## Konfiguration

Stelle sicher, dass alle Pfade in deinem Skript korrekt sind:

- `VENV_PATH`: Pfad zu deiner virtuellen Umgebung
- `SEARCH_DIR`: Hauptverzeichnis mit exportierten Videos aus Final Cut Pro
- `ADDITIONAL_DIR`: Optionales zusätzliches Verzeichnis mit weiteren Videos
- `ICLOUD_DIR`: Zielverzeichnis in iCloud Drive
- `EMBY_DIR`: Zielverzeichnis in der Emby Mediathek

## Verwendung

### Manuelles Ausführen des Skripts

Führe das Skript direkt aus dem Terminal aus:

```sh
/Users/username/Scripts/run_fcp_integrator.sh
```

**Erwartete Ausgabe:**

- Log-Einträge in `~/Scripts/fcp_integrator.log`
- macOS-Systembenachrichtigung über den Erfolg oder Misserfolg der Integration

## Automatisierung mit Apple Automator

Um das Skript automatisch auszuführen, sobald ein Export in Final Cut Pro abgeschlossen ist, kannst du Apple Automator verwenden. Hier ist eine Anleitung, wie du dies einrichten kannst.

### 1. Automator-Anwendung erstellen

1. **Öffne Automator**:
   - Finde Automator über Spotlight (Cmd + Leertaste und „Automator“ eingeben) oder im Ordner Programme.
2. **Neues Dokument erstellen**:
   - Wähle „Neues Dokument“.
   - Wähle „Anwendung“ und klicke auf „Auswählen“.
3. **“Shell Script ausführen” Aktion hinzufügen**:
   - Suche in der linken Spalte nach „Shell Script ausführen“.
   - Ziehe die Aktion in den rechten Bereich.
4. **Shell Script konfigurieren**:
   - Shell: Wähle `/bin/bash`.
   - Eingabe: Wähle „keine Eingabe“.
   - Shell Script Inhalt: Füge den Pfad zu deinem Skript ein:

   ```
   /Users/username/Scripts/run_fcp_integrator.sh
   ```

### 2. Automator-Anwendung speichern

1. **Speichern**:
   - Gehe zu „Ablage“ > „Sichern“ oder drücke Cmd + S.
   - Gib der Anwendung einen Namen, z.B. „FCP Integrator“.
   - Wähle einen Speicherort, z.B. den Ordner Programme.
   - Klicke auf „Sichern“.

### 3. Automator-Anwendung testen

1. **Führe die Anwendung aus**:
   - Navigiere zu dem Speicherort der Anwendung.
   - Doppelklicke auf „FCP Integrator“.
2. **Überprüfe die Ergebnisse**:
   - Schau dir die Log-Datei an (`~/Scripts/fcp_integrator.log`).
   - Überprüfe die macOS-Systembenachrichtigung.
   - Stelle sicher, dass die Dateien korrekt in iCloud Drive und Emby Mediathek integriert wurden.

### 4. Integration in den Workflow von Final Cut Pro

Da Final Cut Pro keine direkte Möglichkeit bietet, externe Skripte nach einem Export auszuführen, kannst du die Automator-Anwendung manuell starten oder eine Tastenkombination erstellen, um sie schnell zu starten.

#### a. Automator-Anwendung ins Dock ziehen

1. **Platziere die Anwendung im Dock**:
   - Ziehe die gespeicherte Automator-Anwendung („FCP Integrator“) in dein Dock, um schnellen Zugriff zu haben.

#### b. Tastenkombination erstellen (optional)

1. **Systemeinstellungen öffnen**:
   - Gehe zu „Systemeinstellungen“ > „Tastatur“ > „Kurzbefehle“.
2. **Neuen Kurzbefehl hinzufügen**:
   - Wähle „App Shortcuts“ aus der linken Liste.
   - Klicke auf das Pluszeichen (+), um einen neuen Kurzbefehl hinzuzufügen.
   - Anwendung: Wähle „Alle Anwendungen“.
   - Menü-Titel: Gib den genauen Namen deiner Automator-Anwendung ein, z.B. „FCP Integrator“.
   - Tastenkombination: Wähle eine bequeme Kombination, z.B. Cmd + Option + I.
   - Klicke auf „Hinzufügen“.
3. **Verwendung**:
   - Nachdem du einen Export in Final Cut Pro abgeschlossen hast, drücke die festgelegte Tastenkombination, um die Automator-Anwendung schnell zu starten.

## Fehlerbehebung

### 1. ExifTool nicht gefunden

**Fehlermeldung**:

`[Errno 2] No such file or directory: 'exiftool'`

**Lösung**:

- Stelle sicher, dass `exiftool` installiert ist und sich im PATH befindet.
- Aktualisiere den PATH in deinem Shell-Skript, indem du den vollständigen Pfad zu `exiftool` hinzufügst:

  ```sh
  export PATH="/opt/homebrew/bin:$PATH"
  ```

### 2. Virtuelle Umgebung konnte nicht aktiviert werden

**Fehlermeldung**:

`Fehler: Virtuelle Umgebung konnte nicht aktiviert werden.`

**Lösung**:

- Überprüfe den Pfad zur virtuellen Umgebung (`VENV_PATH`) in deinem Skript.
- Stelle sicher, dass die virtuelle Umgebung existiert und der Pfad korrekt ist.

### 3. Berechtigungsprobleme

**Symptome**:

- Skript wird nicht ausgeführt.
- Keine Log-Einträge oder Benachrichtigungen.

**Lösung**:

- Stelle sicher, dass das Shell-Skript ausführbar ist:

  ```sh
  chmod +x /Users/username/Scripts/run_fcp_integrator.sh
  ```

- Überprüfe die Berechtigungen der Automator-Anwendung in den Systemeinstellungen:
  - Gehe zu „Systemeinstellungen“ > „Sicherheit & Datenschutz“ > „Datenschutz“.
  - Stelle sicher, dass Automator und dein Skript Vollzugriff haben.

### 4. Überprüfung der Log-Datei

- Öffne die Log-Datei, um detaillierte Fehlermeldungen zu sehen:

  ```sh
  cat /Users/username/Scripts/fcp_integrator.log
  ```

### 5. Testen von osascript

- Überprüfe, ob `osascript` funktioniert, indem du eine einfache Benachrichtigung ausführst:

  ```sh
  osascript -e 'display notification "Testbenachrichtigung." with title "Test"'
  ```

- Wenn diese Benachrichtigung nicht erscheint, überprüfe die Berechtigungen für Benachrichtigungen in den Systemeinstellungen.

**Hinweis**: Diese Dokumentation verwendet anonyme Beispielpfade (`/Users/username/...`). Ersetze `username` und die Pfade durch die tatsächlichen Pfade in deinem System.
