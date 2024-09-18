# html_generator.py

"""
Das Modul 'html_generator' ist verantwortlich für die Erstellung einer statischen HTML-Seite,
die Videos in verschiedenen Auflösungen anbietet und die Metadaten korrekt einbindet.
"""

import os
from datetime import datetime
from emby_integrator.metadata_manager import get_metadata, parse_recording_date

def generate_html(original_file: str, high_res_file: str, mid_res_file: str, artwork_image: str) -> str:
    """
    Generiert den HTML-Inhalt basierend auf den bereitgestellten Dateien und Metadaten.

    Args:
        original_file (str): Pfad zur Originalvideodatei.
        high_res_file (str): Pfad zur hochauflösenden Videodatei (4K HEVC).
        mid_res_file (str): Pfad zur mittelauflösenden Videodatei (HD).
        artwork_image (str): Pfad zum Vorschaubild.

    Returns:
        str: Der generierte HTML-Inhalt als String.
    """

    # Extrahieren der Metadaten aus der Originaldatei
    metadata = get_metadata(original_file)

    # Wichtige Metadaten abrufen
    title = metadata.get('Title') or 'Familienfilm-Freigabe'
    description = metadata.get('Description') or ''
    recording_date = parse_recording_date(original_file)
    if recording_date:
        # Datumsformat auf Deutsch: "1. Juli 2024"
        recording_date_str = recording_date.strftime('%-d. %B %Y')
    else:
        recording_date_str = ''

    image_width = metadata.get('ImageWidth', '1920')
    image_height = metadata.get('ImageHeight', '1080')

    # Dateinamen extrahieren
    original_file_name = os.path.basename(original_file)
    high_res_file_name = os.path.basename(high_res_file)
    mid_res_file_name = os.path.basename(mid_res_file)
    artwork_image_name = os.path.basename(artwork_image)

    # OpenGraph Metadaten
    og_meta_tags = f'''
        <meta property="og:title" content="{title}" />
        <meta property="og:type" content="video.movie" />
        <meta property="og:url" content="{original_file_name}" />
        <meta property="og:image" content="{artwork_image_name}" />
        <meta property="og:image:secure_url" content="{artwork_image_name}" />
        <meta property="og:image:type" content="image/jpeg" />
        <meta property="og:description" content="{description}" />
        <meta property="og:image:width" content="{image_width}" />
        <meta property="og:image:height" content="{image_height}" />
        <meta property="og:locale" content="de_CH" />
        <meta property="og:site_name" content="Patrick Kurmann Familienfilm-Freigabe" />
    '''

    # HTML-Inhalt generieren
    html_content = f'''<!DOCTYPE html>
<html lang="de-CH">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>{title}</title>

    <!-- OpenGraph Metadaten -->
    {og_meta_tags}

    <style>
        /* Modernisiertes CSS für eine ansprechendere Darstellung */
        
        body {{
            margin: 0;
            padding: 0;
            font-family: -apple-system, BlinkMacSystemFont, 'Helvetica Neue', Helvetica, Arial, sans-serif;
            color: #e0e0e0;
            background-color: #181818;
        }}

        h1 {{
            margin: 20px;
            font-size: 2.5em;
            color: #ffffff;
            text-align: center;
            font-weight: 400;
        }}

        h2 {{
            margin: 20px;
            font-size: 1.8em;
            color: #ffffff;
            text-align: center;
            font-weight: 300;
        }}

        .container {{
            max-width: 900px;
            margin: 0 auto;
            padding: 20px;
            background-color: #1e1e1e;
            border-radius: 10px;
            box-shadow: 0 8px 20px rgba(0, 0, 0, 0.5);
        }}

        .video-container {{
            position: relative;
            display: inline-block;
            overflow: hidden;
            cursor: pointer;
            margin: 30px 0;
            border-radius: 8px;
        }}

        .video-image {{
            width: 100%;
            height: auto;
            display: block;
            border-radius: 8px;
            transition: transform 0.3s ease-in-out;
        }}

        .video-container:hover .video-image {{
            transform: scale(1.05);
        }}

        .play-icon {{
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            width: 64px;
            height: 64px;
            opacity: 0.9;
            transition: opacity 0.3s ease-in-out;
        }}

        .video-container:hover .play-icon {{
            opacity: 1;
        }}

        .video-details {{
            position: absolute;
            bottom: 15px;
            left: 15px;
            color: #ffffff;
            background-color: rgba(0, 0, 0, 0.6);
            padding: 10px 15px;
            font-size: 1em;
            border-radius: 5px;
        }}

        .video-description {{
            margin: 20px;
            color: #b0b0b0;
            font-size: 1.1em;
            line-height: 1.7em;
            text-align: justify;
        }}

        .links {{
            margin: 30px;
            font-size: 1.1em;
            text-align: center;
            display: flex;
            justify-content: space-between;
        }}

        .links a {{
            color: #ffae42;
            text-decoration: none;
            padding: 10px 15px;
            border: 1px solid #ffae42;
            border-radius: 5px;
            transition: background-color 0.3s ease, color 0.3s ease;
        }}

        .links a:hover {{
            background-color: #ffae42;
            color: #181818;
        }}

        footer {{
            text-align: center;
            padding: 20px;
            color: #777;
            font-size: 0.9em;
        }}
    </style>
</head>

<body>
    <h1>Familienfilm-Freigabe</h1>
    <div class="container">
        <h2 id="title-link">{title}</h2>

        <div class="video-container" id="video-container">
            <a href="{mid_res_file_name}" id="play-link">
                <img src="{artwork_image_name}" alt="{title}" class="video-image">
                <!-- Play-Icon -->
                <div class="play-icon">
                    <!-- SVG des Play-Icons hier einfügen -->
                    <!-- Beispiel: -->
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

        <!-- Links für manuelle Auswahl der Auflösung -->
        <div class="links">
            <a href="{high_res_file_name}" id="4k-link">Film in 4K-Qualität abspielen</a>
            <a href="{mid_res_file_name}" id="hd-link">Film in HD-Qualität abspielen</a>
            <a href="{original_file_name}" id="original-link">Originaldatei herunterladen</a>
        </div>
    </div>

    <footer>
        &copy; {datetime.now().year} Kurmann Online-Mediathek von Patrick Kurmann. Alle Rechte vorbehalten.
    </footer>

    <script>
        document.addEventListener('DOMContentLoaded', function() {{
            var playLink = document.getElementById('play-link');
            var titleLink = document.getElementById('title-link');
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
                return window.location.hash.substr(1); // Entfernt das '#'
            }}

            var playParam = getQueryParam('play') || getAnchorParam();

            if (playParam) {{
                if (playParam.toLowerCase() === '4k') {{
                    playLink.href = highResFile;
                    titleLink.href = highResFile;
                }} else if (playParam.toLowerCase() === 'hd') {{
                    playLink.href = midResFile;
                    titleLink.href = midResFile;
                }} else {{
                    // Standard
                    playLink.href = midResFile;
                    titleLink.href = midResFile;
                }}
            }} else {{
                if (canPlayHEVC()) {{
                    playLink.href = highResFile;
                    titleLink.href = highResFile;
                }} else {{
                    playLink.href = midResFile;
                    titleLink.href = midResFile;
                }}
            }}
        }});
    </script>
</body>

</html>
'''

    return html_content