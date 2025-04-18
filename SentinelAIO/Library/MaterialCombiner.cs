using System.IO;

namespace SentinelAIO.Library;

public class MaterialCombiner
{
    /// <summary>
    ///     Kombiniert alle .py-Dateien in einem Verzeichnis in eine einzige Datei.
    /// </summary>
    /// <param name="inputFolderPath">Pfad zum Ordner, der die einzelnen .py-Dateien enthält.</param>
    /// <param name="outputFilePath">Pfad zur zentralen Ausgabe-Datei.</param>
    public void CombineMaterials(string inputFolderPath, string outputFilePath)
    {
        // Sicherstellen, dass der Eingabeordner existiert
        if (!Directory.Exists(inputFolderPath))
            throw new DirectoryNotFoundException($"Das Verzeichnis '{inputFolderPath}' wurde nicht gefunden.");

        // Alle .py-Dateien im Ordner finden
        string[] files = Directory.GetFiles(inputFolderPath, "*.py", SearchOption.TopDirectoryOnly);
        if (files.Length == 0)
            throw new FileNotFoundException("Es wurden keine .py-Dateien im angegebenen Verzeichnis gefunden.");

        Console.WriteLine($"Gefundene Dateien: {files.Length}");

        // Inhalte der Dateien kombinieren
        using (var writer = new StreamWriter(outputFilePath))
        {
            foreach (var file in files)
            {
                // Dateiname anzeigen (optional, für Debugging-Zwecke)
                Console.WriteLine($"Füge Datei hinzu: {file}");

                // Dateiinhalt lesen
                var content = File.ReadAllText(file);

                // Dateiinhalt in die zentrale Datei schreiben
                writer.WriteLine(content);
                writer.WriteLine(); // Leerzeile einfügen
            }
        }

        Console.WriteLine($"Die Dateien wurden erfolgreich in '{outputFilePath}' kombiniert.");
    }
}