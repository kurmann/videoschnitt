# xml_utils.py

"""
Das 'xml_utils' Modul enthält Hilfsfunktionen für die Arbeit mit XML-Strukturen.
Es stellt Funktionen bereit, um XML-Elemente zu formatieren, insbesondere die Einrückung
von XML-Elementen für eine bessere Lesbarkeit.
"""

import xml.etree.ElementTree as ET

def indent(elem, level=0, space="  "):
    """
    Formatiert ein XML-Element rekursiv mit Einrückungen für eine bessere Lesbarkeit.

    Args:
        elem (xml.etree.ElementTree.Element): Das zu formatierende XML-Element.
        level (int): Die aktuelle Einrückungsebene (für die Rekursion).
        space (str): Der Einrückungsstring (z.B. zwei Leerzeichen).
    """
    i = "\n" + level * space
    if len(elem):
        if not elem.text or not elem.text.strip():
            elem.text = i + space
        else:
            elem.text = elem.text.strip()
        for child in elem:
            indent(child, level + 1, space)
        if not elem.tail or not elem.tail.strip():
            elem.tail = i
    else:
        if not elem.text or not elem.text.strip():
            elem.text = ''
        if level and (not elem.tail or not elem.tail.strip()):
            elem.tail = i