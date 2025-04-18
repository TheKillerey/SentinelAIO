using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows.Input;
using SentinelAIO.Commands;

namespace SentinelAIO.ViewModel;

public class ManifestDownloaderViewModel : BaseViewModel
{
    private string _outputLog;
    private string _selectedPatchVersion;

    public ManifestDownloaderViewModel()
    {
        // Initialize PatchVersions list
        PatchVersions = new ObservableCollection<string>
        {
            "15.1",
            "15.2",
            "15.3",
            "15.4",
            "15.5",
            "15.6",
            "15.7",
            "15.8",
            "15.9"
        };

        // Initialize Commands
        DownloadManifestCommand = new RelayCommand(async param => await DownloadManifestAsync());
        DownloadFilesCommand = new RelayCommand(async param => await DownloadFilesAsync());
        ExtractFileCommand = new RelayCommand(async param => await ExtractFileAsync(param));
    }

    // List of available Patch Versions
    public ObservableCollection<string> PatchVersions { get; }

    public string SelectedPatchVersion
    {
        get => _selectedPatchVersion;
        set
        {
            _selectedPatchVersion = value;
            OnPropertyChanged();
        }
    }

    public string OutputLog
    {
        get => _outputLog;
        set
        {
            _outputLog = value;
            OnPropertyChanged();
        }
    }

    public ICommand DownloadManifestCommand { get; }
    public ICommand DownloadFilesCommand { get; }
    public ICommand ExtractFileCommand { get; }

    private async Task DownloadManifestAsync()
    {
        if (string.IsNullOrEmpty(SelectedPatchVersion))
        {
            AppendLog("Please select a patch version.");
            return;
        }

        var manifestUrl = GetManifestUrl(SelectedPatchVersion);

        if (string.IsNullOrEmpty(manifestUrl))
        {
            AppendLog($"Manifest URL not found for version {SelectedPatchVersion}");
            return;
        }

        var manifestsFolder = Path.Combine("Downloads", "ManifestFiles", SelectedPatchVersion);
        Directory.CreateDirectory(manifestsFolder);
        var manifestFile = Path.Combine(manifestsFolder, $"{SelectedPatchVersion}.manifest");

        try
        {
            using (var httpClient = new HttpClient())
            {
                AppendLog($"Downloading manifest file for version {SelectedPatchVersion}...");
                var data = await httpClient.GetByteArrayAsync(manifestUrl);
                await File.WriteAllBytesAsync(manifestFile, data);
                AppendLog($"Manifest downloaded: {manifestFile}");
            }
        }
        catch (HttpRequestException ex)
        {
            AppendLog($"Error downloading manifest: {ex.Message}");
        }
        catch (Exception ex)
        {
            AppendLog($"Unexpected error: {ex.Message}");
        }
    }

    private async Task DownloadFilesAsync()
    {
        if (string.IsNullOrEmpty(SelectedPatchVersion))
        {
            AppendLog("Please select a patch version.");
            return;
        }

        var manifestsFile = Path.Combine("Downloads", "ManifestFiles", SelectedPatchVersion,
            $"{SelectedPatchVersion}.manifest");
        var leagueFilesFolder = Path.Combine("Downloads", "LeagueFiles", SelectedPatchVersion);

        Directory.CreateDirectory(leagueFilesFolder);

        if (!File.Exists(manifestsFile))
        {
            AppendLog("Manifest file not found. Please download it first.");
            return;
        }

        var wadFileLocation = "DATA/FINAL/Maps/Shipping/Map22.wad.client";
        var rmanPath = "OtherTools/rman-dl.exe";
        var arguments = $"\"{manifestsFile}\" \"{leagueFilesFolder}\" -p \"{wadFileLocation}\"";

        AppendLog($"Downloading .wad file for {SelectedPatchVersion}...");

        try
        {
            await ExecuteCommandAsync(rmanPath, arguments); // Offload to background thread
            AppendLog("Download completed.");
        }
        catch (Exception ex)
        {
            AppendLog($"Error downloading files: {ex.Message}");
        }
    }

    private async Task ExtractFileAsync(object filePath)
    {
        if (filePath is string file && File.Exists(file) && file.EndsWith(".wad.client"))
        {
            var wadExtractPath = "OtherTools/hashes/wad-extract.exe";
            var arguments = $"\"{file}\"";

            AppendLog($"Extracting {file}...");

            try
            {
                await ExecuteCommandAsync(wadExtractPath, arguments); // Run external tool on background thread
                AppendLog("Extraction completed.");
            }
            catch (Exception ex)
            {
                AppendLog($"Error extracting file: {ex.Message}");
            }
        }
        else
        {
            AppendLog("Invalid file for extraction.");
        }
    }

    private async Task ExecuteCommandAsync(string executablePath, string arguments)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data)) AppendLog($"[Output] {args.Data}");
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data)) AppendLog($"[Info] {args.Data}");
            };

            AppendLog($"Starting process '{executablePath}' with arguments: {arguments}");

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.Run(() => process.WaitForExit());

            AppendLog($"Process exited with code {process.ExitCode}");
        }
        catch (Exception ex)
        {
            AppendLog($"Error executing command: {ex.Message}");
        }
    }

    private string GetManifestUrl(string patchVersion)
    {
        try
        {
            var filePath = Path.Combine("OtherTools", "ManifestFiles", $"{patchVersion}.txt");
            return File.Exists(filePath) ? File.ReadAllText(filePath).Trim() : null;
        }
        catch
        {
            return null;
        }
    }

    private void AppendLog(string message)
    {
        OutputLog += $"{DateTime.Now}: {message}{Environment.NewLine}";
        OnPropertyChanged(nameof(OutputLog));
    }
}