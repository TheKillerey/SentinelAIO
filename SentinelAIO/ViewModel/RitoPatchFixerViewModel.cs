using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ImageMagick;
using LeagueToolkit.Hashing;
using Microsoft.Win32;
using SentinelAIO.Commands;

namespace SentinelAIO.ViewModel;

public class RitoPatchFixerViewModel : BaseViewModel
{
    private static readonly HttpClient HttpClient = new();

    private readonly ParallelOptions options = new()
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount / 2 // Half of CPU cores
    };

    // Checkbox: Fix Missing Textures
    private bool _fixMissingTextures;

    private bool _fixWrongTextureFormat;
    private string _logText;
    private ObservableCollection<ModFileItem> _modFiles;

    private string _selectedMode = "None"; // Default mode

    // Combobox: Selected Patch Number
    private string _selectedPatchNumber;

    private bool _useDownloadedMap11Files;

    public RitoPatchFixerViewModel()
    {
        // Initialize properties
        SelectedMode = "None"; // Default mode
        ModFiles = new ObservableCollection<ModFileItem>();
        CommunityDragonPatches = new ObservableCollection<string>
        {
            "15.1",
            "15.2",
            "15.3",
            "15.4",
            "15.5",
            "15.6",
            "latest",
            "pbe"
        };

        // Initialize commands
        LoadFilesCommand = new RelayCommand(LoadFiles);
        RunCommand = new RelayCommand(async param => await RunFixerAsync());
        RawProjectCommand = new RelayCommand(async param => await HandleRawProjectAsync());
    }

    public ObservableCollection<ModFileItem> ModFiles
    {
        get => _modFiles;
        set
        {
            _modFiles = value;
            OnPropertyChanged();
        }
    }

    public bool UseDownloadedMap11Files
    {
        get => _useDownloadedMap11Files;
        set
        {
            _useDownloadedMap11Files = value;
            OnPropertyChanged();
        }
    }

    public string LogText
    {
        get => _logText;
        set
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _logText = value;
                OnPropertyChanged();
            });
        }
    }

    public bool FixWrongTextureFormat
    {
        get => _fixWrongTextureFormat;
        set
        {
            _fixWrongTextureFormat = value;
            OnPropertyChanged();
        }
    }

    public string SelectedMode
    {
        get => _selectedMode;
        set
        {
            _selectedMode = value;
            OnPropertyChanged();
        }
    }

    public bool FixMissingTextures
    {
        get => _fixMissingTextures;
        set
        {
            _fixMissingTextures = value;
            OnPropertyChanged();
        }
    }

    public string SelectedPatchNumber
    {
        get => _selectedPatchNumber;
        set
        {
            _selectedPatchNumber = value;
            OnPropertyChanged();
        }
    }

    // List of available patch numbers for Community Dragon
    public ObservableCollection<string> CommunityDragonPatches { get; }

    public ICommand LoadFilesCommand { get; }
    public ICommand RawProjectCommand { get; }

    public ICommand RunCommand { get; }

    private void LoadFiles(object parameter)
    {
        var cslolPath = GetCslolManagerPath();
        if (cslolPath == null)
        {
            AppendLog("CSLoL Manager is not running. Please start it and try again.");
            return;
        }

        var managerPath = Path.GetDirectoryName(cslolPath);
        var modsDirectory = Path.Combine(managerPath ?? "", "installed");

        if (!Directory.Exists(modsDirectory))
        {
            AppendLog("Mod directory not found!");
            return;
        }

        ModFiles.Clear();
        var modFolders = Directory.GetDirectories(modsDirectory).ToList();

        foreach (var folder in modFolders) ModFiles.Add(new ModFileItem { FileName = folder, IsSelected = false });

        AppendLog($"Loaded {modFolders.Count} mod folders.");
    }

    private async Task HandleRawProjectAsync()
    {
        AppendLog("Starting RAW project handling...");

        try
        {
            var rawFolderPath = PromptUserForFolderSelection();

            if (string.IsNullOrWhiteSpace(rawFolderPath))
            {
                AppendLog("No folder selected.");
                return;
            }

            AppendLog($"Selected folder: {rawFolderPath}");
            await ProcessRawProjectFolder(rawFolderPath);

            AppendLog("RAW handling completed.");
        }
        catch (Exception ex)
        {
            AppendLog($"Error: {ex.Message}");
        }
    }

    private async Task ProcessRawProjectFolder(string rawFolderPath)
    {
        AppendLog("Processing raw project folder...");

        // Ensure the folder exists
        if (string.IsNullOrWhiteSpace(rawFolderPath) || !Directory.Exists(rawFolderPath))
        {
            AppendLog("Invalid or empty folder path provided.");
            return;
        }

        // Add the folder itself to the ModFiles collection
        ModFiles.Add(new ModFileItem
        {
            FileName = rawFolderPath, // Set the full folder path
            IsSelected = true // Automatically mark this item as selected
        });

        AppendLog($"Added project folder to mod list: {rawFolderPath}");
        await Task.CompletedTask; // No additional processing needed
    }

    private string PromptUserForFolderSelection()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select a Project Folder"
        };

        var result = dialog.ShowDialog();

        // Return the selected path if the user confirmed their selection, otherwise return null
        return result == true ? dialog.FolderName : null;
    }


    private async Task FixWrongTextureFormatAsync(IEnumerable<string> pyFiles, string wadFolder)
    {
        AppendLog("Starting texture format validation and correction...");

        // Gather all existing textures
        var existingFiles = Directory.GetFiles(wadFolder, "*.*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".dds", StringComparison.OrdinalIgnoreCase) ||
                           file.EndsWith(".tex", StringComparison.OrdinalIgnoreCase))
            .AsParallel() // Use parallel for file enumeration
            .Select(file => NormalizePath(file))
            .ToHashSet();

        // Pattern to detect texture paths in `.py` files (with quotes)
        var assetRegex = new Regex("\"(?<path>ASSETS/.+?\\.(dds|tex))\"", RegexOptions.IgnoreCase);

        // Run file processing in parallel
        var tasks = pyFiles.Select(async pyFile =>
        {
            try
            {
                var content = await File.ReadAllTextAsync(pyFile);
                var matches = assetRegex.Matches(content); // Match all texture paths
                var updated = false;

                foreach (Match match in matches)
                {
                    var matchValue = match.Value; // The exact matched string, including quotes
                    var relativePath = NormalizePath(match.Groups["path"].Value); // Path without quotes
                    var fullTexPath = NormalizePath(Path.Combine(wadFolder, relativePath));
                    var hashedTexPath = NormalizePath(GetHashedFilePath(wadFolder, relativePath));

                    // Case 1: If .tex doesn't exist, but .dds does
                    if (relativePath.EndsWith(".tex", StringComparison.OrdinalIgnoreCase) &&
                        !existingFiles.Contains(fullTexPath) && !existingFiles.Contains(hashedTexPath))
                    {
                        var ddsPath = relativePath.Replace(".tex", ".dds"); // Replace extension
                        var fullDdsPath = NormalizePath(Path.Combine(wadFolder, ddsPath));
                        var hashedDdsPath = NormalizePath(GetHashedFilePath(wadFolder, ddsPath));

                        if (existingFiles.Contains(fullDdsPath) || existingFiles.Contains(hashedDdsPath))
                        {
                            AppendLog($"Fixing format: Updating {relativePath} -> {ddsPath}");
                            content = content.Replace(matchValue,
                                matchValue.Replace(".tex", ".dds")); // Update file content
                            updated = true;
                        }
                    }
                    // Case 2: If .dds doesn't exist, but .tex does
                    else if (relativePath.EndsWith(".dds", StringComparison.OrdinalIgnoreCase) &&
                             !existingFiles.Contains(fullTexPath) && !existingFiles.Contains(hashedTexPath))
                    {
                        var texPath = relativePath.Replace(".dds", ".tex"); // Replace extension
                        var fullTexPathCandidate = NormalizePath(Path.Combine(wadFolder, texPath));
                        var hashedTexPathCandidate = NormalizePath(GetHashedFilePath(wadFolder, texPath));

                        if (existingFiles.Contains(fullTexPathCandidate) ||
                            existingFiles.Contains(hashedTexPathCandidate))
                        {
                            AppendLog($"Fixing format: Updating {relativePath} -> {texPath}");
                            content = content.Replace(matchValue,
                                matchValue.Replace(".dds", ".tex")); // Update file content
                            updated = true;
                        }
                    }
                }

                if (updated) await File.WriteAllTextAsync(pyFile, content); // Save updated file
            }
            catch (Exception ex)
            {
                AppendLog($"Error processing file {pyFile}: {ex.Message}");
            }
        });

        await Task.WhenAll(tasks); // Wait for all tasks to complete

        AppendLog("Texture format validation and correction completed.");
    }

    private async Task DownloadWithRetryAsync(string fileUrl, string destinationPath, int retryCount = 3)
    {
        var attempt = 0;
        while (attempt < retryCount)
        {
            attempt++;
            try
            {
                // Perform HTTP GET request
                var response = await HttpClient.GetAsync(fileUrl);
                response.EnsureSuccessStatusCode(); // Throw if status code is not 2xx

                await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
                await response.Content.CopyToAsync(fileStream);

                AppendLog($"Downloaded file successfully: {destinationPath}");
                return; // Exit on successful download
            }
            catch (Exception ex)
            {
                if (attempt >= retryCount)
                {
                    AppendLog($"Failed to download file {fileUrl} after {retryCount} attempts: {ex.Message}");
                    throw; // Rethrow after exhausting retries
                }

                AppendLog($"Retry {attempt}/{retryCount} for file {fileUrl}: {ex.Message}");
                await Task.Delay(1000 * attempt); // Exponential backoff
            }
        }
    }

    private async Task CopyFileAsync(string sourcePath, string destinationPath)
    {
        try
        {
            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                       FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write,
                       FileShare.None, 4096, FileOptions.Asynchronous))
            {
                await sourceStream.CopyToAsync(destinationStream); // Important: await here
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions appropriately (e.g., log, rethrow)
            AppendLog($"Error in CopyFileAsync: {ex.Message}");
            // Consider throwing the exception or handling it differently if needed
            // throw; // Re-throw if you want the exception to propagate
        }
    }

    private async Task DownloadMissingTexturesAsync(string selectedPatchNumber, IEnumerable<string> pyFiles,
        string wadFolder)
    {
        try
        {
            // Fallback patch numbers to try
            var fallbackPatches = new List<string> { "15.1", "15.2", "15.3", "15.4", "15.5", "15.6", "15.7" };

            AppendLog($"Checking for missing files in patch: {selectedPatchNumber}");

            // Step 1: Gather existing files in the wadFolder
            var existingFiles = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase); // Thread-safe
            foreach (var file in Directory.GetFiles(wadFolder, "*.*", SearchOption.AllDirectories)
                         .Where(file =>
                             file.EndsWith(".dds", StringComparison.OrdinalIgnoreCase) ||
                             file.EndsWith(".tex", StringComparison.OrdinalIgnoreCase) ||
                             file.EndsWith(".sco", StringComparison.OrdinalIgnoreCase) ||
                             file.EndsWith(".scb", StringComparison.OrdinalIgnoreCase)))
                existingFiles[NormalizePath(file)] = true;

            // Step 2: Parse .py files for asset paths and separate them into missing and existing
            var assetRegex = new Regex(@"assets/.*\.(dds|tex|sco|scb)", RegexOptions.IgnoreCase);
            var allAssets = new ConcurrentBag<string>(); // Thread-safe
            var missingFiles = new ConcurrentBag<string>(); // Thread-safe

            await Parallel.ForEachAsync(pyFiles,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, async (pyFile, token) =>
                {
                    try
                    {
                        // Read file contents
                        using var streamReader = new StreamReader(pyFile);
                        var fileContent = await streamReader.ReadToEndAsync();

                        // Match asset paths
                        var matches = assetRegex.Matches(fileContent);
                        foreach (Match match in matches)
                        {
                            var relativePath = NormalizePath(match.Value.ToLower());
                            allAssets.Add(relativePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"Error processing .py file: {pyFile}. Exception: {ex.Message}");
                    }
                });

            // Separate missing files from existing
            foreach (var assetPath in allAssets.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var fullPath = NormalizePath(Path.Combine(wadFolder, assetPath));
                if (!existingFiles.ContainsKey(fullPath)) missingFiles.Add(assetPath);
            }

            if (!missingFiles.Any())
            {
                AppendLog("All assets are already present. No further processing required.");
                return; // Early exit if no missing files to process
            }

            // Step 3: Copy missing files from pre-downloaded Map11 files
            if (UseDownloadedMap11Files)
            {
                if (selectedPatchNumber == "latest") selectedPatchNumber = "15.6";

                if (selectedPatchNumber == "pbe") selectedPatchNumber = "15.7";

                var patchVersionsToCheck = new[] { selectedPatchNumber, "15.5", "15.4", "15.3", "15.2" };

                var copiedFilesCount = 0;
                var logQueue = new ConcurrentQueue<string>();

                await Parallel.ForEachAsync(missingFiles,
                    new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 },
                    async (missingFile, token) =>
                    {
                        try
                        {
                            var destinationPath = Path.Combine(wadFolder, "Map11", missingFile);
                            if (existingFiles.ContainsKey(destinationPath)) return; // Skip if the file already exists

                            foreach (var patchVersion in patchVersionsToCheck)
                            {
                                var map11Folder = Path.Combine("Downloads", "LeagueFiles", patchVersion, "DATA",
                                    "FINAL", "Maps", "Shipping", "Map11.wad");
                                var sourceFilePath = Path.Combine(map11Folder, missingFile);

                                if (File.Exists(sourceFilePath))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? string.Empty);
                                    await CopyFileWithLoggingAsync(
                                        sourceFilePath,
                                        destinationPath,
                                        $"Copied: {missingFile} from Map11 patch {patchVersion}",
                                        logQueue
                                    );

                                    Interlocked.Increment(ref copiedFilesCount);
                                    existingFiles[destinationPath] = true; // Mark as copied
                                    break; // Stop further searches for this file
                                }
                            }

                            if (!File.Exists(destinationPath))
                                logQueue.Enqueue($"Not Found in any patch: {missingFile}");
                        }
                        catch (IOException ioEx)
                        {
                            AppendLog($"Error in CopyFileAsync: {ioEx.Message}");
                        }
                        catch (Exception ex)
                        {
                            logQueue.Enqueue($"Error copying {missingFile}: {ex.Message}");
                        }
                    });
            }

            // Step 4: Download missing files from communitydragon
            if (!UseDownloadedMap11Files)
            {
                var downloadOptions = new ParallelOptions
                    { MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 4) };
                var fallbackPatchList = new[] { selectedPatchNumber }.Concat(fallbackPatches);

                await Parallel.ForEachAsync(missingFiles, downloadOptions, async (missingFile, _) =>
                {
                    var destinationPath = NormalizePath(Path.Combine(wadFolder, missingFile));
                    if (File.Exists(destinationPath)) return;

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                    try
                    {
                        foreach (var patchVersion in fallbackPatchList)
                        {
                            var url = $"https://raw.communitydragon.org/{patchVersion}/game/{missingFile}"
                                .Replace(".dds", ".png")
                                .Replace(".tex", ".png");

                            var tempPngPath = destinationPath.Replace(".dds", ".png").Replace(".tex", ".png");

                            try
                            {
                                await DownloadWithRetryAsync(url, tempPngPath);
                                if (missingFile.EndsWith(".dds"))
                                {
                                    await ConvertPngToDdsAsync(tempPngPath, destinationPath);
                                }
                                else if (missingFile.EndsWith(".tex"))
                                {
                                    var tempDdsPath = await ConvertPngToDdsAsync(tempPngPath,
                                        destinationPath.Replace(".tex", ".dds"));
                                    await ConvertDdsToTexAsync(tempDdsPath, destinationPath);
                                    File.Delete(tempDdsPath);
                                }

                                File.Delete(tempPngPath);
                                AppendLog($"File downloaded and converted: {missingFile} (patch {patchVersion})");
                                break;
                            }
                            catch
                            {
                                // Continue with the next patch
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"Error downloading {missingFile}: {ex.Message}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            AppendLog($"Error in processing pipeline: {ex.Message}");
        }
    }

    private async Task CopyFileWithLoggingAsync(string source, string dest, string successMessage,
        ConcurrentQueue<string> logQueue)
    {
        await CopyFileAsync(source, dest);
        logQueue.Enqueue(successMessage);
    }

    private string NormalizePath(string path)
    {
        return path.Replace("\\", "/");
    }

    private string GetHashedFilePath(string wadFolder, string relativePath)
    {
        var hash = ComputeHash(relativePath.ToLower());
        var hashedFileName = $"{hash}{Path.GetExtension(relativePath)}";
        return NormalizePath(Path.Combine(wadFolder, hashedFileName));
    }

    private string ComputeHash(string input)
    {
        // Use the Fnv1aHash to compute the hash, then format it as a hex string
        var hashValue = XxHash64Ext.Hash(input);
        return hashValue.ToString("x16");
    }


    private async Task<string> ConvertPngToDdsAsync(string pngPath, string destinationPath)
    {
        using var image = new MagickImage(pngPath);
        image.Format = MagickFormat.Dds;

        // Asynchronously write the image to the destinationPath
        await Task.Run(() => image.Write(destinationPath));

        return destinationPath;
    }

    private async Task ConvertDdsToTexAsync(string ddsPath, string destinationPath)
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OtherTools/tex2dds.exe");
        var arguments = $"\"{ddsPath}\" \"{destinationPath}\"";

        await RunProcessAsync(toolPath, arguments); // Await the asynchronous process
    }

    private async Task ProcessWadFileAsync(string wadFile, string ritobinPath, string hashesDirectory)
    {
        AppendLog($"Processing WAD file: {wadFile}");

        var extractedFolder =
            Path.Combine(Path.GetDirectoryName(wadFile) ?? "", Path.GetFileNameWithoutExtension(wadFile));

        // Step 3: Extract the WAD file and delete it
        if (Directory.Exists(extractedFolder)) Directory.Delete(extractedFolder, true);
        await RunProcessAsync(Path.Combine(hashesDirectory, "wad-extract.exe"), $"\"{wadFile}\" \"{extractedFolder}\"");
        File.Delete(wadFile); // Delete the original WAD file after extraction

        // Step 4: Process .bin files into .py
        var binFiles = Directory.GetFiles(extractedFolder, "*.bin", SearchOption.AllDirectories);
        var pyFiles = new List<string>();
        foreach (var binFile in binFiles)
        {
            AppendLog($"Converting .bin file to Python: {binFile}");
            await RunProcessAsync(ritobinPath, $"\"{binFile}\"");
            var pyFile = Path.ChangeExtension(binFile, ".py");
            if (File.Exists(pyFile)) pyFiles.Add(pyFile);
        }

        // Step 5: Apply modes and other functions, then process .py files

        // First, apply the renaming based on selected more, if any
        foreach (var pyFile in pyFiles)
            if (SelectedMode != "System.Windows.Controls.ComboBoxItem: None" || SelectedMode != "None")
            {
                AppendLog($"Applying renaming replacements to: {pyFile}");
                await ModifyFileWithReplacementsAsync(pyFile, SelectedMode);
            }
            else
            {
                AppendLog($"Skipping renaming replacements for file: {pyFile}");
            }


        //After renaming, proceed to download missing textures if selected
        if (FixMissingTextures) await DownloadMissingTexturesAsync(SelectedPatchNumber, pyFiles, extractedFolder);

        //After downloading the textures, check if the user wants to fix incorrect format
        if (FixWrongTextureFormat)
        {
            AppendLog("Fixing wrong texture formats...");
            await FixWrongTextureFormatAsync(pyFiles, extractedFolder);
        }


        // Step 6: Convert .py files back into .bin
        foreach (var pyFile in pyFiles)
        {
            AppendLog($"Converting Python file back to .bin: {pyFile}");
            await RunProcessAsync(ritobinPath, $"\"{pyFile}\"");
            File.Delete(pyFile); // Clean up the .py file
        }

        // Step 7: Repack the folder to a new WAD file
        AppendLog($"Repacking folder into WAD file: {wadFile}");
        await RunProcessAsync(Path.Combine(hashesDirectory, "wad-make.exe"), $"\"{extractedFolder}\" \"{wadFile}\"");

        // Step 8: Remove the extracted folder
        if (Directory.Exists(extractedFolder)) Directory.Delete(extractedFolder, true);
        AppendLog($"Finished processing WAD file: {wadFile}");
    }

    private async Task ProcessRawFolderAsync(string rawFolderPath, string ritobinPath)
    {
        AppendLog($"Processing raw folder: {rawFolderPath}");

        // Step 4: Process .bin files into .py
        var binFiles = Directory.GetFiles(rawFolderPath, "*.bin", SearchOption.AllDirectories);
        var pyFiles = new ConcurrentBag<string>(); // Use a thread-safe collection for parallelism
        var maxParallelism = binFiles.Count() / 4; // Define the maximum number of conversions to run at once

        AppendLog($"Converting {binFiles.Length} .bin files to Python...");

        // Use Parallel.ForEachAsync for parallel processing
        await Parallel.ForEachAsync(binFiles, new ParallelOptions { MaxDegreeOfParallelism = maxParallelism },
            async (binFile, _) =>
            {
                try
                {
                    AppendLog($"Converting .bin file to Python: {binFile}");

                    // Run the conversion process asynchronously
                    await RunProcessAsync(ritobinPath, $"\"{binFile}\"");

                    var pyFile = Path.ChangeExtension(binFile, ".py");
                    if (File.Exists(pyFile))
                        // Add to thread-safe collection
                        pyFiles.Add(pyFile);
                    else
                        AppendLog($"Conversion failed: {binFile}. .py file not found.");
                }
                catch (Exception ex)
                {
                    AppendLog($"Error converting file {binFile}: {ex.Message}");
                }
            });

        // Step 5: Apply modes and other functions, then process .py files
        foreach (var pyFile in pyFiles)
        {
            if (SelectedMode != "None" || SelectedMode != "System.Windows.Controls.ComboBoxItem: None")
                await ModifyFileWithReplacementsAsync(pyFile, SelectedMode);

            // Handle missing textures and wrong formats
            if (FixMissingTextures) await DownloadMissingTexturesAsync(SelectedPatchNumber, pyFiles, rawFolderPath);

            if (FixWrongTextureFormat)
            {
                AppendLog("Fixing wrong texture formats...");
                await FixWrongTextureFormatAsync(pyFiles, rawFolderPath);
            }
        }

        // Step 6: Clean up .py files
        foreach (var pyFile in pyFiles)
        {
            AppendLog($"Cleaning up file: {pyFile}");
            File.Delete(pyFile); // Delete the .py files after processing
        }

        AppendLog($"Finished processing raw folder: {rawFolderPath}");
    }

    public async Task RunFixerAsync()
    {
        // Ensure there are selected mods or folders to process
        if (!ModFiles.Any(f => f.IsSelected))
        {
            AppendLog("No mods selected for processing.");
            return;
        }

        // Define base directories for tool dependencies
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var toolDirectory = Path.Combine(baseDirectory, "OtherTools");
        var hashesDirectory = Path.Combine(toolDirectory, "hashes");

        var ritobinPath = Path.Combine(toolDirectory, "ritobin_cli.exe");

        // Validate tools
        if (!File.Exists(ritobinPath))
        {
            AppendLog("Required tools not found in the 'OtherTools' folder. Ensure ritobin_cli.exe is present.");
            return;
        }

        AppendLog("All required tools found. Starting Patch Fixer process...");

        // Collect selected files/folders to process
        var selectedItems = ModFiles.Where(f => f.IsSelected).Select(f => f.FileName).ToList();
        AppendLog($"Running Patch Fixer in {SelectedMode} mode on {selectedItems.Count} items...");

        foreach (var item in selectedItems)
        {
            AppendLog($"Processing item: {item}");

            if (Directory.Exists(item))
            {
                // Check if item is a raw folder or contains wad.client files
                var wadFiles = Directory.GetFiles(item, "*.wad.client", SearchOption.AllDirectories);

                if (wadFiles.Any())
                    // Process WAD files
                    foreach (var wadFile in wadFiles)
                        await ProcessWadFileAsync(wadFile, ritobinPath, hashesDirectory);
                else
                    // Process raw folder
                    await ProcessRawFolderAsync(item, ritobinPath);
            }
            else
            {
                AppendLog($"Invalid directory: {item}. Skipping...");
            }
        }

        AppendLog("Patch Fixer process completed.");
    }


    private async Task ModifyFileWithReplacementsAsync(string filePath, string mode)
    {
        try
        {
            // Read the file content asynchronously
            var originalContent = await File.ReadAllTextAsync(filePath);
            var fixedmode = mode.Replace("System.Windows.Controls.ComboBoxItem: ", "");
            var content = originalContent; // Assign content variable here.


            // Apply replacements based on the mode
            if (fixedmode == "25.S1.3")
            {
                var textureNameToPathRegex = new Regex(@"textureName: string =");
                content = textureNameToPathRegex.Replace(content, "texturePath: string =");
                // Write the modified content back to the file asynchronously ONLY if it has changed.
                if (content != originalContent) await File.WriteAllTextAsync(filePath, content);
            }
            else if (fixedmode == "25.S1.4")
            {
                var samplerNameToTextureNameRegex = new Regex(@"samplerName: string =");
                content = samplerNameToTextureNameRegex.Replace(content, "textureName: string =");
                // Write the modified content back to the file asynchronously ONLY if it has changed.
                if (content != originalContent) await File.WriteAllTextAsync(filePath, content);
            }
        }
        catch (Exception ex)
        {
            AppendLog($"Error modifying file {filePath}: {ex.Message}");
        }
    }


    private async Task RunProcessAsync(string filePath, string arguments)
    {
        try
        {
            await Task.Run(() =>
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = filePath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0)
                    AppendLog($"Process failed: {filePath} with arguments: {arguments}");
            });
        }
        catch (Exception ex)
        {
            AppendLog($"Error running process {filePath}: {ex.Message}");
        }
    }

    private async Task EnsureHashFiles(string hashesDirectory)
    {
        string[] requiredHashes =
        {
            "hashes.game.txt",
            "hashes.lcu.txt",
            "hashes.binfields.txt",
            "hashes.bintypes.txt",
            "hashes.binhashes.txt",
            "hashes.binentries.txt"
        };

        var missingHashes = requiredHashes
            .Where(hash => !File.Exists(Path.Combine(hashesDirectory, hash)))
            .ToList();

        if (missingHashes.Count > 0)
        {
            AppendLog("Missing hash files. Downloading...");
            using var httpClient = new HttpClient();

            foreach (var hash in missingHashes)
            {
                var url = $"https://raw.communitydragon.org/data/hashes/lol/{hash}";
                var destPath = Path.Combine(hashesDirectory, hash);
                try
                {
                    var data = await httpClient.GetByteArrayAsync(url);
                    Directory.CreateDirectory(hashesDirectory);
                    await File.WriteAllBytesAsync(destPath, data);
                    AppendLog($"Downloaded: {hash}");
                }
                catch (Exception ex)
                {
                    AppendLog($"Failed to download {hash}: {ex.Message}");
                }
            }
        }
        else
        {
            AppendLog("All required hash files are present.");
        }
    }

    private string? GetCslolManagerPath()
    {
        return Process.GetProcessesByName("cslol-manager").FirstOrDefault()?.MainModule?.FileName;
    }

    private void AppendLog(string message)
    {
        Application.Current.Dispatcher.Invoke(() => { LogText += $"{message}\n"; });
    }
}

public class ModFileItem
{
    public string FileName { get; set; } // Holds the full file or folder path
    public bool IsSelected { get; set; } // Indicates whether the item is selected

    // Derived property to calculate the display-friendly name
    public string ModName => Path.GetFileName(FileName);
}