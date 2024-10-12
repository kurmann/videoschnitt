# src/online_medialibrary_manager/commands/create_html.py

import locale
import typer
import os
from datetime import datetime
from metadata_manager import get_metadata_with_exiftool
from metadata_manager.commands.get_recording_date import get_recording_date_command as get_recording_date
from online_medialibrary_manager.commands.create_og_image import create_og_image_command

def create_html_command(
    metadata_source: str = typer.Argument(..., help="Pfad zur Videodatei, aus der die Metadaten extrahiert werden sollen"),
    high_res_file: str = typer.Argument(..., help="Pfad zur hochauflösenden Videodatei (4K HEVC)"),
    mid_res_file: str = typer.Argument(..., help="Pfad zur mittelauflösenden Videodatei (HD)"),
    artwork_image: str = typer.Argument(..., help="Pfad zum Vorschaubild"),
    subtitle: str = typer.Option(None, help="Optionaler Untertitel für die Seite (z.B. Ukrainisch)"),
    download_file: str = typer.Option(None, help="Optionaler Pfad zur Download-Datei (z.B. ZIP-Datei)"),
    base_url: str = typer.Option('', help="Basis-URL für die OG-Metadaten (z.B. https://example.com/videos)")
):
    """
    Erstellt eine statische HTML-Seite.

    Diese Methode verwendet die bereitgestellten Videodateien und Metadaten, um eine HTML-Seite zu generieren,
    die die Videos in verschiedenen Auflösungen anzeigt. Zusätzlich wird ein OpenGraph-Bild erstellt, das für
    die Vorschau auf sozialen Medien verwendet werden kann.
    """
    # Verzeichnis der metadata_source-Datei ermitteln
    output_directory = os.path.dirname(metadata_source)
    output_file = os.path.join(output_directory, 'index.html')

    # Prüfen, ob die Datei bereits existiert und Rückfrage stellen
    if os.path.exists(output_file):
        overwrite = typer.confirm(f"Die Datei '{output_file}' existiert bereits. Möchtest du sie überschreiben?")
        if not overwrite:
            typer.secho("Vorgang abgebrochen.", fg=typer.colors.RED)
            raise typer.Exit(code=1)

    # HTML generieren
    html_content = generate_html(metadata_source, high_res_file, mid_res_file, artwork_image, subtitle, download_file, base_url)

    # HTML-Datei speichern
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(html_content)
    typer.secho(f"HTML-Datei wurde erfolgreich erstellt: {output_file}", fg=typer.colors.GREEN)

