using System.Globalization;
using System.IO;
using System.Reflection;
using LeagueToolkit.Core.Meta;
using LeagueToolkit.Meta;
using LeagueToolkit.Meta.Attributes;
using LeagueToolkit.Toolkit.Ritobin;

namespace SentinelAIO.Library;

public class LtRitobinReader
{
    public static void RitobinWriter(string inputMaterialPath, string outputMaterialPath)
    {
        try
        {
            if (!File.Exists(inputMaterialPath))
            {
                Console.WriteLine($"Input file not found: {inputMaterialPath}");
                return;
            }

            using var binFile = File.OpenRead(inputMaterialPath);
            BinTree bin = new(binFile); // Parse the binary tree structure from the input file

            // Load meta-information from exported types
            var metaEnvironment = MetaEnvironment.Create(
                Assembly.Load("LeagueToolkit.Meta.Classes")
                    .GetExportedTypes()
                    .Where(type => type.IsClass)
            );

            try
            {
                // Define the path to the WAD hashes file
                var wadHashesPath = @"OtherTools/hashes/hashes.game.txt";

                // Create dictionaries for the writer
                Dictionary<string, string> objects = CreateObjectDictionary(bin);
                Dictionary<string, string> classes = CreateClassDictionary(metaEnvironment);
                Dictionary<string, string> properties = CreatePropertyDictionary(metaEnvironment);
                Dictionary<string, string> binHashes = CreateBinHashesDictionary(bin);
                Dictionary<string, string> wadHashes = CreateWadHashesDictionary(wadHashesPath);

                // Initialize the RitobinWriter
                using RitobinWriter writer = new(
                    ConvertKeysToUint(objects),
                    ConvertKeysToUint(classes),
                    ConvertKeysToUint(properties),
                    ConvertKeysToUint(binHashes),
                    ConvertWadKeysToULong(wadHashes)
                );

                // Write the `.bin` file data to the output path
                var result = writer.WritePropertyBin(bin);
                File.WriteAllText(outputMaterialPath, result);

                Console.WriteLine($"Successfully written to: {outputMaterialPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while processing dictionaries or writing output: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            // Handle errors gracefully
            Console.WriteLine($"Error in RitobinWriter: {ex.Message}");
        }
    }

    private static Dictionary<string, string> CreateObjectDictionary(BinTree bin)
    {
        var dictionary = new Dictionary<string, string>();
        foreach (var kvp in bin.Objects)
        {
            var hexKey = $"0x{kvp.Key:X}";
            try
            {
                if (!dictionary.ContainsKey(hexKey))
                    dictionary.Add(hexKey, $"Object_{hexKey}");
                else
                    Console.WriteLine($"Duplicate object key found: {hexKey}. Skipping.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while adding to object dictionary. Key: {hexKey}, Error: {ex.Message}");
            }
        }

        return dictionary;
    }

    private static Dictionary<string, string> CreateClassDictionary(MetaEnvironment metaEnvironment)
    {
        var dictionary = new Dictionary<string, string>();
        foreach (var entry in metaEnvironment.RegisteredMetaClasses)
        {
            var hexKey = $"0x{entry.Key:X}";
            try
            {
                if (!dictionary.ContainsKey(hexKey))
                    dictionary.Add(hexKey, entry.Value.Name ?? $"Class_{hexKey}");
                else
                    Console.WriteLine($"Duplicate class hash detected: {hexKey}. Using first occurrence.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while processing class dictionary. Key: {hexKey}, Error: {ex.Message}");
            }
        }

        return dictionary;
    }

    private static Dictionary<string, string> CreatePropertyDictionary(MetaEnvironment metaEnvironment)
    {
        var properties = new Dictionary<string, string>();

        foreach (var classType in metaEnvironment.RegisteredMetaClasses.Values)
        {
            var metaProperties = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in metaProperties)
            {
                var attribute = property.GetCustomAttribute<MetaPropertyAttribute>();
                if (attribute != null)
                {
                    var hexKey = $"0x{attribute.NameHash:X}";
                    if (!properties.ContainsKey(hexKey))
                        properties[hexKey] = property.Name;
                    else
                        Console.WriteLine($"Duplicate property hash detected: {hexKey}. Ignoring duplicate.");
                }
            }
        }

        return properties;
    }

    private static Dictionary<string, string> CreateBinHashesDictionary(BinTree bin)
    {
        var dictionary = new Dictionary<string, string>();
        foreach (var key in bin.Objects.Keys)
        {
            var hexKey = $"0x{key:X}";
            if (!dictionary.ContainsKey(hexKey))
                dictionary.Add(hexKey, $"{hexKey}");
            else
                Console.WriteLine($"Duplicate bin hash detected: {hexKey}. Skipping.");
        }

        return dictionary;
    }

    private static Dictionary<string, string> CreateWadHashesDictionary(string hashFilePath)
    {
        var dictionary = new Dictionary<string, string>();

        try
        {
            // Read the hash file
            var lines = File.ReadAllLines(hashFilePath);

            foreach (var line in lines)
            {
                // Split the line by the first space
                var parts = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var hash = parts[0].Trim();
                    var filePath = parts[1].Trim();

                    // Populate the dictionary
                    dictionary[hash] = filePath;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading WAD hashes file: {ex.Message}");
        }

        return dictionary;
    }

    private static Dictionary<uint, string> ConvertKeysToUint(Dictionary<string, string> hexDict)
    {
        var dictionary = new Dictionary<uint, string>();
        foreach (var kvp in hexDict)
            if (uint.TryParse(kvp.Key.TrimStart('0', 'x'), NumberStyles.HexNumber, null, out var parsedKey))
                dictionary[parsedKey] = kvp.Value;

        return dictionary;
    }

    private static Dictionary<ulong, string> ConvertWadKeysToULong(Dictionary<string, string> hexDict)
    {
        var ulongDict = new Dictionary<ulong, string>();

        foreach (var kvp in hexDict)
            if (ulong.TryParse(kvp.Key, NumberStyles.HexNumber, null, out var ulongKey))
                ulongDict[ulongKey] = kvp.Value;

        return ulongDict;
    }
}