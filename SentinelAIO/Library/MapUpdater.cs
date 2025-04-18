using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace SentinelAIO.Library;

public class MapUpdater
{
    /// <summary>
    ///     Updates the map file for the given map ID.
    /// </summary>
    /// <param name="projectFolderPath">Path to the project folder.</param>
    /// <param name="mapId">Target map ID (e.g., "map11" or "map12").</param>
    /// <param name="replacementValues">Custom replacement values for patterns.</param>
    public static void UpdateMapFile(string projectFolderPath, string mapId, MapReplacementValues replacementValues)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(projectFolderPath) || !Directory.Exists(projectFolderPath) ||
            string.IsNullOrWhiteSpace(mapId)) return;

        // File paths
        var targetFolder =
            Path.Combine(projectFolderPath, mapId.ToUpper(), "DATA", "Maps", "shipping", mapId.ToLower());
        var binFilePath = Path.Combine(targetFolder, $"{mapId.ToLower()}.bin");
        var pyFilePath = Path.Combine(targetFolder, $"{mapId.ToLower()}.py");
        var backupFilePath = Path.Combine(targetFolder, $"{mapId.ToLower()}_backup.bin");

        // Ensure the directory structure exists
        Directory.CreateDirectory(targetFolder);

        // Backup the existing .bin file
        if (File.Exists(binFilePath))
            File.Copy(binFilePath, backupFilePath, true);
        else
            return;

        // Convert the .bin file to .py using ritobin
        if (!RunRitobinConversion(binFilePath, pyFilePath, false)) return;

        // Read, modify, and save the .py file
        if (File.Exists(pyFilePath))
        {
            var fileContent = File.ReadAllText(pyFilePath);

            // Perform replacements using custom patterns
            fileContent = Regex.Replace(fileContent, @"mMapContainerLink: string = ""[^""]*""",
                $@"mMapContainerLink: string = ""{replacementValues.MapContainer}""");
            fileContent = Regex.Replace(fileContent, @"mMapObjectsCFG: string = ""[^""]*""",
                $@"mMapObjectsCFG: string = ""{replacementValues.ObjectsCFG}""");
            fileContent = Regex.Replace(fileContent, @"mWorldParticlesINI: string = ""[^""]*""",
                $@"mWorldParticlesINI: string = ""{replacementValues.ParticlesINI}""");
            fileContent = Regex.Replace(fileContent, @"mGrassTintTexture: string = ""[^""]*""",
                $@"mGrassTintTexture: string = ""{replacementValues.GrassTintTexture}""");

            File.WriteAllText(pyFilePath, fileContent);
        }
        else
        {
            return;
        }

        // Convert the updated .py file back to .bin
        if (!RunRitobinConversion(pyFilePath, binFilePath, true)) return;

        // Clean up - Delete all .py files
        File.Delete(pyFilePath);
    }

    /// <summary>
    ///     Helper function to run ritobin for converting between bin and py.
    /// </summary>
    /// <param name="inputPath">Input file path.</param>
    /// <param name="outputPath">Output file path.</param>
    /// <param name="isConvertingToBin">Set to true if converting .py to .bin, false for .bin to .py.</param>
    /// <returns>Returns true if the process succeeds, otherwise false.</returns>
    private static bool RunRitobinConversion(string inputPath, string outputPath, bool isConvertingToBin)
    {
        try
        {
            var inputFormat = isConvertingToBin ? "text" : "bin";
            var outputFormat = isConvertingToBin ? "bin" : "text";

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "OtherTools/ritobin_cli.exe",
                Arguments = $"-i {inputFormat} -o {outputFormat} \"{inputPath}\" \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            process?.WaitForExit();

            return process != null && process.ExitCode == 0;
        }
        catch
        {
            // Handle with minimal interference; no need for console output
            return false;
        }
    }
}

/// <summary>
///     Represents the customizable replacement values for the map updater.
/// </summary>
public class MapReplacementValues
{
    public string MapContainer { get; set; } = "Maps/MapGeometry/Map*/MapName";
    public string ObjectsCFG { get; set; } = "ObjectCFG_Default.cfg";
    public string ParticlesINI { get; set; } = "Particles_Default.ini";
    public string GrassTintTexture { get; set; } = "Grasstint_Default.dds";
}