def generate_html(metadata_source: str, high_res_file: str, mid_res_file: str, artwork_image: str, subtitle: str = None, download_file: str = None, base_url: str = '') -> str:
    """
    Generiert eine statische HTML-Seite für das Familienvideo und erstellt ein OpenGraph-Bild.

    Diese Methode verwendet die bereitgestellten Videodateien und Metadaten, um eine HTML-Seite zu erstellen,
    die die Videos in verschiedenen Auflösungen anzeigt. Zusätzlich wird ein OpenGraph-Bild erstellt, das für
    die Vorschau auf sozialen Medien verwendet werden kann.
    """
    # Locale auf Deutsch setzen
    try:
        locale.setlocale(locale.LC_TIME, 'de_CH.UTF-8')
    except locale.Error:
        try:
            locale.setlocale(locale.LC_TIME, 'de_DE.UTF-8')
        except locale.Error:
            locale.setlocale(locale.LC_TIME, '')

    # Extrahieren der Metadaten aus der angegebenen Metadatenquelle
    metadata = get_metadata_with_exiftool(metadata_source)

    # Wichtige Metadaten abrufen
    title = metadata.get('Title') or 'Familienfilm-Freigabe'
    description = metadata.get('Description') or ''
    recording_date = get_recording_date(metadata_source)
    if recording_date:
        recording_date_str = recording_date.strftime('%A, %-d. %B %Y')
    else:
        recording_date_str = ''

    # Erstellen des OpenGraph-Bildes und Rückgabe des Pfads
    og_image_path = create_og_image_command(artwork_image)

    # Dateinamen extrahieren
    high_res_file_name = os.path.basename(high_res_file)
    mid_res_file_name = os.path.basename(mid_res_file)
    og_image_name = os.path.basename(og_image_path)
    download_file_name = os.path.basename(download_file) if download_file else None

    # Wenn base_url angegeben ist, fügen wir sie den Dateinamen hinzu
    def make_absolute(url):
        return f'{base_url.rstrip("/")}/{url.lstrip("/")}' if base_url else url

    # OpenGraph Metadaten mit absoluten URLs
    og_meta_tags = f'''
        <meta property="og:title" content="{title}" />
        <meta property="og:type" content="website" />
        <meta property="og:url" content="{make_absolute('index.html')}" />
        <meta property="og:image" content="{make_absolute(og_image_name)}" />
        <meta property="og:image:type" content="image/jpeg" />
        <meta property="og:description" content="{description}" />
        <meta property="og:image:width" content="1536" />
        <meta property="og:image:height" content="804" />
        <meta property="og:locale" content="de_CH" />
        <meta property="og:site_name" content="Kurmann Mediathek: Familienfilm-Freigabe" />
    '''

    # HTML-Inhalt generieren
    subtitle_section = f'<div class="below-subtitle">{subtitle}</div>' if subtitle else ''

    # Download-Link, falls vorhanden
    download_section = f'''
        <a href="{download_file_name}" id="download-link">
            Original herunterladen
            <span>Für Wiedergabe auf PC/Mac</span>
        </a>
    ''' if download_file_name else ''

    html_content = f'''<!DOCTYPE html>
<html lang="de-CH">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>{title}</title>

    <!-- OpenGraph Metadaten -->
    {og_meta_tags}

    <!-- CSS -->
    {generate_css()}

</head>
<body>
    <h1>Kurmann Mediathek</h1>
    <h2 class="subtitle">Familienfilm-Freigabe von Patrick Kurmann</h2>
    <div class="container">
        <h2 id="title-link">{title}</h2>
        {subtitle_section}
        <div class="video-container" id="video-container">
            <a href="{mid_res_file_name}" id="play-link">
                <img src="{artwork_image}" alt="{title}" class="video-image">
                <div class="play-icon">
                    <svg width="100%" height="100%" viewBox="0 0 64 64" xmlns="http://www.w3.org/2000/svg" fill="#ffffff">
                        <circle cx="32" cy="32" r="32" opacity="0.7"/>
                        <polygon points="26,20 26,44 46,32" fill="#000000"/>
                    </svg>
                </div>
                <div class="video-details">
                    {recording_date_str}
                </div>
            </a>
        </div>

        <p class="video-description">
            {description}
        </p>

        <!-- Links für manuelle Auswahl der Auflösung und Download -->
        <div class="links">
            <a href="{high_res_file_name}" id="4k-link">
                Film in 4K-Qualität
                <span>Dolby Vision</span>
            </a>
            <a href="{mid_res_file_name}" id="hd-link">
                Film in HD-Qualität
                <span>1080p SDR</span>
            </a>
            {download_section}
        </div>
    </div>

    <footer>
        &copy; {datetime.now().year} Kurmann Online-Mediathek von Patrick Kurmann. Alle Rechte vorbehalten.
    </footer>

    <!-- JavaScript -->
    {generate_javascript(high_res_file_name, mid_res_file_name)}
</body>

</html>
'''

    return html_content

