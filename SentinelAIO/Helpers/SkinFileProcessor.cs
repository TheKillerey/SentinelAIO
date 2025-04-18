using System.Diagnostics;
using System.IO;

namespace SentinelAIO.Helpers;

public class SkinFileProcessor
{
    /// <summary>
    ///     Converts a .bin file to a .py file and returns its content.
    /// </summary>
    public static async Task<string> ConvertBinToPyAsync(string binFilePath)
    {
        var pyFilePath = Path.ChangeExtension(binFilePath, ".py");

        if (File.Exists(pyFilePath) && File.GetLastWriteTime(pyFilePath) >= File.GetLastWriteTime(binFilePath))
            return await File.ReadAllTextAsync(pyFilePath);

        await RunRitobinAsync(binFilePath, pyFilePath, false);
        return await File.ReadAllTextAsync(pyFilePath);
    }

    /// <summary>
    ///     Converts a .bin file to a .py file without reading the content of the generated .py file.
    /// </summary>
    public static async Task ConvertBinToPyWithoutReadAsync(string binFilePath)
    {
        var pyFilePath = Path.ChangeExtension(binFilePath, ".py");

        // Skip conversion if the .py file is already up-to-date.
        if (File.Exists(pyFilePath) && File.GetLastWriteTime(pyFilePath) >= File.GetLastWriteTime(binFilePath))
            return;

        // Run the conversion process
        await RunRitobinAsync(binFilePath, pyFilePath, false);
    }

    /// <summary>
    ///     Converts and saves a .py file back to the original .bin format.
    /// </summary>
    public static async Task SavePyToBinAsync(string pyFilePath, string originalBinPath)
    {
        await RunRitobinAsync(pyFilePath, originalBinPath, true);
        //File.Delete(pyFilePath);
    }

    /// <summary>
    ///     Performs the conversion using an external CLI tool.
    /// </summary>
    private static async Task RunRitobinAsync(string inputPath, string outputPath, bool isConvertingToBin)
    {
        var inputFormat = isConvertingToBin ? "text" : "bin";
        var outputFormat = isConvertingToBin ? "bin" : "text";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "OtherTools/ritobin_cli.exe",
                Arguments = $"-i {inputFormat} -o {outputFormat} \"{inputPath}\" \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Ritobin conversion failed: {error}");
        }
    }
}