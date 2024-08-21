## **Erstellen und Strukturieren der Python-Packages**

In diesem Kapitel wird erläutert, wie wir die Python-Packages für die verschiedenen Prozessschritte erstellen und organisieren. Jedes Package entspricht einem spezifischen Schritt in unserem Workflow und kann sowohl automatisch vom Prozessmanager als auch manuell über die Kommandozeile aufgerufen werden.

### **1. Konzept: Jedes Package als eigenständiger Prozessschritt**

Jedes Python-Package repräsentiert einen klar definierten Prozessschritt in unserem Workflow. Zum Beispiel:
- **`kurmann-compress-fcp-export`:** Kompression der Final Cut Pro Exporte.
- **`kurmann-manage-mediasets`:** Integration von neuen Mediathekinhalten.

Diese Trennung stellt sicher, dass jeder Prozessschritt unabhängig entwickelt und ausgeführt werden kann. Alle Packages sind so gestaltet, dass sie sowohl automatisiert (z.B. durch `watchdog`) als auch manuell über die Kommandozeile gestartet werden können.

### **2. Struktur der Python-Packages**

Die Struktur eines typischen Python-Packages sieht folgendermaßen aus:

```
~/bin/
    ├── kurmann-compress-fcp-export  # Hauptskript für den Prozessschritt
    └── kurmann-manage-mediasets     # Hauptskript für den Prozessschritt
~/kurmann-videoschnitt/
    ├── kurmann_compress_fcp_export/
    │   ├── __init__.py
    │   ├── main.py
    │   └── helper.py
    └── kurmann_manage_mediasets/
        ├── __init__.py
        ├── main.py
        └── helper.py
```

- **`~/bin/`**: Hier befinden sich die ausführbaren Scripte, die direkt aufgerufen werden können. Diese Scripte sind die Einstiegspunkte für die jeweiligen Prozessschritte.
- **`~/kurmann-videoschnitt/`**: Der eigentliche Code der Python-Packages ist in diesem Verzeichnis organisiert. Jedes Package hat eine klare Struktur und kann unabhängig entwickelt und getestet werden.

### **3. Erstellen des ausführbaren Scripts im `bin`-Verzeichnis**

Um ein Python-Package direkt aus dem `bin`-Verzeichnis starten zu können, erstellen wir ein ausführbares Skript mit dem entsprechenden Shebang. Der Shebang ist eine spezielle Zeile am Anfang des Skripts, die dem System mitteilt, mit welchem Interpreter (in diesem Fall Python) das Skript ausgeführt werden soll.

#### **Beispiel für das ausführbare Script `kurmann-compress-fcp-export`:**

```python
#!/usr/bin/env python3

from kurmann_compress_fcp_export.main import run

if __name__ == "__main__":
    run()
```

- **`#!/usr/bin/env python3`**: Der Shebang gibt an, dass das Skript mit Python 3 ausgeführt werden soll. Das System nutzt den Python-Interpreter, der in der Umgebung verfügbar ist.
- **Import der Logik**: Das Skript importiert die Hauptfunktion (`run`) aus dem entsprechenden Modul. Diese Funktion enthält die Logik für den Prozessschritt.

#### **Shebang und Ausführbarkeit**

Der Shebang sorgt dafür, dass das Skript ohne explizites Aufrufen von Python gestartet werden kann. Damit dies funktioniert, muss das Skript ausführbar gemacht werden:

```bash
chmod +x ~/bin/kurmann-compress-fcp-export
```

Nach dem Setzen der Berechtigungen kann das Skript direkt über die Kommandozeile aufgerufen werden:

```bash
kurmann-compress-fcp-export
```

#### **Wichtig: Keine Dateierweiterung für Hauptscripts**

Um die Benutzerfreundlichkeit zu maximieren, verwenden wir für die ausführbaren Scripts im `bin`-Verzeichnis **keine Dateierweiterung** wie `.py`. Dadurch können die Scripts wie reguläre Befehle genutzt werden, was den Aufruf intuitiver und kürzer macht.

**Warum lassen wir die Dateierweiterung weg?**
1. **Klarheit und Benutzerfreundlichkeit:** Befehle wie `kurmann-compress-fcp-export` sind intuitiver und kürzer als z.B. `kurmann-compress-fcp-export.py`. Viele Unix-Programme wie `git`, `python` oder `ls` folgen diesem Prinzip.
2. **Systemintegration:** Das Weglassen der Dateierweiterung signalisiert, dass es sich um ein ausführbares Skript oder Programm handelt, das wie ein regulärer Befehl verwendet wird.
3. **Flexibilität:** Falls sich der Code in der Zukunft ändert oder eine andere Sprache verwendet wird, bleibt der Befehl unverändert. Die Benutzer müssen sich keine neuen Namen merken.

### **4. Deployment der Packages in das `bin`-Verzeichnis**

Um sicherzustellen, dass die ausführbaren Scripte immer aktuell sind, verwenden wir Vscode Tasks zum Deployment. Jedes Mal, wenn Änderungen am Code vorgenommen werden, können die Scripte automatisch ins `bin`-Verzeichnis kopiert und bereitgestellt werden.

### **5. CLI-Parameter für die Pakete**

Jedes Package kann spezifische CLI-Parameter verarbeiten, die entweder die Konfiguration aus der YAML-Datei überschreiben oder zusätzliche Optionen bieten.

#### **Beispiel für die Verarbeitung von CLI-Parametern:**

```python
import argparse

def run():
    parser = argparse.ArgumentParser(description="Kompression von Final Cut Pro Exporten")
    parser.add_argument("--directory", "-d", type=str, help="Verzeichnis für die Exporte")
    
    args = parser.parse_args()
    
    # Standardverzeichnis aus der Konfiguration laden, falls kein Parameter übergeben wurde
    directory = args.directory or "/Standardpfad/zum/Verzeichnis"
    
    # Logik für den Prozessschritt
    print(f"Starte Kompression im Verzeichnis: {directory}")
```

In diesem Beispiel kann das Verzeichnis über den CLI-Parameter `--directory` überschrieben werden.

### **6. Vorteile dieser Struktur**

- **Modularität:** Jedes Package ist unabhängig und kann für sich selbst stehen. Dadurch wird die Entwicklung und Wartung vereinfacht.
- **Flexibilität:** Die Pakete können sowohl automatisiert als auch manuell gestartet werden, ohne dass Anpassungen nötig sind.
- **Einfache Erweiterbarkeit:** Neue Prozessschritte können durch Hinzufügen eines neuen Python-Packages und eines entsprechenden Scripts im `bin`-Verzeichnis leicht integriert werden.

### **7. Zusammenfassung**

Mit dieser Struktur erreichst du eine saubere Trennung der Prozesslogik und eine einfache Möglichkeit, jedes Package separat aufzurufen. Die Verwendung des Python-Shebangs in Kombination mit den ausführbaren Scripten im `bin`-Verzeichnis ermöglicht eine nahtlose Integration in den Workflow und die Systemumgebung. Die Entscheidung, keine Dateierweiterung für die ausführbaren Scripts zu verwenden, sorgt für eine benutzerfreundliche und intuitive Bedienung.
