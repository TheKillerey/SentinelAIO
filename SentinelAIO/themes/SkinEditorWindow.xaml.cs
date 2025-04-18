using System.IO;
using System.Windows;
using System.Windows.Controls;
using SentinelAIO.Helpers;
using SentinelAIO.Library;

namespace SentinelAIO.Themes;

public partial class SkinEditorWindow : Window
{
    private readonly string _projectPath; // Store the passed path
    private readonly MinionSkinHandler _skinHandler;
    private string _copySourceFile; //Source 1 to Copy from
    private string _copyTargetFile; //Source 2 to Copy from

    private bool _isProcessing;
    private string _pasteSourceFile; //Source 3 to Paste to from Source 1
    private string _pasteTargetFile; // Source 4 to Paste to from Source 2
    private string _selectedChampion;

    // Constructor to initialize with path
    public SkinEditorWindow(string path)
    {
        InitializeComponent();
        _projectPath = path;
        IsProcessing = true; // Disable the grid initially
        _skinHandler = new MinionSkinHandler(path);
        LoadChampions();
    }

    private bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            _isProcessing = value;
            Dispatcher.Invoke(() => ProcessingGrid.IsEnabled = !value); // Enable/disable the grid
        }
    }

    /// <summary>
    ///     Loads the list of champions and populates the ChampionsList ListBox.
    /// </summary>
    private async void LoadChampions()
    {
        try
        {
            // Disable UI during processing
            IsProcessing = true;

            // Get all subdirectories under the project path
            var potentialMapPaths = Directory.GetDirectories(_projectPath, "Map*", SearchOption.TopDirectoryOnly);

            // Find all valid "skins" directories
            var skinDirectories = potentialMapPaths
                .SelectMany(mapPath =>
                    Directory.GetDirectories(Path.Combine(mapPath, "DATA", "Characters"), "*",
                        SearchOption.AllDirectories))
                .Where(championPath =>
                    Directory.Exists(Path.Combine(championPath, "skins")))
                .Select(championPath => Path.Combine(championPath, "skins"))
                .ToList();

            // Collect all .bin files in the "skins" directories
            var binFiles = skinDirectories
                .SelectMany(skinDir => Directory.GetFiles(skinDir, "skin*.bin"))
                .ToList();

            // Dynamically calculate a safe parallelism level (based on system cores)
            var maxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount * 2);
            maxDegreeOfParallelism = Math.Min(maxDegreeOfParallelism, 100); // Cap at 100 for stability

            // Process valid .bin files in parallel with controlled parallelism
            await Parallel.ForEachAsync(binFiles,
                new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, async (binFile, _) =>
                {
                    // Convert .bin to .py only if not already converted
                    if (!File.Exists(binFile.Replace(".bin", ".py")))
                        await SkinFileProcessor.ConvertBinToPyWithoutReadAsync(binFile);
                });

            // Fetch the list of champions dynamically and update the UI
            var champions = await _skinHandler.LoadChampionsAsync();

            ChampionsList.ItemsSource = champions;

            if (!champions.Any())
                MessageBox.Show("No champions were found in the game files.", "Information", MessageBoxButton.OK,
                    MessageBoxImage.Information);
        }
        catch (DirectoryNotFoundException ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while loading champions: {ex.Message}", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    ///     Handles champion selection and populates the skin-related ComboBoxes with skin files.
    /// </summary>
    private async void ChampionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedChampion = (string)((ListBox)sender).SelectedItem;

        if (!string.IsNullOrEmpty(_selectedChampion))
            try
            {
                // Resolve the directory path for the selected champion's skins
                string skinsDirectory = null;
                var potentialMapPaths = Directory.GetDirectories(_projectPath, "Map*", SearchOption.TopDirectoryOnly);

                foreach (var mapPath in potentialMapPaths)
                {
                    var potentialSkinsPath = Path.Combine(mapPath, "DATA", "Characters", _selectedChampion, "Skins");
                    if (Directory.Exists(potentialSkinsPath))
                    {
                        skinsDirectory = potentialSkinsPath;
                        break;
                    }
                }

                if (skinsDirectory == null)
                {
                    MessageBox.Show($"Skins directory not found for champion: {_selectedChampion}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Get a list of all .bin files in the skins directory
                var binFiles = Directory.GetFiles(skinsDirectory, "*.bin");
                var pyFileNames = new List<string>();

                foreach (var binFile in binFiles)
                {
                    // Convert to .py if not already converted
                    var pyFilePath = Path.ChangeExtension(binFile, ".py");
                    if (!File.Exists(pyFilePath)) await SkinFileProcessor.ConvertBinToPyWithoutReadAsync(binFile);

                    // Add the .py file's name (without extension) to the list (e.g., 'skin01')
                    pyFileNames.Add(Path.GetFileNameWithoutExtension(pyFilePath));
                }

                if (pyFileNames.Count == 0)
                {
                    MessageBox.Show($"No valid skin files found for champion: {_selectedChampion}", "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Populate skin-related ComboBoxes with the filenames (e.g., 'skin01', 'skin02')
                ComboBoxCopySource.ItemsSource = pyFileNames;
                ComboBoxCopyTarget.ItemsSource = pyFileNames;
                ComboBoxPasteSource.ItemsSource = pyFileNames;
                ComboBoxPasteTarget.ItemsSource = pyFileNames;
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("Champion skins directory not found!", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading skins: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
    }

    /// <summary>
    ///     Detect selection changes in Copy Source ComboBox.
    /// </summary>
    private void ComboBoxCopySource_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _copySourceFile = (string)((ComboBox)sender).SelectedItem;
    }

    /// <summary>
    ///     Detect selection changes in Copy Target ComboBox.
    /// </summary>
    private void ComboBoxCopyTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _copyTargetFile = (string)((ComboBox)sender).SelectedItem;
    }

    /// <summary>
    ///     Detect selection changes in Paste Source ComboBox.
    /// </summary>
    private void ComboBoxPasteSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _pasteSourceFile = (string)((ComboBox)sender).SelectedItem;
    }

    /// <summary>
    ///     Detect selection changes in Paste Target ComboBox.
    /// </summary>
    private void ComboBoxPasteTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _pasteTargetFile = (string)((ComboBox)sender).SelectedItem;
    }

    /// <summary>
    ///     Handles the Copy → Paste button click event to copy skinMeshProperties between files.
    /// </summary>
    private async void ButtonCopyPaste_Click(object sender, RoutedEventArgs e)
    {
        // Ensure valid selections for copying and pasting
        if (string.IsNullOrEmpty(_copySourceFile) || string.IsNullOrEmpty(_copyTargetFile) ||
            string.IsNullOrEmpty(_pasteSourceFile) || string.IsNullOrEmpty(_pasteTargetFile))
        {
            MessageBox.Show("Please select valid source and target files for all operations.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            // Append .py extension to selected files
            var copySourceFileWithExtension = _copySourceFile + ".py";
            var copyTargetFileWithExtension = _copyTargetFile + ".py";
            var pasteSourceFileWithExtension = _pasteSourceFile + ".py";
            var pasteTargetFileWithExtension = _pasteTargetFile + ".py";

            // Locate potential map folders
            var potentialMapPaths = Directory.GetDirectories(_projectPath, "Map*", SearchOption.TopDirectoryOnly);

            string copySourcePath = null;
            string copyTargetPath = null;
            string pasteSourcePath = null;
            string pasteTargetPath = null;

            // Resolve and validate paths for source and target files
            foreach (var mapPath in potentialMapPaths)
            {
                var charactersPath = Path.Combine(mapPath, "DATA", "Characters");

                if (Directory.Exists(charactersPath))
                {
                    // Resolve paths for each file
                    var sourceChampionPath = Path.Combine(charactersPath, _selectedChampion, "skins");
                    if (Directory.Exists(sourceChampionPath))
                    {
                        // 1. Copy Source File
                        if (copySourcePath == null &&
                            File.Exists(Path.Combine(sourceChampionPath, copySourceFileWithExtension)))
                            copySourcePath = Path.Combine(sourceChampionPath, copySourceFileWithExtension);

                        // 2. Copy Target File
                        if (copyTargetPath == null &&
                            File.Exists(Path.Combine(sourceChampionPath, copyTargetFileWithExtension)))
                            copyTargetPath = Path.Combine(sourceChampionPath, copyTargetFileWithExtension);

                        // 3. Paste Source File
                        if (pasteSourcePath == null &&
                            File.Exists(Path.Combine(sourceChampionPath, pasteSourceFileWithExtension)))
                            pasteSourcePath = Path.Combine(sourceChampionPath, pasteSourceFileWithExtension);

                        // 4. Paste Target File
                        if (pasteTargetPath == null &&
                            File.Exists(Path.Combine(sourceChampionPath, pasteTargetFileWithExtension)))
                            pasteTargetPath = Path.Combine(sourceChampionPath, pasteTargetFileWithExtension);
                    }
                }

                // Stop searching once all paths are resolved
                if (copySourcePath != null && copyTargetPath != null && pasteSourcePath != null &&
                    pasteTargetPath != null)
                    break;
            }

            // Validate all paths
            if (copySourcePath == null)
            {
                MessageBox.Show($"Copy Source file not found: {_copySourceFile}.py", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (copyTargetPath == null)
            {
                MessageBox.Show($"Copy Target file not found: {_copyTargetFile}.py", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (pasteSourcePath == null)
            {
                MessageBox.Show($"Paste Source file not found: {_pasteSourceFile}.py", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (pasteTargetPath == null)
            {
                MessageBox.Show($"Paste Target file not found: {_pasteTargetFile}.py", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Perform the copy-paste operations
            // 1. Copy skin properties from Copy Source to Paste Source
            await _skinHandler.CopyAndPasteSkinMeshPropertiesAsync(copySourcePath, pasteSourcePath);

            // 2. Copy skin properties from Copy Target to Paste Target
            await _skinHandler.CopyAndPasteSkinMeshPropertiesAsync(copyTargetPath, pasteTargetPath);

            // Notify the user of success
            MessageBox.Show("SkinMeshProperties copied successfully between all sources and targets!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred during the copy-paste operation: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    //protected override async void OnClosed(EventArgs e)
    //{
    //    base.OnClosed(e);
//
    //    try
    //    {
    //        // Locate all potential map folders
    //        var potentialMapPaths = Directory.GetDirectories(_projectPath, "Map*", SearchOption.TopDirectoryOnly);
//
    //        foreach (var mapPath in potentialMapPaths)
    //        {
    //            var charactersPath = Path.Combine(mapPath, "DATA", "Characters");
//
    //            if (Directory.Exists(charactersPath))
    //                foreach (var champPath in Directory.GetDirectories(charactersPath))
    //                {
    //                    var skinsPath = Path.Combine(champPath, "Skins");
//
    //                    if (Directory.Exists(skinsPath))
    //                    {
    //                        var pyFiles = Directory.GetFiles(skinsPath, "*.py");
    //                        foreach (var pyFile in pyFiles)
    //                        {
    //                            var binPath = Path.ChangeExtension(pyFile, ".bin");
//
    //                            if (File.Exists(binPath))
    //                                // Save changes back into .bin format
    //                                await SkinFileProcessor.SavePyToBinAsync(pyFile, binPath);
    //                        }
    //                    }
    //                }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"An error occurred while cleaning up temporary files: {ex.Message}", "Error",
    //            MessageBoxButton.OK, MessageBoxImage.Error);
    //    }
    //}
}