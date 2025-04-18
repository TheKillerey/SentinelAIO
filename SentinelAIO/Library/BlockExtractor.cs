using System.IO;
using System.Text.RegularExpressions;

namespace SentinelAIO.Library;

public class BlockExtractor
{
    /// <summary>
    ///     Extracts all blocks from a file or folder of .py files and exports them into a combined file or separate files.
    /// </summary>
    /// <param name="inputFolderPath">Folder path to search .py files in.</param>
    /// <param name="outputFilePathOrFolder">Path of the export file (single file mode) or folder (separate file mode).</param>
    /// <param name="exportToSeparateFiles">True: Export each block as a separate file. False: Aggregate into one file.</param>
    public void ExtractAndExportBlocks(string inputFolderPath, string outputFilePathOrFolder,
        bool exportToSeparateFiles)
    {
        // Find all .py files in the folder and its subfolders
        string[] inputFiles = Directory.GetFiles(inputFolderPath, "*.py", SearchOption.AllDirectories);
        Console.WriteLine($"Found files: {inputFiles.Length}");

        // Store all parsed blocks
        List<string> allBlocks = new();

        // Process each file to extract blocks
        foreach (var inputFile in inputFiles) allBlocks.AddRange(ExtractBlocksFromFile(inputFile));

        // Export blocks to the desired location
        if (exportToSeparateFiles)
            ExportBlocksToSeparateFiles(allBlocks, outputFilePathOrFolder);
        else
            ExportBlocksToSingleFile(allBlocks, outputFilePathOrFolder);
    }

    /// <summary>
    ///     Extracts all brace-enclosed blocks ({ ... }) from a single file.
    /// </summary>
    /// <param name="filePath">The file to process.</param>
    /// <returns>A list of all blocks as strings.</returns>
    private List<string> ExtractBlocksFromFile(string filePath)
    {
        List<string> blocks = new();

        // Read the file line by line
        string[] lines = File.ReadAllLines(filePath);

        // Use a stack to track nested blocks
        List<string> currentBlock = new();
        var isInsideBlock = false;
        var openBraceCount = 0;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Detect the start of a block (looking for the first '{')
            if (!isInsideBlock && trimmedLine.Contains("{"))
            {
                isInsideBlock = true; // Block starts here
                openBraceCount = 0; // Reset brace count
            }

            if (isInsideBlock)
            {
                // Add line to current block
                currentBlock.Add(line);

                // Count braces
                openBraceCount += CountOccurrences(line, '{');
                openBraceCount -= CountOccurrences(line, '}');

                // A complete block is detected when braces are balanced
                if (openBraceCount == 0)
                {
                    // Save the block
                    blocks.Add(string.Join(Environment.NewLine, currentBlock));
                    currentBlock.Clear();
                    isInsideBlock = false; // Reset for next block
                }
            }
        }

        return blocks;
    }

    /// <summary>
    ///     Writes all extracted blocks into a single output file.
    /// </summary>
    /// <param name="blocks">The list of extracted blocks.</param>
    /// <param name="outputFilePath">Output file path.</param>
    private void ExportBlocksToSingleFile(List<string> blocks, string outputFilePath)
    {
        File.WriteAllLines(outputFilePath, blocks);
        Console.WriteLine($"All blocks have been exported to the file {outputFilePath}.");
    }

    /// <summary>
    ///     Writes each extracted block into a separate file.
    /// </summary>
    /// <param name="blocks">The list of extracted blocks.</param>
    /// <param name="outputFolder">The folder path for the output files.</param>
    private void ExportBlocksToSeparateFiles(List<string> blocks, string outputFolder)
    {
        // Ensure the output folder exists
        if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

        for (var i = 0; i < blocks.Count; i++)
        {
            // Extract a base name/key from the block
            var blockKey = ExtractBlockKey(blocks[i]) ?? $"Block_{i + 1}";

            // Sanitize block key for file use
            blockKey = MakeFileNameSafe(blockKey);

            // Write block to file
            var outputFilePath = Path.Combine(outputFolder, $"{blockKey}.py");
            File.WriteAllText(outputFilePath, blocks[i]);
        }

        Console.WriteLine("Blocks have been exported into separate files.");
    }

    /// <summary>
    ///     Attempts to extract a block name or key for use in the file name.
    /// </summary>
    /// <param name="block">The block content.</param>
    /// <returns>The extracted key or null if none found.</returns>
    private string ExtractBlockKey(string block)
    {
        // Use regex to extract a name from the block (e.g., from `name: string = "..."`)
        var nameRegex = new Regex(@"name:\s*string\s*=\s*""(.*?)""");
        var match = nameRegex.Match(block);

        if (match.Success) return match.Groups[1].Value;

        // No name found
        return null;
    }

    /// <summary>
    ///     Replaces invalid characters in a file name with '_'.
    /// </summary>
    /// <param name="fileName">The original file name.</param>
    /// <returns>A safe file name.</returns>
    private string MakeFileNameSafe(string fileName)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars()) fileName = fileName.Replace(invalidChar, '_');
        return fileName;
    }

    /// <summary>
    ///     Counts the occurrences of a specific character in a string.
    /// </summary>
    /// <param name="line">The string to search.</param>
    /// <param name="character">The character to count.</param>
    /// <returns>The number of occurrences.</returns>
    private int CountOccurrences(string line, char character)
    {
        var count = 0;
        foreach (var c in line)
            if (c == character)
                count++;

        return count;
    }
}