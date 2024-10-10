import typer
from online_medialibrary_manager.commands import create_og_image, generate_html, create_artwork

app = typer.Typer(help="Online Medialibrary Manager für Familienvideos")

app.command("create-html")(generate_html.create_html_command)
app.command("create-og-image")(create_og_image.create_og_image_command)
app.command("create-artwork")(create_artwork.create_artwork_command)

if __name__ == '__main__':
    app()