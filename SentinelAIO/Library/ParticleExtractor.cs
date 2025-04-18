using System.IO;
using System.Text.RegularExpressions;

namespace SentinelAIO.Library;

public class ParticleExtractor
{
    /// <summary>
    ///     Extrahiert alle Partikel aus allen Dateien in einem Ordner (und dessen Unterordnern) und speichert diese im
    ///     angegebenen Zielordner.
    /// </summary>
    /// <param name="inputFolderPath">Der Pfad zum Quellordner.</param>
    /// <param name="outputFolderPath">Der Zielordner, in dem die extrahierten Dateien gespeichert werden sollen.</param>
    public void ExtractParticlesFromFolder(string inputFolderPath, string outputFolderPath)
    {
        // Hole alle Dateien aus dem Quellordner und seinen Unterordnern
        string[] inputFiles =
            Directory.GetFiles(inputFolderPath, "*.py",
                SearchOption.AllDirectories); // Filter basierend auf Dateierweiterung

        Console.WriteLine($"Gefundene Dateien: {inputFiles.Length}");

        // Verarbeite jede gefundene Datei
        foreach (var inputFile in inputFiles)
        {
            Console.WriteLine($"Verarbeite Datei: {Path.GetFileName(inputFile)}");

            // Extrahiere Partikel aus der aktuellen Datei
            ExtractParticlesToFiles(inputFile, outputFolderPath);
        }

        Console.WriteLine("Alle Dateien wurden verarbeitet.");
    }

    /// <summary>
    ///     Extrahiert alle Partikel-Codeblöcke aus einer Datei und speichert diese in separaten Dateien im angegebenen Ordner.
    /// </summary>
    /// <param name="inputFilePath">Der Pfad zur Quelldatei.</param>
    /// <param name="outputFolderPath">Der Zielordner, in dem die extrahierten Dateien gespeichert werden sollen.</param>
    public void ExtractParticlesToFiles(string inputFilePath, string outputFolderPath)
    {
        string[] lines = File.ReadAllLines(inputFilePath); // Datei auslesen
        List<List<string>> particleBlocks = ExtractParticleBlocks(lines); // Partikelblöcke extrahieren

        // Schreibe jeden Partikelblock in eine eigene Datei
        var fileIndex = 1;
        foreach (var block in particleBlocks)
        {
            var particleContent = string.Join(Environment.NewLine, block);

            // Versuche den Partikelnamen aus dem Block zu extrahieren
            var particleName = ExtractParticleName(particleContent);
            if (string.IsNullOrEmpty(particleName)) particleName = $"particle_{fileIndex}";

            // Schreiben der Datei
            var outputFilePath = Path.Combine(outputFolderPath, $"{particleName}.py");

            // Erstelle Zielordner, falls nicht vorhanden
            Directory.CreateDirectory(outputFolderPath);

            File.WriteAllText(outputFilePath, particleContent);
            fileIndex++;
        }

        Console.WriteLine(
            $"Extraktion abgeschlossen. {particleBlocks.Count} Partikel aus {Path.GetFileName(inputFilePath)} gespeichert.");
    }

    /// <summary>
    ///     Extrahiert Partikelblöcke aus einem Array von Zeilen.
    /// </summary>
    /// <param name="lines">Der Inhalt der Datei als Array von Zeilen.</param>
    /// <returns>Eine Liste von Partikelblöcken, wobei jeder Partikelblock eine Liste von Zeilen ist.</returns>
    private List<List<string>> ExtractParticleBlocks(string[] lines)
    {
        List<List<string>> particleBlocks = new();
        List<string> currentBlock = new();
        var insideParticle = false;
        var openBracesCount = 0;

        // Regel zur Erkennung des Partikelanfangs
        var particleStartRegex = new Regex(@""".*Particles.*"" = VfxSystemDefinitionData {");

        foreach (var line in lines)
        {
            if (particleStartRegex.IsMatch(line))
            {
                if (currentBlock.Count > 0 && openBracesCount == 0)
                {
                    particleBlocks.Add(currentBlock);
                    currentBlock = new List<string>();
                }

                insideParticle = true;
                openBracesCount = 0;
            }

            if (insideParticle)
            {
                currentBlock.Add(line);

                openBracesCount += CountOccurrences(line, '{');
                openBracesCount -= CountOccurrences(line, '}');

                if (openBracesCount == 0)
                {
                    insideParticle = false;
                    particleBlocks.Add(currentBlock);
                    currentBlock = new List<string>();
                }
            }
        }

        if (currentBlock.Count > 0 && openBracesCount == 0) particleBlocks.Add(currentBlock);

        return particleBlocks;
    }

    /// <summary>
    ///     Extrahiert den Namen des Partikels aus einem Partikelblock.
    /// </summary>
    /// <param name="particleContent">Der Codeblock des Partikels als String.</param>
    /// <returns>Der Name des Partikels, falls vorhanden. Andernfalls null.</returns>
    private string ExtractParticleName(string particleContent)
    {
        var particleNameRegex = new Regex(@"particleName: string = \""([^\""]+)\""");
        var match = particleNameRegex.Match(particleContent);
        if (match.Success) return match.Groups[1].Value;
        return null;
    }

    /// <summary>
    ///     Hilfsfunktion: Zählt, wie oft ein bestimmtes Zeichen in einer Zeile vorkommt.
    /// </summary>
    /// <param name="line">Die Zeile, in der das Zeichen gezählt werden soll.</param>
    /// <param name="character">Das zu zählende Zeichen.</param>
    /// <returns>Anzahl der Vorkommen des Zeichens in der Zeile.</returns>
    private int CountOccurrences(string line, char character)
    {
        var count = 0;
        foreach (var c in line)
            if (c == character)
                count++;

        return count;
    }
}