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
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">

    <!-- OpenGraph Metadaten -->
    {og_meta_tags}

    <style>
        /* Hier kommen Ihre CSS-Stile hin */
        html,
        body {{
            font-family: -apple-system, BlinkMacSystemFont, sans-serif, 'Helvetica Neue', Helvetica, Arial, sans-serif;
            color: #bbb;
            background-color: black;
        }}

        h1 {{
            margin-bottom: 2rem;
        }}

        h1:focus {{
            outline: none;
        }}

        a {{
            color: #0071c1;
        }}

        .content {{
            padding-top: 1.1rem;
        }}

        .page {{
            position: relative;
            display: flex;
            flex-direction: column;
            background-color: black;
        }}

        main {{
            flex: 1;
            max-width: 1440px;
            margin: 0 auto;
        }}

        .video-container {{
            position: relative;
            display: inline-block;
            overflow: hidden;
        }}

        .video-image {{
            border: 1px solid #444;
        }}

        .video-container:hover img {{
            opacity: 0.8;
        }}

        .play-icon {{
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            height: 14%;
            opacity: 0.5;
        }}

        .video-category {{
            position: absolute;
            top: 3%;
        }}

        .video-container:hover .play-icon,
        .video-container:hover .video-text {{
            opacity: 0.7;
        }}

        .video-text {{
            position: absolute;
            top: 64%;
            left: 50%;
            transform: translate(-50%, -50%);
            opacity: 0;
            font-size: 1.2em;
            letter-spacing: 0.1em;
            color: white;
            text-align: center;
        }}

        .video-quality {{
            position: absolute;
            bottom: 3%;
            right: 0;
            font-size: 0.8em;
            white-space: nowrap;
            opacity: 0.8;
            height: 32px;
        }}

        .video-category,
        .video-quality {{
            color: white;
            background-color: rgba(0, 0, 0, 0.5);
            padding: 5px 12px;
            letter-spacing: 0.1em;
            font-weight: normal;
        }}

        .video-quality svg {{
            position: relative;
            margin-right: 10px;
            bottom: 4px;
        }}

        .video-quality .text {{
            position: relative;
            bottom: 3px;
        }}

        .recording-date-container {{
            position: absolute;
            bottom: 3%;
        }}

        .recording-date {{
            color: white;
            background-color: rgba(0, 0, 0, 0.5);
            padding: 0 12px;
            font-size: 0.8em;
            white-space: nowrap;
            letter-spacing: 0.1em;
            opacity: 0.8;
            height: 32px;
        }}

        .recording-date svg {{
            position: relative;
            top: 4px;
            margin-right: 7px;
        }}

        .recording-date .text {{
            position: relative;
            top: 6px;
        }}

        .check-icon {{
            opacity: 0.5;
            float: left;
            margin-right: 0.25rem;
            position: relative;
            left: -2px;
        }}

        .link-style {{
            color: #888;
            text-decoration: underline;
            cursor: pointer;
        }}
    </style>
</head>

<body>
    <main>
        <div class="container mt-3">
            <h1>Familienfilm-Freigabe</h1>
            <div class="mediaset">
                <h2><a href="{mid_res_file_name}" id="title-link">{title}</a></h2>

                <div class="video-container">
                    <div class="video-container" role="button" aria-label="{title}">
                        <a href="{mid_res_file_name}" id="play-link">
                            <img src="{artwork_image_name}" class="img-fluid video-image" alt="{title}">
                            <!-- Play-Icon -->
                            <div class="play-icon">
                                <!-- SVG des Play-Icons hier einfügen -->
                            </div>
                            <div class="video-category">Familie Kurmann-Glück</div>
                            <div class="video-quality">
                                <!-- Hier die Videoqualität anpassen -->
                                <svg width="32px" height="32px" viewBox="34.366 -43.13 181.269 181.269"
                                    xmlns="http://www.w3.org/2000/svg" fill="#ffffff">
                                    <!-- SVG-Inhalt -->
                                </svg>
                                <span class="text">Dolby Vision</span>
                            </div>
                            <div class="recording-date-container">
                                <div class="recording-date">
                                    <!-- Kalender-Icon -->
                                    <svg width="20px" height="20px" viewBox="0 -2 32 32" version="1.1"
                                        xmlns="http://www.w3.org/2000/svg" fill="#ffffff">
                                        <!-- SVG-Inhalt -->
                                    </svg>
                                    <span class="text">{recording_date_str}</span>
                                </div>
                            </div>
                        </a>
                    </div>
                </div>

                <p class="video-description">
                    {description}
                </p>

                <!-- Links für manuelle Auswahl der Auflösung -->
                <p>
                    <a href="{mid_res_file_name}" id="hd-link">Film in HD-Qualität abspielen</a><br>
                    <a href="{high_res_file_name}" id="4k-link">Film in 4K-Qualität abspielen</a><br>
                    <a href="{original_file_name}" id="original-link">Originaldatei herunterladen</a>
                </p>

            </div>
        </div>
    </main>

    <footer class="text-center py-3">
        <div class="container">
            <p>&copy; {datetime.now().year} Mediaset Share by Patrick Kurmann. Alle Rechte vorbehalten.</p>
        </div>
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