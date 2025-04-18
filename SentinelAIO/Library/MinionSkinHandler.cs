using System.IO;
using System.Text.RegularExpressions;
using SentinelAIO.Helpers;

namespace SentinelAIO.Library;

public class MinionSkinHandler
{
    private readonly string _projectFolder;

    public MinionSkinHandler(string projectFolder)
    {
        _projectFolder = projectFolder ?? throw new ArgumentNullException(nameof(projectFolder));
    }

    /// <summary>
    ///     Loads all skins for a given champion asynchronously.
    /// </summary>
    public async Task<SkinFile[]> LoadChampionSkinsAsync(string championName)
    {
        if (string.IsNullOrEmpty(championName))
            throw new ArgumentException("Champion name cannot be null or empty.", nameof(championName));

        // Locate all possible Map folders under the project directory
        var mapFolders = Directory.GetDirectories(_projectFolder, "Map*", SearchOption.TopDirectoryOnly);

        string skinsPath = null;

        // Search for the correct Skins folder in any MapX folder
        foreach (var mapFolder in mapFolders)
        {
            var potentialSkinsPath = Path.Combine(mapFolder, "DATA", "Characters", championName, "Skins");

            if (Directory.Exists(potentialSkinsPath))
            {
                skinsPath = potentialSkinsPath;
                break; // Stop searching once the directory is found
            }
        }

        if (skinsPath == null)
            throw new DirectoryNotFoundException(
                $"Champion skin folder for '{championName}' not found in any MapX directory.");

        // Collect all BIN files from the resolved path and sort them
        var skinFiles = Directory.GetFiles(skinsPath, "*.bin")
            .OrderBy(SortSkinFiles);

        // Process each skin file asynchronously and return the results
        return await Task.WhenAll(skinFiles.Select(ProcessSkinFileAsync));
    }

    public async Task<string[]> LoadChampionsAsync()
    {
        // Locate all potential map folders containing DATA/Characters
        var potentialMapPaths = Directory.GetDirectories(_projectFolder, "Map*", SearchOption.TopDirectoryOnly);

        // Initialize a list to collect valid characters directories
        var characterPaths = new List<string>();

        foreach (var mapPath in potentialMapPaths)
        {
            var charactersPath = Path.Combine(mapPath, "DATA", "Characters");

            if (Directory.Exists(charactersPath)) characterPaths.Add(charactersPath); // Add valid characters folder
        }

        if (!characterPaths.Any())
            throw new DirectoryNotFoundException($"No valid Characters directory found under: {_projectFolder}");

        // Collect all unique champion folder names under all characters directories
        var champions = await Task.Run(() =>
            characterPaths.SelectMany(path => Directory.GetDirectories(path)
                    .Select(Path.GetFileName)) // Get folder names (champions)
                .Distinct() // Avoid duplicates across multiple Maps
                .ToArray()
        );

        return champions;
    }

    /// <summary>
    ///     Loads the full content of the skin file and extracts the `skinMeshProperties` block for UI editing.
    /// </summary>
    public async Task<(string FullFileContent, string SkinMeshProperties)> LoadSkinPropertiesAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("Skin file not found!", filePath);

        // Load the full content of the binary file as .py content
        var fullContent = await SkinFileProcessor.ConvertBinToPyAsync(filePath);

