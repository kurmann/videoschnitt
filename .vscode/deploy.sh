#!/bin/bash

# Prüfen, ob der Pfad übergeben wurde
if [ -z "$1" ]; then
  echo "Bitte den Pfad zum Projekt als Parameter übergeben."
  exit 1
fi

# Pfad zum Projekt
PROJECT_PATH=$1

# Benutzername aus Umgebungsvariable lesen
USER_NAME=$USER

# Verzeichnis für die Anwendung
APP_DIR="/Users/${USER_NAME}/bin/Kurmann/Videoschnitt"

# Lösche und erstelle das Verzeichnis neu für die Anwendung
rm -rf "$APP_DIR"
mkdir -p "$APP_DIR"

# Prozess finden, der Port 5024 verwendet und ihn beenden
PID=$(lsof -t -i:5024)
if [ -n "$PID" ]; then
  echo "Beende den Prozess, der Port 5024 verwendet (PID: $PID)..."
  kill -9 $PID
  echo "Prozess beendet."
else
  echo "Kein Prozess verwendet Port 5024."
fi

# Anwendung veröffentlichen ohne Single-File und Debug-Dateien
echo "Veröffentliche die .NET-Anwendung..."
dotnet publish "$PROJECT_PATH/src/Application/Application.csproj" -c Release --self-contained -p:DebugType=None -o "$APP_DIR"

if [ $? -eq 0 ]; then
  echo "Veröffentlichung erfolgreich. Die Anwendung wurde nach $APP_DIR deployed."
else
  echo "Fehler bei der Veröffentlichung."
  exit 1
fi

# launchctl Dienst neu laden
echo "Neuladen des LaunchAgent-Dienstes..."
launchctl unload "$HOME/Library/LaunchAgents/com.swiss.kurmann.videoschnitt.plist"
launchctl load "$HOME/Library/LaunchAgents/com.swiss.kurmann.videoschnitt.plist"

if [ $? -eq 0 ]; then
  echo "LaunchAgent-Dienst erfolgreich neu geladen."
  echo "Die Anwendung läuft jetzt unter $APP_DIR"
else
  echo "Fehler beim Neuladen des LaunchAgent-Dienstes."
  exit 1
fi