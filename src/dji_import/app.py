import typer
from dji_import.commands.import_dji import import_dji_files

app = typer.Typer()

app.command("import")(import_dji_files)

if __name__ == "__main__":
    app()