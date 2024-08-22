## **Konfiguration und CLI-Parameterüberschreibung**

In diesem Kapitel erfährst du, wie du die Konfiguration für die einzelnen Python-Pakete einrichtest und wie du bei Bedarf spezifische Einstellungen direkt über die Kommandozeile überschreiben kannst.

### **1. Konfigurationsdatei in `Application Support`**

Die Konfiguration der Verzeichnisse und Pfade erfolgt über eine YAML-Datei, die zentral in `Application Support` abgelegt wird. Diese Konfigurationsdatei definiert die Verzeichnispfade und andere Einstellungen, die für die einzelnen Arbeitsflüsse relevant sind.

#### **Pfad zur Konfigurationsdatei:**

```bash
~/Library/Application Support/kurmann-workflows/config.yaml
```

#### **Beispiel für den Inhalt der Konfigurationsdatei:**

```yaml
# config.yaml
final_cut_export_directory: "/Users/patrickkurmann/Videoschnitt/FinalCutExports"
mediathek_input_directory: "/Users/patrickkurmann/Videoschnitt/MediathekEingang"
```

In dieser YAML-Datei sind die Verzeichnispfade definiert, die direkt mit den spezifischen Prozessen und Arbeitsflüssen verbunden sind. Diese Pfade werden vom Prozessmanager ausgelesen und entsprechend überwacht.

### **2. Verwendung der Konfigurationswerte**

Die Konfigurationswerte aus der YAML-Datei werden beim Starten des Prozessmanagers geladen und den entsprechenden Arbeitsflüssen zugeordnet. Der Prozessmanager überwacht die definierten Verzeichnisse und startet die entsprechenden Arbeitsflüsse, sobald dort neue Dateien erkannt werden.

Beispielhafte Arbeitsflüsse:
- **Final Cut Pro Export Verzeichnis:** Startet den Kompressionsprozess.
- **Mediathek Eingang:** Startet den Integrationsprozess für neue Mediathekinhalte.

### **3. Sicheres Speichern von Secrets mit `keyring` (macOS)**

Für sensible Daten wie FTP-Zugangsdaten oder API-Schlüssel solltest du die `config.yaml` **nicht** verwenden, da diese im Klartext gespeichert werden. Stattdessen bietet sich auf macOS die Verwendung der nativen Keychain mit dem Python-Modul `keyring` an.

#### **Installation von `keyring`:**

```bash
pip install keyring
```

#### **Speichern von Zugangsdaten in der Keychain:**

Du kannst Secrets sicher in der Keychain speichern:

```python
import keyring

# Speichern der Zugangsdaten in der Keychain
keyring.set_password("kurmann_ftp", "username", "dein_ftp_passwort")
```

#### **Abrufen von Zugangsdaten aus der Keychain:**

In deinem Python-Code kannst du dann auf die sicher gespeicherten Zugangsdaten zugreifen:

```python
import keyring

ftp_password = keyring.get_password("kurmann_ftp", "username")
```

### **4. Überschreiben von Konfigurationswerten über CLI-Parameter**

Es kann Situationen geben, in denen du die in der YAML-Datei definierten Verzeichnispfade oder andere Parameter manuell überschreiben möchtest. Dies kannst du direkt über CLI-Parameter tun.

#### **Beispiel für den CLI-Aufruf mit Parameterüberschreibung:**

Angenommen, du möchtest den `kurmann-compress-fcp-export`-Arbeitsfluss mit einem anderen Verzeichnis als dem in der YAML-Datei definierten starten:

```bash
kurmann-compress-fcp-export --directory /Users/patrickkurmann/Videoschnitt/AlternativeExports
```

In diesem Beispiel wird der Pfad `--directory` verwendet, um den Standardpfad aus der YAML-Konfiguration zu überschreiben.

### **5. Priorität der Konfigurationen**

Die Konfiguration folgt der folgenden Prioritätsregel:

1. **CLI-Parameter haben Vorrang:** Alle über die Kommandozeile übergebenen Parameter überschreiben die Standardwerte aus der YAML-Konfigurationsdatei.
2. **YAML-Datei als Fallback:** Wenn keine CLI-Parameter angegeben sind, werden die Verzeichnispfade und anderen Konfigurationswerte aus der YAML-Datei geladen.
3. **Secrets über `keyring`:** Sensible Daten wie Passwörter oder Zugangsdaten werden sicher über `keyring` abgerufen und nicht in der YAML-Datei gespeichert.

#### **Beispiel-Szenario:**

1. Die Konfigurationsdatei definiert das Verzeichnis für Final Cut Exporte als:

    ```yaml
    final_cut_export_directory: "/Users/patrickkurmann/Videoschnitt/FinalCutExports"
    ```

2. Du startest den Prozess jedoch mit einem anderen Verzeichnis:

    ```bash
    kurmann-compress-fcp-export --directory /Users/patrickkurmann/Videoschnitt/AlternativeExports
    ```

In diesem Fall wird das Verzeichnis aus der CLI verwendet, und die YAML-Konfiguration wird ignoriert.

### **6. Übersicht der verfügbaren CLI-Parameter**

Jedes Python-Paket bietet spezifische CLI-Parameter an, die du bei Bedarf überschreiben kannst. Hier eine Übersicht der wichtigsten Parameter:

#### **`kurmann-compress-fcp-export`:**

- `--directory` oder `-d`: Pfad zum Verzeichnis, das die zu verarbeitenden Exporte enthält.

    Beispiel:

    ```bash
    kurmann-compress-fcp-export --directory /Users/patrickkurmann/Videoschnitt/AlternativeExports
    ```

#### **`kurmann-manage-mediasets`:**

- `--directory` oder `-d`: Pfad zum Verzeichnis, in dem die neuen Mediathekinhalte integriert werden.

    Beispiel:

    ```bash
    kurmann-manage-mediasets --directory /Users/patrickkurmann/Videoschnitt/AlternativeMediathek
    ```
