import typer
from online_medialibrary_manager.commands import create_html, create_og_image, create_artwork

app = typer.Typer(help="Online Medialibrary Manager für Familienvideos")

app.command("create-html")(create_html.create_html_command)
app.command("create-og-image")(create_og_image.create_og_image_command)
app.command("create-artwork")(create_artwork.create_artwork_command)

if __name__ == '__main__':
    app()