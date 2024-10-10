import typer
from online_medialibrary_manager.commands import create_og_image, generate_html

app = typer.Typer(help="Online Medialibrary Manager für Familienvideos")

# Registriere den Befehl für create_html
app.command()(generate_html.create_html_command)

# Registriere den Befehl für create_og_image
app.command()(create_og_image.create_og_image)

if __name__ == '__main__':
    app()