def generate_css() -> str:
    """
    Generiert das CSS für die HTML-Seite.

    Returns:
        str: Der CSS-String.
    """
    return '''
    <style>
        body {
            margin: 0;
            padding: 0;
            font-family: -apple-system, BlinkMacSystemFont, 'Helvetica Neue', Helvetica, Arial, sans-serif;
            color: #e0e0e0;
            background-color: #181818;
        }

        h1 {
            font-size: 1.8em;
            color: #ffffff;
            text-align: center;
            font-weight: 400;
            padding-bottom: 0.7rem;
            border-bottom: 1px solid silver;
            letter-spacing: 0.1em;
        }

        h2 {
            color: #ffffff;
            text-align: center;
            font-weight: 300;
            letter-spacing: 0.1em;
        }

        h2.subtitle {
            font-size: 1em;
        }

        .below-subtitle {
            text-align: center;
            font-size: 1em;
            margin-top: 0;
        }

        h2.title-link {
            font-size: 1.5em;
        }

        .container {
            max-width: 900px;
            margin: 0 auto;
            padding: 20px;
            background-color: #1e1e1e;
            border-radius: 10px;
            box-shadow: 0 8px 20px rgba(0, 0, 0, 0.5);
        }

        .video-container {
            position: relative;
            display: inline-block;
            overflow: hidden;
            cursor: pointer;
            border-radius: 8px;
        }

        .video-image {
            width: 100%;
            height: auto;
            display: block;
            border-radius: 8px;
            transition: transform 0.3s ease-in-out;
        }

        .video-container:hover .video-image {
            transform: scale(1.05);
        }

        .play-icon {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            width: 64px;
            height: 64px;
            opacity: 0.9;
            transition: opacity 0.3s ease-in-out;
        }

        .video-container:hover .play-icon {
            opacity: 1;
        }

        .video-details {
            position: absolute;
            bottom: 15px;
            left: 15px;
            color: #ffffff;
            background-color: rgba(0, 0, 0, 0.6);
            padding: 10px 15px;
            font-size: 1em;
            border-radius: 5px;
        }

        .video-description {
            margin: 20px;
            color: #b0b0b0;
            font-size: 1.1em;
            line-height: 1.7em;
            text-align: center;
        }

        .links {
            text-align: center;
            display: flex;
            justify-content: center;
            flex-wrap: wrap;
            gap: 20px;
            margin-top: 30px;
        }

        .links a {
            color: #ffae42;
            text-decoration: none;
            padding: 10px 15px;
            border: 1px solid #ffae42;
            border-radius: 5px;
            transition: background-color 0.3s ease, color 0.3s ease;
            width: 200px;
            text-align: center;
        }

        .links span {
            color: #b0b0b0;
            font-size: 0.8em;
            margin-top: 5px;
            display: block;
        }

        .links a:hover {
            background-color: #ffae42;
            color: #181818;
        }

        .links a:hover span {
            color: white;
        }

        footer {
            text-align: center;
            padding: 20px;
            color: #777;
            font-size: 0.9em;
        }
    </style>
    '''

def generate_javascript(high_res_file_name: str, mid_res_file_name: str) -> str:
    """
    Generiert das JavaScript für die HTML-Seite.

    Args:
        high_res_file_name (str): Der Name der 4K-Datei.
        mid_res_file_name (str): Der Name der HD-Datei.

    Returns:
        str: Der JavaScript-String.
    """
    return f'''
    <script>
        document.addEventListener('DOMContentLoaded', function() {{
            var playLink = document.getElementById('play-link');
            var highResFile = '{high_res_file_name}';
            var midResFile = '{mid_res_file_name}';

            function canPlayHEVC() {{
                var video = document.createElement('video');
                return video.canPlayType('video/mp4; codecs="hvc1"') !== '';
            }}

            function getQueryParam(param) {{
                var urlParams = new URLSearchParams(window.location.search);
                return urlParams.get(param);
            }}

            function getAnchorParam() {{
                return window.location.hash.substr(1);
            }}

            var playParam = getQueryParam('play') || getAnchorParam();

            if (playParam) {{
                if (playParam.toLowerCase() === '4k') {{
                    playLink.href = highResFile;
                }} else if (playParam.toLowerCase() === 'hd') {{
                    playLink.href = midResFile;
                }} else {{
                    playLink.href = midResFile;
                }}
            }} else {{
                if (canPlayHEVC()) {{
                    playLink.href = highResFile;
                }} else {{
                    playLink.href = midResFile;
                }}
            }}
        }});
    </script>
    '''