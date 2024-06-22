#!/bin/bash

# Prüfen, ob der Pfad übergeben wurde
if [ -z "$1" ]; then
  echo "Bitte den Pfad zum Projekt als Parameter übergeben."
  exit 1
fi

# Pfad zum Projekt
PROJECT_PATH=$1

# Verzeichnis für die Anwendung
APP_DIR="/usr/local/kurmann-videoschnitt"

# Erklärung für das Admin-Passwort
echo "Das Admin-Passwort wird benötigt, um die Anwendung in ein systemweites Verzeichnis zu installieren und den LaunchDaemon-Dienst neu zu starten."

# Erstelle das Verzeichnis, falls es nicht existiert
sudo mkdir -p $APP_DIR/Application

# Anwendung veröffentlichen
echo "Veröffentliche die .NET-Anwendung als Single-File..."
sudo dotnet publish $PROJECT_PATH/src/Application/Application.csproj -c Release -o $APP_DIR/Application

if [ $? -eq 0 ]; then
  echo "Veröffentlichung erfolgreich. Die Anwendung wurde nach $APP_DIR/Application deployed."
else
  echo "Fehler bei der Veröffentlichung."
  exit 1
fi

# launchd Dienst neu laden
echo "Neuladen des LaunchDaemon-Dienstes..."
sudo launchctl unload /Library/LaunchDaemons/com.swiss.kurmann.videoschnitt.plist
sudo launchctl load /Library/LaunchDaemons/com.swiss.kurmann.videoschnitt.plist

if [ $? -eq 0 ]; then
  echo "LaunchDaemon-Dienst erfolgreich neu geladen."
  echo "Die Anwendung läuft jetzt unter $APP_DIR/Application"
else
  echo "Fehler beim Neuladen des LaunchDaemon-Dienstes."
  exit 1
fi