        // Extract the `skinMeshProperties` block
        var skinMeshProperties = ExtractSkinMeshBlock(fullContent);
        return (fullContent, skinMeshProperties);
    }

    /// <summary>
    ///     Copies the skinMeshProperties block from a source file and pastes it into the target file.
    /// </summary>
    public async Task CopyAndPasteSkinMeshPropertiesAsync(string sourceFilePath, string targetFilePath)
    {
        if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(targetFilePath))
            throw new ArgumentException("Source or target file path cannot be null or empty.");

        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException("Source file not found!", sourceFilePath);

        if (!File.Exists(targetFilePath))
            throw new FileNotFoundException("Target file not found!", targetFilePath);

        // Load the .py content directly from the source file
        var sourceContent = await File.ReadAllTextAsync(sourceFilePath);

        // Extract the skinMeshProperties block from the source content
        var sourceSkinMeshProperties = ExtractSkinMeshBlock(sourceContent);

        // Load the .py content of the target file
        var targetContent = await File.ReadAllTextAsync(targetFilePath);

        // Replace the skinMeshProperties block in the target file
        var modifiedTargetContent = ReplaceSkinMeshPropertiesBlock(targetContent, sourceSkinMeshProperties);

        // Update the .py file directly (no need to create temporary files)
        await File.WriteAllTextAsync(targetFilePath, modifiedTargetContent);

        // Convert the updated .py file back to its binary (.bin) format
        await SkinFileProcessor.SavePyToBinAsync(targetFilePath, Path.ChangeExtension(targetFilePath, ".bin"));
    }

    /// <summary>
    ///     Saves the modified skinMeshProperties block to the corresponding binary file while preserving other file content.
    /// </summary>
    public async Task SaveSkinProperties(string filePath, string updatedSkinMeshProperties)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        if (string.IsNullOrEmpty(updatedSkinMeshProperties))
            throw new InvalidDataException("Updated skinMeshProperties block cannot be null or empty!");

        // Ensure the file exists before proceeding
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found!", filePath);

        // Convert the .bin file to its editable .py format
        var originalContent = await SkinFileProcessor.ConvertBinToPyAsync(filePath);

        // Find and replace the skinMeshProperties block
        var modifiedContent = ReplaceSkinMeshPropertiesBlock(originalContent, updatedSkinMeshProperties);

        // Write the modified content to a temporary .py file for conversion
        var pyFilePath = Path.ChangeExtension(filePath, ".py");
        await File.WriteAllTextAsync(pyFilePath, modifiedContent);

        // Convert the updated .py content back to the binary format
        await SkinFileProcessor.SavePyToBinAsync(pyFilePath, filePath);
    }

    /// <summary>
    ///     Replaces the existing skinMeshProperties block in the original content with the updated one.
    /// </summary>
    private static string ReplaceSkinMeshPropertiesBlock(string originalContent, string newSkinMeshProperties)
    {
        // Regex to match the existing skinMeshProperties block
        var regex = new Regex(@"skinMeshProperties\s*:\s*embed\s*=\s*SkinMeshDataProperties\s*\{.*?\}",
            RegexOptions.Singleline);

        if (!regex.IsMatch(originalContent))
            throw new InvalidOperationException(
                "Cannot replace skinMeshProperties block: No matching block found in the original file.");

        // Replace the old block with the new one
        return regex.Replace(originalContent, newSkinMeshProperties);
    }

    /// <summary>
    ///     Processes a single skin file to extract relevant properties for display.
    /// </summary>
    private async Task<SkinFile> ProcessSkinFileAsync(string filePath)
    {
        var content = await SkinFileProcessor.ConvertBinToPyAsync(filePath);
        var skinProperties = ExtractSkinProperties(content);
        return new SkinFile
        {
            FileName = Path.GetFileName(filePath),
            SkinValue = skinProperties
        };
    }

    /// <summary>
    ///     Extracts the `skinMeshProperties` block, ensuring proper handling of deeply nested braces.
    /// </summary>
    private static string ExtractSkinMeshBlock(string content)
    {
        // Locate the start of the `skinMeshProperties` block using an improved regular expression
        var match = Regex.Match(content, @"skinMeshProperties\s*:\s*embed\s*=\s*SkinMeshDataProperties\s*\{",
            RegexOptions.Singleline);

        if (!match.Success)
            throw new InvalidOperationException(
                "Failed to locate the skinMeshProperties block. Ensure the file contains a valid `skinMeshProperties` block.");

        // Start parsing the block from the match position
        var startIndex = match.Index;
        var currentIndex = match.Index + match.Length;

        var openBraces = 1; // Track the number of open braces ('{')

        // Traverse the file content character by character
        while (currentIndex < content.Length)
        {
            if (content[currentIndex] == '{') openBraces++;
            if (content[currentIndex] == '}') openBraces--;

            // If braces balance out, we've found the end of the block
            if (openBraces == 0) break;

            currentIndex++;
        }

        if (openBraces != 0)
            throw new InvalidOperationException(
                "Unbalanced braces detected in the skinMeshProperties block. Check the file content for any structural issues.");

        // Return the extracted block
        return content.Substring(startIndex, currentIndex - startIndex + 1);
    }

    /// <summary>
    ///     Extracts skin properties from the .py content.
    /// </summary>
    private static string ExtractSkinProperties(string content)
    {
        var match = Regex.Match(content, @"skinMeshProperties\s*:\s*embed\s*=\s*SkinMeshDataProperties\s*\{.*?}",
            RegexOptions.Singleline);
        return match.Success
            ? match.Value
            : throw new InvalidOperationException("Failed to extract skin properties.");
    }

    /// <summary>
    ///     Sorts skin files based on numeric suffix in the file name (e.g., skin1, skin2).
    /// </summary>
    private static int SortSkinFiles(string path)
    {
        var match = Regex.Match(Path.GetFileNameWithoutExtension(path), @"skin(\d+)");
        return match.Success
            ? int.Parse(match.Groups[1].Value)
            : int.MaxValue;
    }
}

/// <summary>
///     Represents a skin file with its properties.
/// </summary>
public class SkinFile
{
    public string FileName { get; set; }
    public string SkinValue { get; set; }
}