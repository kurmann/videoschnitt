import subprocess
import re
import os

def generate_cli_docs(cli_command, title):
    """Generiert die CLI-Dokumentation für einen Click-Befehl."""
    result = subprocess.run(cli_command + ['--help'], capture_output=True, text=True)
    return f"### {title}\n\n```\n{result.stdout}\n```\n"

def update_readme_with_cli_docs(cli_docs):
    """Aktualisiert das README.md im Wurzelverzeichnis, indem der alte CLI-Dokumentationsabschnitt ersetzt wird."""
    # Definiere den Pfad zur README.md im Wurzelverzeichnis
    root_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "../../"))
    readme_path = os.path.join(root_dir, "README.md")
    
    # Lies die aktuelle README-Datei
    with open(readme_path, 'r') as file:
        readme_content = file.read()

    # Suchmuster für den CLI-Dokumentationsabschnitt
    cli_docs_pattern = re.compile(r"(## CLI-Dokumentation\n)(.*?)(\n##|$)", re.DOTALL)
    
    # Ersetze den alten CLI-Dokumentationsabschnitt durch die neue Dokumentation
    if cli_docs_pattern.search(readme_content):
        updated_readme_content = cli_docs_pattern.sub(f"## CLI-Dokumentation\n{cli_docs}\n##", readme_content)
    else:
        # Falls das CLI-Dokumentationskapitel nicht vorhanden ist, hänge es am Ende an
        updated_readme_content = f"{readme_content}\n## CLI-Dokumentation\n{cli_docs}\n"

    # Schreibe die aktualisierte README-Datei
    with open(readme_path, 'w') as file:
        file.write(updated_readme_content)

# Generiere die CLI-Dokumentation für alle Befehle
cli_docs = generate_cli_docs(['kurmann-videoschnitt'], 'Haupt-CLI')
cli_docs += generate_cli_docs(['kurmann-videoschnitt', 'compressor'], 'Apple Compressor Manager CLI')
cli_docs += generate_cli_docs(['kurmann-videoschnitt', 'integrator'], 'Original Media Integrator CLI')
cli_docs += generate_cli_docs(['kurmann-videoschnitt', 'emby'], 'Emby Integrator CLI')

# Aktualisiere das README.md mit der neuen CLI-Dokumentation
update_readme_with_cli_docs(cli_docs)
