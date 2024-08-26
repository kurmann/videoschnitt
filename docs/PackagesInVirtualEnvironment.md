## Arbeiten mit PIP-Packages in einer virtuellen Umgebung (`.venv`) und `setup.py`

Die Arbeit mit PIP-Packages in einer Python-Entwicklungsumgebung kann komplex sein, insbesondere wenn man gleichzeitig die Vorteile virtueller Umgebungen (`.venv`) nutzt. In diesem Kapitel wird erklärt, wie du eigene Python-Pakete in einer virtuellen Umgebung korrekt erstellst, installierst und ausführst, und welche Stolpersteine (wie etwa Probleme bei der Parameterübergabe) du dabei vermeiden solltest.

### Grundlagen: Virtuelle Umgebungen (`.venv`) und `setup.py`

Virtuelle Umgebungen sind ein essenzielles Werkzeug, um Python-Pakete isoliert von der globalen Umgebung zu entwickeln und zu testen. Sie ermöglichen es, Pakete lokal in einer Umgebung zu installieren, ohne Konflikte mit globalen Installationen zu riskieren.

#### Erstellen und Aktivieren einer virtuellen Umgebung

```bash
# Erstellen der virtuellen Umgebung
python3 -m venv .venv

# Aktivieren der virtuellen Umgebung (Bash)
source .venv/bin/activate
```

Nach der Aktivierung befindet man sich in einer isolierten Umgebung, in der nur die Pakete und Abhängigkeiten installiert sind, die für dieses Projekt benötigt werden.

#### Konfiguration von `setup.py`

Die `setup.py` ist das zentrale Konfigurationsskript für dein Python-Paket. Sie definiert, welche Pakete und Skripte installiert werden und wie diese aufgerufen werden können.

Ein typisches Beispiel für eine `setup.py`:

```python
from setuptools import setup, find_packages

setup(
    name="mein_paket",
    version="0.1.0",
    description="Beispiel für ein Python-Paket",
    packages=find_packages(),
    entry_points={
        'console_scripts': [
            'mein-befehl=mein_modul.mein_skript:main',
        ],
    },
    python_requires='>=3.6',
)
```

**Wichtiger Hinweis:** Der `entry_point` wie `mein-befehl` ist der Befehl, den du später in der Konsole ausführst. Die Funktion `main` ist der Einstiegspunkt des Skripts.

#### Installation des Pakets in der virtuellen Umgebung

Um dein Paket in der virtuellen Umgebung zu installieren, verwende:

```bash
pip install .
```

Durch diesen Schritt wird dein Paket in der `.venv`-Umgebung installiert und der definierte Befehl (`mein-befehl`) steht global innerhalb der virtuellen Umgebung zur Verfügung.

### Die Problematik mit der Parameterübergabe in PIP-Packages

Ein häufiges Problem bei der Arbeit mit PIP-Packages ist die fehlerhafte Übergabe von Parametern. Dies tritt oft dann auf, wenn ein Skript direkt funktioniert, aber beim Aufruf als installiertes Paket nicht wie erwartet reagiert.

**Problem:** Die Parameter werden bei der Übergabe über `sys.argv` nicht korrekt verarbeitet.

**Lösung:** Stelle sicher, dass dein Skript die Parameter richtig einliest und verarbeitet. Ein typischer Ansatz:

```python
import sys

def main():
    if len(sys.argv) != 4:
        print("Usage: mein-befehl /pfad/zum/quellverzeichnis /pfad/zum/zielverzeichnis [true/false]")
        sys.exit(1)

    quellverzeichnis = sys.argv[1]
    zielverzeichnis = sys.argv[2]
    kompression = sys.argv[3].lower() == "true"

    # Weiterverarbeitung...

if __name__ == "__main__":
    main()
```

**Hinweis:** Der Schlüssel liegt darin, dass der `main`-Einstiegspunkt die Parameter direkt aus `sys.argv` liest. Falls du Probleme hast, überprüfe, ob der Pfad und die Parameter in der Konsole korrekt übergeben werden.

### Zusammenfassung und Best Practices

1. **Nutze virtuelle Umgebungen (`.venv`)**: Diese isolieren dein Projekt und verhindern Konflikte mit globalen Installationen. Für die Produktion sollten diese ebenfalls in Betracht gezogen werden, vor allem wenn spezifische Abhängigkeiten und Versionen benötigt werden.

2. **Achte auf die korrekte Konfiguration in `setup.py`**: Insbesondere die Definition der `entry_points` ist wichtig, um sicherzustellen, dass dein Paket wie erwartet aufgerufen wird.

3. **Parameterübergabe richtig handhaben**: Verwende immer `sys.argv` in der `main`-Funktion, um Parameter sauber zu verarbeiten. Das ist entscheidend für die Funktionalität deines Pakets, wenn es über die Konsole aufgerufen wird.

4. **Installation in der virtuellen Umgebung**: Mit `pip install .` wird dein Paket lokal installiert und kann in der `.venv`-Umgebung direkt getestet werden.

Durch die Beachtung dieser Best Practices kannst du sicherstellen, dass deine eigenen Python-Pakete zuverlässig funktionieren, sowohl in der Entwicklung als auch in der Produktion.
