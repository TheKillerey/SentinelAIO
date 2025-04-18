using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace SentinelAIO.Library;

public class FileExtractorAndUnhasher
{
    // Regex to extract "ASSETS" paths (acts as a fallback for objects containing asset-like strings)
    private static readonly Regex AssetsRegex = new(@"ASSETS\/[\w\/\.\-]+", RegexOptions.IgnoreCase);

    /// <summary>
    ///     Processes a binary file by extracting paths, generating hashes,
    ///     and unhashing files in a specified folder.
    /// </summary>
    /// <param name="inputBinaryFilePath">Path to the input binary file.</param>
    /// <param name="hashedFolder">Folder containing hashed files.</param>
    public void ProcessBinary(string inputBinaryFilePath, string hashedFolder)
    {
        // Step 1: Extract all "ASSETS" paths from the binary file
        var originalPaths = ExtractPathsFromBinary(inputBinaryFilePath);

        // Step 2: Generate a hash map from the extracted paths
        var hashMap = GenerateHashMap(originalPaths);

        // Step 3: Unhash files in the specified folder
        UnhashFiles(hashedFolder, hashMap);
    }

    /// <summary>
    ///     Extracts "ASSETS" paths from a binary file.
    /// </summary>
    /// <param name="binaryFilePath">Path to the binary file.</param>
    /// <returns>List of extracted paths.</returns>
    private List<string> ExtractPathsFromBinary(string binaryFilePath)
    {
        var extractedPaths = new List<string>();

        try
        {
            // Read the file line by line
            foreach (var line in File.ReadLines(binaryFilePath))
            {
                // Match paths containing "ASSETS" using the regex
                var matches = AssetsRegex.Matches(line);
                foreach (Match match in matches)
                    if (match.Success)
                    {
                        var path = match.Value.ToLower(); // Normalize to lowercase
                        extractedPaths.Add(path);
                    }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to process binary file: {ex.Message}");
        }

        return extractedPaths;
    }

    /// <summary>
    ///     Generates a hash map of asset paths to their respective hashes.
    /// </summary>
    /// <param name="originalPaths">List of original asset paths.</param>
    /// <returns>A dictionary mapping hash values to their original paths.</returns>
    private Dictionary<string, string> GenerateHashMap(List<string> originalPaths)
    {
        var hashMap = new Dictionary<string, string>();

        foreach (var path in originalPaths)
        {
            // Generate hash as a hexadecimal string
            var hashHex = Fnv1aHash(path).ToString("x");
            if (!hashMap.ContainsKey(hashHex)) hashMap[hashHex] = path;
        }

        return hashMap;
    }

    /// <summary>
    ///     Unhashes files in the specified folder using a hash map.
    /// </summary>
    /// <param name="hashedFolder">Folder containing hashed files.</param>
    /// <param name="hashMap">Hash map of file paths.</param>
    private void UnhashFiles(string hashedFolder, Dictionary<string, string> hashMap)
    {
        foreach (var filePath in Directory.GetFiles(hashedFolder, "*.*", SearchOption.AllDirectories))
        {
            // Extract the hashed filename (ignore extensions for filenames)
            var fileName = Path.GetFileNameWithoutExtension(filePath).ToLower(); // Ensure lowercase

            // Check if the filename exists in the hash map
            if (hashMap.TryGetValue(fileName.Trim().ToLower(), out var originalPath))
                try
                {
                    // Adjust the original path relative to "ASSETS/"
                    var relativePath =
                        originalPath.Substring(originalPath.IndexOf("ASSETS", StringComparison.OrdinalIgnoreCase));
                    var originalFilePath = Path.Combine(hashedFolder, relativePath).Replace("/", "\\");

                    Directory.CreateDirectory(Path.GetDirectoryName(originalFilePath) ?? "");
                    File.Move(filePath, originalFilePath); // Move file to original path
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while moving file: {filePath}, Error: {ex.Message}");
                }
        }
    }

    /// <summary>
    ///     Implements a custom ELF hash function.
    /// </summary>
    private ulong ElfHash(string s)
    {
        ulong h = 0;
        foreach (var c in s.ToLower())
        {
            h = (h << 4) + c;
            var high = h & 0xF0000000;
            if (high != 0) h ^= high >> 24;
            h &= ~high;
        }

        return h;
    }

    /// <summary>
    ///     Implements a custom FNV1 hash function.
    /// </summary>
    private ulong Fnv1Hash(string s)
    {
        ulong h = 0x811c9dc5;
        foreach (var b in s.ToLower().ToCharArray().Select(c => (byte)c)) h = (h * 0x01000193) ^ b;
        return h;
    }

    /// <summary>
    ///     Implements a custom FNV1a hash function.
    /// </summary>
    private ulong Fnv1aHash(string s)
    {
        ulong h = 0x811c9dc5;
        foreach (var b in s.ToLower().ToCharArray().Select(c => (byte)c))
        {
            h ^= b;
            h *= 0x01000193;
        }

        return h;
    }

    /// <summary>
    ///     Validates whether a string is a valid 16-character hexadecimal hash.
    /// </summary>
    /// <param name="fileName">File name to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    private bool IsValidHash(string fileName)
    {
        return fileName.Length == 16 && ulong.TryParse(fileName, NumberStyles.HexNumber, null, out _);
    }
}