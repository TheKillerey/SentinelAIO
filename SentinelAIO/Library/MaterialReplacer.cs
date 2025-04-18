using System.IO;
using System.Text.RegularExpressions;

namespace SentinelAIO.Library;

public class MaterialReplacer
{
    /// <summary>
    ///     Entfernt alle Materialblöcke aus einer Datei und ersetzt sie durch neue Materialblöcke.
    /// </summary>
    /// <param name="originalFilePath">Pfad zur Originaldatei.</param>
    /// <param name="combinedMaterialsFilePath">Pfad zur Datei mit den neuen kombinierten Materialien.</param>
    public void ReplaceMaterialsInFile(string originalFilePath, string combinedMaterialsFilePath)
    {
        // Sicherstellen, dass die Originaldatei und die kombinierte Datei existieren
        if (!File.Exists(originalFilePath))
            throw new FileNotFoundException($"Die Datei '{originalFilePath}' wurde nicht gefunden.");
        if (!File.Exists(combinedMaterialsFilePath))
            throw new FileNotFoundException($"Die Datei '{combinedMaterialsFilePath}' wurde nicht gefunden.");

        // Dateiinhalt Zeile für Zeile laden
        string[] lines = File.ReadAllLines(originalFilePath);

        // Regex zur Erkennung eines Materialanfangs
        var materialStartRegex = new Regex(@"^\s*"".*StaticMaterialDef\s*\{");

        // Neue Zeilen, basierend auf den alten Inhalten (ohne Materialien)
        List<string> updatedLines = new();

        var insideMaterial = false;
        var openBracesCount = 0;

        // Erste Position eines Materials in der Originaldatei speichern
        var firstMaterialPosition = -1;

        // Datei Zeile für Zeile durchgehen
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Erkennen, wo ein neuer Materialblock beginnt
            if (!insideMaterial && materialStartRegex.IsMatch(line))
            {
                insideMaterial = true;
                openBracesCount = 0;

                // Position des ersten Materials speichern (für späteres Einfügen)
                if (firstMaterialPosition == -1)
                    firstMaterialPosition = updatedLines.Count;
            }

            if (insideMaterial)
            {
                // Klammern zählen (um Materialblockende zu erkennen)
                openBracesCount += CountOccurrences(line, '{');
                openBracesCount -= CountOccurrences(line, '}');

                // Ende des Materialblocks erreicht
                if (openBracesCount == 0)
                    insideMaterial = false;
            }
            else
            {
                // Alle Nicht-Material-Zeilen hinzufügen
                updatedLines.Add(line);
            }
        }

        // Inhalte der Combined-Materials-Datei laden
        string[] combinedMaterials = File.ReadAllLines(combinedMaterialsFilePath);

        // Die neuen Materialien an der ersten Materialposition einfügen
        if (firstMaterialPosition != -1)
            updatedLines.InsertRange(firstMaterialPosition, combinedMaterials);
        else
            // Kein Materialblock gefunden, also ans Ende der Datei hinzufügen
            updatedLines.AddRange(combinedMaterials);

        // Datei mit den aktualisierten Inhalten überschreiben
        File.WriteAllLines(originalFilePath, updatedLines);

        Console.WriteLine(
            $"Materialblöcke wurden erfolgreich durch die neuen Materialien ersetzt in '{originalFilePath}'.");
    }

    /// <summary>
    ///     Hilfsfunktion: Zählt, wie oft ein bestimmtes Zeichen in einer Zeile vorkommt.
    /// </summary>
    /// <param name="line">Die Zeile, die geprüft werden soll.</param>
    /// <param name="character">Das Zeichen, dessen Vorkommen gezählt werden sollen.</param>
    /// <returns>Anzahl der Vorkommen.</returns>
    private int CountOccurrences(string line, char character)
    {
        var count = 0;
        foreach (var c in line)
            if (c == character)
                count++;
        return count;
    }
}