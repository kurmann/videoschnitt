# Docker-basierte Steuerung und Logging auf einem Synology NAS

In diesem Kapitel wird beschrieben, wie du Python-Skripte und andere Prozesse auf einem Synology NAS innerhalb von Docker-Containern effizient steuern und überwachen kannst. Dabei wird besonders auf folgende Aspekte eingegangen:

- **Trigger von außen** (z.B. Webhooks)
- **Zeitliche Steuerung von Prozessen** (z.B. Cron-basierte Tasks)
- **Logging und Zugriff auf Logs**

## Inhaltsverzeichnis

1. [Trigger von außen: Webhooks und HTTP Requests](#trigger-von-außen-webhooks-und-http-requests)
2. [Zeitliche Steuerung: Automatisierung mit Docker und Cron](#zeitliche-steuerung-automatisierung-mit-docker-und-cron)
3. [Logging: Docker-native Logs und Zugriff über SSH](#logging-docker-native-logs-und-zugriff-über-ssh)

## Trigger von außen: Webhooks und HTTP Requests

Wenn du einen Prozess innerhalb eines Docker-Containers auf deinem Synology NAS starten möchtest, ohne direkten Zugriff per SSH, kannst du HTTP-basierte Trigger (z.B. Webhooks) verwenden. Dies ist besonders nützlich, wenn du einen Dienst wie Flask oder FastAPI nutzt, um einfache API-Endpunkte zu erstellen, die deine Skripte ausführen.

### Implementierung eines einfachen Flask-Servers:

```python
from flask import Flask, request
import subprocess

app = Flask(__name__)

@app.route('/trigger-script', methods=['POST'])
def run_script():
    # Hier das Python-Skript ausführen
    subprocess.call(["python", "/usr/local/bin/your_script.py"])
    return "Script executed!", 200

if __name__ == "__main__":
    app.run(host='0.0.0.0', port=5000)
```

Dieser Flask-Server empfängt einen `POST`-Request auf `/trigger-script` und führt das Python-Skript aus. Dies kann durch einen einfachen `curl`-Befehl oder einen Webhook ausgelöst werden:

```bash
curl -X POST http://synology-ip:5000/trigger-script
```

### Vorteile:
- **Einfacher Aufbau:** Keine zusätzliche Software nötig, nur Flask oder FastAPI.
- **Zugriff über das Netzwerk:** Ideal für Remote-Zugriffe ohne SSH.

## Zeitliche Steuerung: Automatisierung mit Docker und Cron

Wenn du regelmäßig laufende Aufgaben in einem Docker-Container auf deinem Synology NAS planen möchtest, kannst du dies mit `cron` direkt im Container tun. Dadurch bleibt dein Setup einfach und vollständig innerhalb von Docker.

### Dockerfile-Beispiel mit Cron:

```Dockerfile
FROM python:3.11

# Installiere cron
RUN apt-get update && apt-get install -y cron

# Kopiere dein Python-Skript in das Image
COPY your_script.py /usr/local/bin/your_script.py
RUN chmod +x /usr/local/bin/your_script.py

# Kopiere die Crontab-Konfiguration
COPY crontab /etc/cron.d/my-cron-job

# Setze die richtigen Rechte für die Crontab-Datei
RUN chmod 0644 /etc/cron.d/my-cron-job

# Starte den cron-Dienst im Vordergrund
CMD ["cron", "-f"]
```

### Beispiel für eine Crontab-Konfiguration:

```bash
# Crontab-Datei unter /etc/cron.d/my-cron-job
0 0 * * * root python /usr/local/bin/your_script.py >> /var/log/cron.log 2>&1
```

Diese Konfiguration führt das Python-Skript täglich um Mitternacht aus und schreibt die Logs in eine Datei.

### Docker-Compose-Beispiel:

```yaml
version: '3.8'

services:
  cron-service:
    build: .
    container_name: cron-container
    restart: always
    volumes:
      - /path/on/nas/logs:/var/log/my-app
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "5"
```

### Vorteile:
- **Komplett Docker-basiert:** Keine Abhängigkeiten von externen Tools wie dem Synology Task Scheduler.
- **Zuverlässig:** Docker sorgt dafür, dass der Container immer läuft und die Cron-Jobs ausgeführt werden.

## Logging: Docker-native Logs und Zugriff über SSH

Eine effiziente Log-Verwaltung ist entscheidend, besonders wenn mehrere Skripte in einem Container laufen. Docker bietet eingebaute Logging-Funktionen, die flexibel und einfach zu konfigurieren sind.

### Docker-native Logging:

In der `docker-compose.yml` kannst du das Logging wie folgt konfigurieren:

```yaml
logging:
  driver: "json-file"
  options:
    max-size: "10m"
    max-file: "5"
```

Diese Konfiguration sorgt dafür, dass die Logs automatisch rotiert werden, wenn sie eine bestimmte Größe erreichen, und hält die letzten fünf Log-Dateien vor.

### Zugriff auf Logs über die Docker CLI:

Um die Logs einzusehen, kannst du einfach den folgenden Befehl verwenden:

```bash
docker logs <container_name>
```

- **Echtzeit-Logs:** Mit `-f` kannst du die Logs in Echtzeit verfolgen:

  ```bash
  docker logs -f <container_name>
  ```

- **Logs durchsuchen:** Mit Tools wie `grep` kannst du gezielt nach Fehlern oder anderen Ereignissen suchen:

  ```bash
  docker logs <container_name> | grep "ERROR"
  ```

### Zugriff auf Logs per SSH:

Wenn du remote auf dein Synology NAS zugreifen möchtest, kannst du das direkt über SSH tun und die Docker-Logs anzeigen:

```bash
ssh username@synology-ip
docker logs <container_name>
```

### Vorteile:
- **Flexibel und unkompliziert:** Zugriff auf Logs von überall, ohne auf das NAS-Interface angewiesen zu sein.
- **Automatische Log-Rotation:** Docker verwaltet die Logs und stellt sicher, dass sie nicht unnötig viel Speicherplatz belegen.
