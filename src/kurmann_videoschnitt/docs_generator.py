import subprocess

def generate_cli_docs(cli_command, output_file, title):
    """Generiert die Dokumentation für einen Click CLI-Befehl."""
    with open(output_file, 'a') as f:
        # Schreibe den Titel der Sektion in die Markdown-Datei
        f.write(f"# {title} CLI-Dokumentation\n\n")
        
        # Führe den CLI-Befehl mit der --help Option aus und schreibe die Ausgabe in die Datei
        result = subprocess.run(cli_command + ['--help'], capture_output=True, text=True)
        f.write("## Befehlsübersicht\n\n")
        f.write(f"```\n{result.stdout}\n```\n")

# Lösche den Inhalt der Datei vor der Neugenerierung
open("kurmann_videoschnitt_cli_docs.md", 'w').close()

# Generiere die Dokumentation für die Haupt-CLI
generate_cli_docs(['kurmann-videoschnitt'], 'kurmann_videoschnitt_cli_docs.md', 'Kurmann Videoschnitt')

# Generiere die Dokumentation für die Sub-CLIs
generate_cli_docs(['kurmann-videoschnitt', 'compressor'], 'kurmann_videoschnitt_cli_docs.md', 'Apple Compressor Manager')
generate_cli_docs(['kurmann-videoschnitt', 'integrator'], 'kurmann_videoschnitt_cli_docs.md', 'Original Media Integrator')
generate_cli_docs(['kurmann-videoschnitt', 'emby'], 'kurmann_videoschnitt_cli_docs.md', 'Emby Integrator')
