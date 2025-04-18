using System.IO;
using System.Text.RegularExpressions;

namespace SentinelAIO.Library;

public class MaterialExtractor
{
    /// <summary>
    ///     Extracts complete material blocks from files in a folder
    ///     and exports them either to a single file or separate .py files.
    /// </summary>
    /// <param name="inputFolderPath">The folder path to search for files.</param>
    /// <param name="outputFilePathOrFolder">Output file path (.txt) for a single file or folder path for multiple files.</param>
    /// <param name="exportToSeparateFiles">True to export blocks to separate files; False for a single output file.</param>
    public void ExtractAndExportMaterials(string inputFolderPath, string outputFilePathOrFolder,
        bool exportToSeparateFiles)
    {
        // Locate all files in the folder and its subfolders
        string[] inputFiles = Directory.GetFiles(inputFolderPath, "*.py", SearchOption.AllDirectories);
        Console.WriteLine($"Found files: {inputFiles.Length}");

        // List to store extracted material blocks
        List<string> materialBlocks = new();

        // Process each file
        foreach (var inputFile in inputFiles) materialBlocks.AddRange(ExtractMaterialBlocksFromFile(inputFile));

        // Export material blocks
        if (exportToSeparateFiles)
            ExportMaterialsToSeparateFiles(materialBlocks, outputFilePathOrFolder);
        else
            ExportMaterialsToSingleFile(materialBlocks, outputFilePathOrFolder);
    }

    /// <summary>
    ///     Extracts material blocks from a single file.
    /// </summary>
    /// <param name="filePath">Path to the file to process.</param>
    /// <returns>List of material blocks as strings.</returns>
    private List<string> ExtractMaterialBlocksFromFile(string filePath)
    {
        List<string> materialBlocks = new();

        // Read file content line by line
        string[] lines = File.ReadAllLines(filePath);

        // Regex to identify the start of a material block
        var materialStartRegex = new Regex(@"^\s*"".*StaticMaterialDef\s*\{");

        List<string> currentBlock = new();
        var insideMaterial = false;
        var openBracesCount = 0;

        foreach (var line in lines)
        {
            // Detect the start of a material block
            if (!insideMaterial && materialStartRegex.IsMatch(line))
            {
                insideMaterial = true; // Start block detection
                openBracesCount = 0; // Reset the braces counter
            }

            if (insideMaterial)
            {
                // Add the line to the current block
                currentBlock.Add(line);

                // Count braces
                openBracesCount += CountOccurrences(line, '{');
                openBracesCount -= CountOccurrences(line, '}');

                // Block is complete when all braces are closed
                if (openBracesCount == 0)
                {
                    insideMaterial = false; // End of block
                    materialBlocks.Add(string.Join(Environment.NewLine, currentBlock));
                    currentBlock.Clear(); // Reset for next block
                }
            }
        }

        return materialBlocks;
    }

    /// <summary>
    ///     Exports all material blocks to a single file.
    /// </summary>
    /// <param name="materialBlocks">List of material blocks.</param>
    /// <param name="outputFilePath">Path to the output file.</param>
    private void ExportMaterialsToSingleFile(List<string> materialBlocks, string outputFilePath)
    {
        File.WriteAllLines(outputFilePath, materialBlocks);
        Console.WriteLine($"All material blocks exported to the file: {outputFilePath}.");
    }

    /// <summary>
    ///     Exports each material block to a separate file using its name as the filename.
    /// </summary>
    /// <param name="materialBlocks">List of material blocks.</param>
    /// <param name="outputFolder">Target folder for the output files.</param>
    private void ExportMaterialsToSeparateFiles(List<string> materialBlocks, string outputFolder)
    {
        if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

        for (var i = 0; i < materialBlocks.Count; i++)
        {
            // Extract material name
            var materialName = ExtractMaterialName(materialBlocks[i]);

            // Fallback if no name is found
            if (string.IsNullOrEmpty(materialName)) materialName = $"Material_{i + 1}";

            // Ensure the filename is safe
            materialName = MakeFileNameSafe(materialName);

            var fileName = $"{materialName}.py";
            var outputFilePath = Path.Combine(outputFolder, fileName);
            File.WriteAllText(outputFilePath, materialBlocks[i]);
        }

        Console.WriteLine("Material blocks exported to separate files.");
    }

    /// <summary>
    ///     Extracts the material name from a material block.
    /// </summary>
    /// <param name="materialBlock">The material block text.</param>
    /// <returns>The material name or null if not found.</returns>
    private string ExtractMaterialName(string materialBlock)
    {
        // Match for material name: 'name: string = "<material_name>"'
        var nameRegex = new Regex(@"name:\s*string\s*=\s*""(.*?)""");

        var match = nameRegex.Match(materialBlock);
        if (match.Success) return match.Groups[1].Value;

        return null;
    }

    /// <summary>
    ///     Replaces invalid characters in filenames with '_'.
    /// </summary>
    /// <param name="fileName">The original filename.</param>
    /// <returns>A safe filename.</returns>
    private string MakeFileNameSafe(string fileName)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars()) fileName = fileName.Replace(invalidChar, '_');
        return fileName;
    }

    /// <summary>
    ///     Utility function to count occurrences of a specific character in a line.
    /// </summary>
    /// <param name="line">The line to process.</param>
    /// <param name="character">The character to count.</param>
    /// <returns>Number of occurrences of the character in the line.</returns>
    private int CountOccurrences(string line, char character)
    {
        var count = 0;
        foreach (var c in line)
            if (c == character)
                count++;

        return count;
    }
}