import os
import subprocess

# Modulvariablen für die Pfade
ROOT_DIR = os.path.dirname(os.path.dirname(os.path.dirname(__file__)))
README_FILE = os.path.join(ROOT_DIR, 'README.md')
CLI_DOCS_FILE = os.path.join(ROOT_DIR, 'CLI_Documentation.md')

def generate_cli_docs(command, output_file):
    """Generiert die CLI-Dokumentation für ein bestimmtes Kommando."""
    with open(output_file, 'a') as f:
        f.write(f"## {command} CLI-Dokumentation\n\n")
        try:
            result = subprocess.run([command, '--help'], capture_output=True, text=True)
            f.write(f"```\n{result.stdout}\n```\n")
        except Exception as e:
            f.write(f"Fehler beim Generieren der Dokumentation für {command}: {e}\n")
        f.write("\n")

def insert_cli_docs_in_readme(readme_file, cli_docs_file):
    """Fügt die generierte CLI-Dokumentation in das README.md ein."""
    with open(cli_docs_file, 'r') as cli_docs:
        cli_content = cli_docs.read()

    with open(readme_file, 'r') as readme:
        readme_content = readme.read()

    # Suche nach einem Platzhalter in README.md, wo die CLI-Doku eingefügt werden soll
    start_marker = "<!-- CLI-DOCS-START -->"
    end_marker = "<!-- CLI-DOCS-END -->"

    start_index = readme_content.find(start_marker) + len(start_marker)
    end_index = readme_content.find(end_marker)

    if start_index == -1 or end_index == -1:
        raise ValueError("Platzhalter für CLI-Dokumentation nicht gefunden.")

    new_readme_content = (readme_content[:start_index] +
                          "\n" + cli_content + "\n" +
                          readme_content[end_index:])

    with open(readme_file, 'w') as readme:
        readme.write(new_readme_content)

if __name__ == "__main__":
    # Lösche den Inhalt der CLI-Dokumentation-Datei
    open(CLI_DOCS_FILE, 'w').close()

    # Generiere die Dokumentation für jedes CLI
    commands = ['apple-compressor-manager', 'original-media-integrator', 'emby-integrator']
    for command in commands:
        generate_cli_docs(command, CLI_DOCS_FILE)

    # Füge die generierte CLI-Dokumentation in das README ein
    insert_cli_docs_in_readme(README_FILE, CLI_DOCS_FILE)