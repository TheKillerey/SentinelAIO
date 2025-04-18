using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ImageMagick;
using LeagueToolkit.Core.Environment;
using LeagueToolkit.Core.Wad;
using LeagueToolkit.Hashing;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using SentinelAIO.Commands;
using SentinelAIO.Library;
using SentinelAIO.themes;
using SentinelAIO.Themes;

namespace SentinelAIO.ViewModel;

public sealed class ModToolsViewModel : INotifyPropertyChanged
{
    private const string SettingsFileName = "Settings.json";
    private static ModToolsViewModel _instance;

    private readonly MatrixHelperConverter helper = new();
    private readonly List<string> originalTexFiles = new();
    private string _currentFileInfo;
    private int _currentProgress;

    private bool _isIndeterminate;

    private bool _isLoading;

    private string _leagueOfLegendsFolderPath = string.Empty;

    private float _m11Input = 1;
    private float _m12Input;
    private float _m13Input;
    private float _m14Input;

    private float _m21Input;
    private float _m22Input = 1;
    private float _m23Input;
    private float _m24Input;

    private float _m31Input;
    private float _m32Input;
    private float _m33Input = 1;
    private float _m34Input;

    private float _m41Input;
    private float _m42Input;
    private float _m43Input;
    private float _m44Input = 1;

    private string _matrixData;


    private ObservableCollection<ModInfo> _modInfos;
    private double _progressBarPercentage;

    private Visibility _progressBarVisibility = Visibility.Collapsed;

    private string _projectFolderPath;

    private float _rotXInput;
    private float _rotYInput;
    private float _rotZInput;

    private float _scaleXInput = -1;
    private float _scaleYInput = 1;
    private float _scaleZInput = -1;

    private string _selectedMeshName;
    private int _totalFiles;

    private float _transXInput;
    private float _transYInput;
    private float _transZInput;

    private Visibility _visibilityProgressbar = Visibility.Hidden;
    private bool isUpdatingEuler;


    private bool isUpdatingMatrix;
    private Matrix4x4 matrixEntry;
    private Vector3 translateEntry, eulerEntry, scaleEntry;

    public ModToolsViewModel()
    {
        ModInfos = new ObservableCollection<ModInfo>();
        MeshNames = new ObservableCollection<string>();
        OpenModFileCommand = new RelayCommand(OpenModFile);
        OpenLUTFileCommand = new RelayCommand(OpenLUTFile);
        GetMapFilesCommand = new RelayCommand(GetMapFiles);
        RitoPatchFixerCommand = new RelayCommand(RitoPatchFixer);
        ManifestDownloaderCommand = new RelayCommand(ManifestDownloader);
        SetDefaultMapCommand = new RelayCommand(SetDefaultMap);
        FixImageFilesCommand = new RelayCommand(FixImageFilesAsync);
        RemoveNotNeededTexturesCommand = new RelayCommand(RemoveNotNeededTextures);
        SetDefaultMinionsCommand = new RelayCommand(SetDefaultMinions);
        AddMapCommand = new RelayCommand(AddMap);
        matrixEntry = Matrix4x4.Identity;
        translateEntry = Vector3.Zero;
        eulerEntry = Vector3.Zero;
        scaleEntry = Vector3.One;
        UpdateMatrixCommand = new RelayCommand(UpdateMatrixTextBox);
        OpenMapGeoCommand = new RelayCommand(OpenMapGeo);
        MapMaterialEditorCommand = new RelayCommand(MapMaterialEditor);
        LoadSettings();
    }


    public ICommand OpenModFileCommand { get; }
    public ICommand OpenRecolorCommand { get; }
    public ICommand OpenLUTFileCommand { get; }
    public ICommand GetMapFilesCommand { get; }
    public ICommand RitoPatchFixerCommand { get; }
    public ICommand ManifestDownloaderCommand { get; }
    public ICommand SetDefaultMapCommand { get; }
    public ICommand FixImageFilesCommand { get; }
    public ICommand UpdateMatrixCommand { get; }
    public ICommand SetDefaultMinionsCommand { get; }
    public ICommand RemoveNotNeededTexturesCommand { get; }
    public ICommand OpenMapGeoCommand { get; }
    public ICommand MapMaterialEditorCommand { get; }

    public ICommand AddMapCommand { get; }

    public ObservableCollection<string> MeshNames { get; set; }

    public Visibility ProgressBarVisibility
    {
        get => _progressBarVisibility;
        set
        {
            _progressBarVisibility = value;
            OnPropertyChanged();
        }
    }

    public double ProgressBarPercentage
    {
        get => _progressBarPercentage;
        set
        {
            _progressBarPercentage = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        set
        {
            _isIndeterminate = value;
            OnPropertyChanged();
        }
    }

    public int TotalFiles
    {
        get => _totalFiles;
        set
        {
            _totalFiles = value;
            OnPropertyChanged();
        }
    }

    public int CurrentProgress
    {
        get => _currentProgress;
        set
        {
            _currentProgress = value;
            OnPropertyChanged();
        }
    }

    public string CurrentFileInfo
    {
        get => _currentFileInfo;
        set
        {
            _currentFileInfo = value;
            OnPropertyChanged();
        }
    }

    public string ProjectFolderPath
    {
        get => _projectFolderPath;
        set
        {
            _projectFolderPath = value;
            OnPropertyChanged();
        }
    }

    public string LeagueOfLegendsFolderPath
    {
        get => _leagueOfLegendsFolderPath;
        private set
        {
            if (_leagueOfLegendsFolderPath != value)
            {
                _leagueOfLegendsFolderPath = value;
                OnPropertyChanged();
            }
        }
    }

    public string SelectedMeshName
    {
        get => _selectedMeshName;
        set
        {
            if (_selectedMeshName != value)
            {
                _selectedMeshName = value;
                OnPropertyChanged(); // Notify the UI of property changes
            }
        }
    }

    public string MatrixData
    {
        get => _matrixData;
        set
        {
            _matrixData = value;
            OnPropertyChanged();
        }
    }

    public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

    public float TransXInput
    {
        get => _transXInput;
        set
        {
            _transXInput = value;
            OnPropertyChanged();
            if (!isUpdatingEuler)
            {
                isUpdatingMatrix = true;
                UpdateMatrix();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float TransYInput
    {
        get => _transYInput;
        set
        {
            _transYInput = value;
            OnPropertyChanged();
            if (!isUpdatingEuler)
            {
                isUpdatingMatrix = true;
                UpdateMatrix();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float TransZInput
    {
        get => _transZInput;
        set
        {
            _transZInput = value;
            OnPropertyChanged();
            if (!isUpdatingEuler)
            {
                isUpdatingMatrix = true;
                UpdateMatrix();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float RotXInput
    {
        get => _rotXInput;
        set
        {
            _rotXInput = value;
            OnPropertyChanged();
            if (!isUpdatingEuler)
            {
                isUpdatingMatrix = true;
                UpdateMatrix();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float RotYInput
    {
        get => _rotYInput;
        set
        {
            _rotYInput = value;
            OnPropertyChanged();
            if (!isUpdatingEuler)
            {
                isUpdatingMatrix = true;
                UpdateMatrix();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float RotZInput
    {
        get => _rotZInput;
        set
        {
            _rotZInput = value;
            OnPropertyChanged();
            if (!isUpdatingEuler)
            {
                isUpdatingMatrix = true;
                UpdateMatrix();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float ScaleXInput
    {
        get => _scaleXInput;
        set
        {
            _scaleXInput = value;
            OnPropertyChanged();
            if (!isUpdatingEuler)
            {
                isUpdatingMatrix = true;
                UpdateMatrix();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float ScaleYInput
    {
        get => _scaleYInput;
        set
        {
            _scaleYInput = value;
            OnPropertyChanged();
            if (!isUpdatingEuler)
            {
                isUpdatingMatrix = true;
                UpdateMatrix();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float ScaleZInput
    {
        get => _scaleZInput;
        set
        {
            _scaleZInput = value;
            OnPropertyChanged();
            if (!isUpdatingEuler)
            {
                isUpdatingMatrix = true;
                UpdateMatrix();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M11Input
    {
        get => _m11Input;
        set
        {
            _m11Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            // We make sure the string version is always up to date
            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M12Input
    {
        get => _m12Input;
        set
        {
            _m12Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M13Input
    {
        get => _m13Input;
        set
        {
            _m13Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M14Input
    {
        get => _m14Input;
        set
        {
            _m14Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M21Input
    {
        get => _m21Input;
        set
        {
            _m21Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M22Input
    {
        get => _m22Input;
        set
        {
            _m22Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M23Input
    {
        get => _m23Input;
        set
        {
            _m23Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M24Input
    {
        get => _m24Input;
        set
        {
            _m24Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M31Input
    {
        get => _m31Input;
        set
        {
            _m31Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M32Input
    {
        get => _m32Input;
        set
        {
            _m32Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M33Input
    {
        get => _m33Input;
        set
        {
            _m33Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M34Input
    {
        get => _m34Input;
        set
        {
            _m34Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M41Input
    {
        get => _m41Input;
        set
        {
            _m41Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M42Input
    {
        get => _m42Input;
        set
        {
            _m42Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M43Input
    {
        get => _m43Input;
        set
        {
            _m43Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public float M44Input
    {
        get => _m44Input;
        set
        {
            _m44Input = value;
            OnPropertyChanged();
            if (!isUpdatingMatrix)
            {
                isUpdatingEuler = true;
                UpdateEuler();
            }

            MatrixData = MatrixToString(matrixEntry);
        }
    }

    public ObservableCollection<ModInfo> ModInfos
    {
        get => _modInfos;
        set
        {
            _modInfos = value;
            OnPropertyChanged();
        }
    }

    public Visibility VisibilityProgressbar
    {
        get => _visibilityProgressbar;
        set
        {
            _visibilityProgressbar = value;
            OnPropertyChanged();
        }
    }

    public static ModToolsViewModel Instance => _instance ??= new ModToolsViewModel();


    public event PropertyChangedEventHandler? PropertyChanged;

    private void RitoPatchFixer(object obj)
    {
        // Create and open the RitoPatchFixer window
        var window = new RitoPatchFixer();

        // Show the window as a modal dialog
        window.ShowDialog();
    }

    private void ManifestDownloader(object obj)
    {
        // Create and open the RitoPatchFixer window
        var window = new ManifestDownloader();

        // Show the window as a modal dialog
        window.ShowDialog();
    }

    private void MapMaterialEditor(object parameter)
    {
        var inputFilePath =
            @"D:\Mods_Github\OldSummonersRiftV2\Old Summoners Rift V2\OldSummonersRiftV2_Tests\Map11\DATA\Maps\mapgeometry\map11\base_srx.materials.bin";
        var outputFilePath =
            @"D:\Mods_Github\OldSummonersRiftV2\Old Summoners Rift V2\OldSummonersRiftV2_Tests\Map11\DATA\Maps\mapgeometry\map11\base_srx.materials.txt";

        LtRitobinReader.RitobinWriter(inputFilePath, outputFilePath);
    }

    private void OpenMapGeo(object parameter)
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Map Geometry File (*.mapgeo)|*.mapgeo",
                Title = "Select MapGeo File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var mapgeoFilePath = openFileDialog.FileName;

                if (!File.Exists(mapgeoFilePath))
                {
                    var errorBox = new CustomMessageBox(
                        "Error",
                        PackIconKind.FileRemove,
                        "The selected file does not exist.",
                        "OK",
                        "",
                        Visibility.Collapsed);

                    errorBox.ShowDialog();
                    return;
                }

                using var mapgeoStream = File.OpenRead(mapgeoFilePath);
                using EnvironmentAsset mapgeoAsset = new(mapgeoStream);

                // Clear current collection
                MeshNames.Clear();

                // Retrieve the mesh names
                var meshes = mapgeoAsset.Meshes; // Assuming mapgeoAsset contains `Meshes`
                foreach (var mesh in meshes)
                {
                    var names = mesh.Name.Split(','); // Split the mesh name if it contains commas
                    foreach (var name in names)
                        // Add each mesh name to the collection
                        MeshNames.Add(name.Trim());
                }

                // Automatically select the first item in the collection (if any exists)
                if (MeshNames.Any())
                    SelectedMeshName =
                        MeshNames.First(); // NOTE: This will update the `SelectedMeshName` property in the ViewModel

                var successBox = new CustomMessageBox(
                    "Success",
                    PackIconKind.CheckCircleOutline,
                    $"Successfully loaded mapgeo file and found {MeshNames.Count} meshes.",
                    "OK",
                    "",
                    Visibility.Collapsed);

                successBox.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            var errorBox = new CustomMessageBox(
                "Error",
                PackIconKind.AlertCircleOutline,
                $"An error occurred while opening the mapgeo file:\n{ex.Message}",
                "OK",
                "",
                Visibility.Collapsed);

            errorBox.ShowDialog();
        }
    }

    private void SetDefaultMinions(object parameter)
    {
        try
        {
            // Ensure the ProjectFolderPath is valid
            if (string.IsNullOrEmpty(ProjectFolderPath) || !Directory.Exists(ProjectFolderPath))
            {
                var errorBox = new CustomMessageBox(
                    "Error",
                    PackIconKind.FolderAlert,
                    "Please ensure the Project Folder Path is specified and valid.",
                    "OK",
                    "",
                    Visibility.Collapsed);

                errorBox.ShowDialog();
                return;
            }

            // Append "Map11" to the ProjectFolderPath
            var map11Path = Path.Combine(ProjectFolderPath, "Map11");

            // Ensure Map11 folder exists
            if (!Directory.Exists(map11Path)) Directory.CreateDirectory(map11Path);

            // Open the SkinEditorWindow
            var skinEditorWindow = new SkinEditorWindow(ProjectFolderPath); // Pass the base ProjectFolderPath
            skinEditorWindow.ShowDialog(); // Open the window as a modal dialog
        }
        catch (Exception ex)
        {
            // Handle errors encountered during processing
            var errorBox = new CustomMessageBox(
                "Error",
                PackIconKind.AlertCircleOutline,
                $"An error occurred: {ex.Message}",
                "OK",
                "",
                Visibility.Collapsed);

            errorBox.ShowDialog();
        }
    }

    private async void FixImageFilesAsync(object parameter)
    {
        var folderDialog = new OpenFolderDialog();
        if (folderDialog.ShowDialog() == true)
        {
            var inputFolder = folderDialog.FolderName;

            var backupFolder = Path.Combine(Path.GetDirectoryName(inputFolder),
                Path.GetFileName(inputFolder) + "_backup");
            if (Directory.Exists(backupFolder))
            {
                //
            }
            else
            {
                DirectoryCopy(inputFolder, backupFolder, true);
            }

            // Clear the originalTexFiles list before processing
            originalTexFiles.Clear();

            // Step 2: Convert all .tex files to .dds
            var texFilePaths = Directory.GetFiles(inputFolder, "*.tex", SearchOption.AllDirectories);
            foreach (var texFilePath in texFilePaths)
                try
                {
                    var ddsFilePath = await ConvertTexToDdsAsync(texFilePath);
                    if (ddsFilePath == null) continue;

                    // Add the converted DDS file path to the list
                    originalTexFiles.Add(ddsFilePath);

                    // Process the .dds file as originating from a .tex file
                    await ProcessDdsFileAsyncNoLut(ddsFilePath, true);
                }
                catch (Exception ex)
                {
                    //
                }

            // Step 3: Process remaining .dds files
            var ddsFilePaths = Directory.GetFiles(inputFolder, "*.dds", SearchOption.AllDirectories);
            foreach (var ddsFilePath in ddsFilePaths)
            {
                // Skip files that were originally .tex (already processed)
                if (originalTexFiles.Contains(ddsFilePath))
                    continue;

                try
                {
                    // Process the .dds file as standalone (not from .tex)
                    await ProcessDdsFileAsyncNoLut(ddsFilePath, false);
                }
                catch (Exception ex)
                {
                    //
                }
            }

            // Step 4: Convert processed `.dds` back to `.tex`
            foreach (var originalTexFile in originalTexFiles)
                try
                {
                    var texFilePath = Path.ChangeExtension(originalTexFile, ".tex");
                    await ConvertDdsToTexAsync(originalTexFile, texFilePath);
                }
                catch (Exception ex)
                {
                    //
                }
        }
    }

    /// <summary>
    ///     Detects whether the alpha channel is completely white.
    /// </summary>
    /// <param name="image">The MagickImage to analyze.</param>
    /// <returns>True if all alpha values are white (255); otherwise, false.</returns>
    private bool IsAlphaChannelWhite(MagickImage image)
    {
        try
        {
            // Extract the alpha channel of the image
            using (var alphaChannel = image.Separate(Channels.Alpha).First())
            {
                // Check if all pixels in the alpha channel are white
                var histogram = alphaChannel.Histogram();

                foreach (var entry in histogram)
                    // A pixel is not white if the value is less than the maximum
                    if (entry.Key.ToByteArray()[0] < 255)
                        return false; // Non-white pixel found

                // All pixels are white
                return true;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private void SetDefaultMap(object parameter)
    {
        // Create a default or existing MapReplacementValues object
        var defaultValues = new MapReplacementValues
        {
            MapContainer = "Maps/MapGeometry/Map11/Base_SRX",
            ObjectsCFG = "ObjectCFG_SRX.cfg",
            ParticlesINI = "Particles_SRX.ini",
            GrassTintTexture = "Grasstint_SRX.dds"
        };

        // Open the MapValuesEditor window
        var editor = new MapValuesEditor(defaultValues);

        if (editor.ShowDialog() == true)
        {
            // After user saves, retrieve the modified values
            var updatedValues = editor.MapValues;

            // Update map using the new values (example usage)
            var projectFolderPath = ProjectFolderPath;
            MapUpdater.UpdateMapFile(projectFolderPath, "map11", updatedValues);
        }
        else
        {
            // User canceled the operation
            Console.WriteLine("Changes were canceled.");
        }
    }

    private void RemoveNotNeededTextures(object parameter)
    {
        try
        {
            // Step 1: Determine the base folder path and append "Map11"
            var baseFolder = ProjectFolderPath; // Assuming ProjectFolderPath is bound to the input from ModTools.xaml
            if (string.IsNullOrEmpty(baseFolder))
            {
                var emptyPathBox = new CustomMessageBox(
                    "Error",
                    PackIconKind.FolderRemove,
                    "Project folder path is empty. Please specify the project folder.",
                    "OK",
                    "",
                    Visibility.Collapsed);

                emptyPathBox.ShowDialog();
                return;
            }

            var mapFolder = Path.Combine(baseFolder, "Map11");
            if (!Directory.Exists(mapFolder))
            {
                var folderNotFoundBox = new CustomMessageBox(
                    "Error",
                    PackIconKind.FolderAlert,
                    $"The folder {mapFolder} does not exist. Please check your input path.",
                    "OK",
                    "",
                    Visibility.Collapsed);

                folderNotFoundBox.ShowDialog();
                return;
            }


            // Step 2: Build the full path to `base_srx.materials.py`
            var materialsFilePath =
                Path.Combine(mapFolder, "data", "maps", "mapgeometry", "map11", "base_srx.materials.py");
            if (!File.Exists(materialsFilePath))
            {
                var errorBox = new CustomMessageBox(
                    "Error",
                    PackIconKind.FileAlert,
                    $"The file `base_srx.materials.py` does not exist at: {materialsFilePath}",
                    "OK",
                    "",
                    Visibility.Collapsed);

                errorBox.ShowDialog();
                return;
            }


            // Step 3: Use regex to extract required textures from `base_srx.materials.py`
            HashSet<string> requiredTextures = new();
            var pattern = @"texturePath: string = \""(ASSETS/Maps/KitPieces/[^\""]+)\"""; // Regex


            var lines = File.ReadLines(materialsFilePath);
            foreach (var line in lines)
            {
                // Match the line with regex
                var match = Regex.Match(line, pattern);
                if (match.Success)
                {
                    var texturePath = match.Groups[1].Value; // Extract the matched texture path

                    // Normalize the path for comparison (lowercase and replace `/` with `\`)
                    var normalizedPath = texturePath.Replace("/", "\\").ToLowerInvariant();
                    requiredTextures.Add(normalizedPath);

                    // Additional Requirement: Add `2x_` and `4x_` versions of `.dds` textures
                    if (normalizedPath.EndsWith(".dds"))
                    {
                        var directory = Path.GetDirectoryName(normalizedPath);
                        var fileName = Path.GetFileName(normalizedPath);
                        if (!string.IsNullOrEmpty(directory) && !string.IsNullOrEmpty(fileName))
                        {
                            // Add `2x_` and `4x_` prefixed versions to the list
                            var texture2x = Path.Combine(directory, "2x_" + fileName).ToLowerInvariant();
                            var texture4x = Path.Combine(directory, "4x_" + fileName).ToLowerInvariant();

                            requiredTextures.Add(texture2x);
                            requiredTextures.Add(texture4x);
                        }
                    }
                }
            }

            // Step 4: Traverse the directory for texture files
            var rootFolder = Path.Combine(mapFolder, "ASSETS", "Maps", "KitPieces");
            if (!Directory.Exists(rootFolder))
            {
                var errorBox = new CustomMessageBox(
                    "Error",
                    PackIconKind.FolderAlert,
                    $"The directory `{rootFolder}` does not exist.",
                    "OK",
                    "",
                    Visibility.Collapsed);

                errorBox.ShowDialog();
                return;
            }

            // Recursively scan all files in the folder
            var allFiles = Directory.GetFiles(rootFolder, "*.*", SearchOption.AllDirectories);

            // Identify files to delete — those not in the `requiredTextures` list
            List<string> filesToDelete = new();
            foreach (var file in allFiles)
            {
                // Convert the file's absolute path to a relative path starting from "ASSETS/Maps/KitPieces"
                var relativePath = file.Replace(mapFolder + "\\", "").Replace("\\", "/").ToLowerInvariant();

                if (!requiredTextures.Contains(relativePath.Replace("/", "\\"))) // Compare normalized paths
                    filesToDelete.Add(file);
            }

            // Step 5: Delete unnecessary files
            foreach (var file in filesToDelete)
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DEBUG] Failed to delete {file}. Error: {ex.Message}");
                }

            // Notify the user upon completion
            var successBox = new CustomMessageBox(
                "Cleanup Complete",
                PackIconKind.CheckCircleOutline,
                $"Removed {filesToDelete.Count} unnecessary texture files.",
                "OK",
                "",
                Visibility.Collapsed);

            successBox.ShowDialog();
        }
        catch (Exception ex)
        {
            var errorBox = new CustomMessageBox(
                "Error",
                PackIconKind.AlertCircleOutline,
                $"An unexpected error occurred: {ex.Message}",
                "OK",
                "",
                Visibility.Collapsed);

            errorBox.ShowDialog();
        }
    }

    private void LoadSettings()
    {
        if (File.Exists(SettingsFileName))
        {
            var json = File.ReadAllText(SettingsFileName);
            var settings = JsonConvert.DeserializeAnonymousType(json,
                new { LeagueOfLegendsFolderPath = string.Empty });

            LeagueOfLegendsFolderPath = settings.LeagueOfLegendsFolderPath;
        }
        else
        {
            // If the file does not exist
            var errorBox = new CustomMessageBox(
                "Error",
                PackIconKind.FileAlert,
                "The file 'Settings.json' was not found. Please configure the settings.",
                "OK",
                "",
                Visibility.Collapsed);

            errorBox.ShowDialog();
        }
    }

    private string MatrixToString(Matrix4x4 matrix)
    {
        var sb = new StringBuilder();

        sb.AppendLine("transform: mtx44 = {");
        sb.AppendLine("                    " + M11Input.ToString(CultureInfo.InvariantCulture) + ", " +
                      M12Input.ToString(CultureInfo.InvariantCulture) + ", " +
                      M13Input.ToString(CultureInfo.InvariantCulture) + ", " +
                      M14Input.ToString(CultureInfo.InvariantCulture));

        sb.AppendLine("                    " + M21Input.ToString(CultureInfo.InvariantCulture) + ", " +
                      M22Input.ToString(CultureInfo.InvariantCulture) + ", " +
                      M23Input.ToString(CultureInfo.InvariantCulture) + ", " +
                      M24Input.ToString(CultureInfo.InvariantCulture));

        sb.AppendLine("                    " + M31Input.ToString(CultureInfo.InvariantCulture) + ", " +
                      M32Input.ToString(CultureInfo.InvariantCulture) + ", " +
                      M33Input.ToString(CultureInfo.InvariantCulture) + ", " +
                      M34Input.ToString(CultureInfo.InvariantCulture));

        sb.AppendLine("                    " + M41Input.ToString(CultureInfo.InvariantCulture) + ", " +
                      M42Input.ToString(CultureInfo.InvariantCulture) + ", " +
                      M43Input.ToString(CultureInfo.InvariantCulture) + ", " +
                      M44Input.ToString(CultureInfo.InvariantCulture));
        sb.AppendLine("                }");

        return sb.ToString();
    }

    private void UpdateMatrixTextBox(object matrixObject)
    {
        if (!(matrixObject is string matrixString))
            // Couldn't convert the method argument to a string.
            // Error handling...
            return;

        var rawValues = matrixString.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // Your filter logic here
        var values = rawValues.Where((val, idx) => idx != 0 && idx != 1 && idx != 2 && idx != 3 && idx != 20)
            .Select(val => val.TrimEnd(',')) // Remove trailing commas
            .ToArray();

        if (values.Length != 16) return;

        // If parsing fails for any value, abort the operation
        if (!float.TryParse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var m11) ||
            !float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var m12) ||
            !float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var m13) ||
            !float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var m14) ||
            !float.TryParse(values[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var m21) ||
            !float.TryParse(values[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var m22) ||
            !float.TryParse(values[6], NumberStyles.Float, CultureInfo.InvariantCulture, out var m23) ||
            !float.TryParse(values[7], NumberStyles.Float, CultureInfo.InvariantCulture, out var m24) ||
            !float.TryParse(values[8], NumberStyles.Float, CultureInfo.InvariantCulture, out var m31) ||
            !float.TryParse(values[9], NumberStyles.Float, CultureInfo.InvariantCulture, out var m32) ||
            !float.TryParse(values[10], NumberStyles.Float, CultureInfo.InvariantCulture, out var m33) ||
            !float.TryParse(values[11], NumberStyles.Float, CultureInfo.InvariantCulture, out var m34) ||
            !float.TryParse(values[12], NumberStyles.Float, CultureInfo.InvariantCulture, out var m41) ||
            !float.TryParse(values[13], NumberStyles.Float, CultureInfo.InvariantCulture, out var m42) ||
            !float.TryParse(values[14], NumberStyles.Float, CultureInfo.InvariantCulture, out var m43) ||
            !float.TryParse(values[15], NumberStyles.Float, CultureInfo.InvariantCulture, out var m44)
           ) return;

        M11Input = m11;
        M12Input = m12;
        M13Input = m13;
        M14Input = m14;
        M21Input = m21;
        M22Input = m22;
        M23Input = m23;
        M24Input = m24;
        M31Input = m31;
        M32Input = m32;
        M33Input = m33;
        M34Input = m34;
        M41Input = m41;
        M42Input = m42;
        M43Input = m43;
        M44Input = m44;
    }

    private void UpdateMatrix()
    {
        // Converting degree input to radians for calculations
        var eulerInput = new Vector3(helper.DegreesToRadians(RotXInput), helper.DegreesToRadians(RotYInput),
            helper.DegreesToRadians(RotZInput));
        var translateInput = new Vector3(-TransXInput, TransYInput, TransZInput);
        var scaleInput = new Vector3(-ScaleXInput, ScaleYInput, -ScaleZInput);

        if (translateInput != translateEntry || eulerInput != eulerEntry || scaleInput != scaleEntry)
        {
            translateEntry = translateInput;
            eulerEntry = eulerInput;
            scaleEntry = scaleInput;
            var (_, matrix, _, _) = helper.EulerToQuaternionAndMatrix(eulerInput, translateInput, scaleInput);
            UpdateMatrixFields(matrix);
        }

        isUpdatingMatrix = false;

        // Update TextBox
        MatrixData = MatrixToString(matrixEntry);
    }

    private void UpdateEuler()
    {
        //Updating only when Matrix4x4 actually changed
        var m11Input = M11Input;
        var m12Input = M12Input;
        var m13Input = M13Input;
        var m14Input = M14Input;
        var m21Input = M21Input;
        var m22Input = M22Input;
        var m23Input = M23Input;
        var m24Input = M24Input;
        var m31Input = M31Input;
        var m32Input = M32Input;
        var m33Input = M33Input;
        var m34Input = M34Input;
        var m41Input = M41Input;
        var m42Input = M42Input;
        var m43Input = M43Input;
        var m44Input = M44Input;
        var matrixInput = new Matrix4x4(
            M11Input, M12Input, M13Input, M14Input,
            M21Input, M22Input, M23Input, M24Input,
            M31Input, M32Input, M33Input, M34Input,
            M41Input, M42Input, M43Input, M44Input
        );

        if (matrixInput != matrixEntry)
        {
            matrixEntry = matrixInput;
            var (_, euler, translate, scale) = helper.MatrixToQuaternionAndEuler(matrixInput);
            // Converting radians back to degrees for display
            UpdateEulerFields(
                new Vector3(helper.RadiansToDegrees(euler.X), helper.RadiansToDegrees(euler.Y),
                    helper.RadiansToDegrees(euler.Z)), translate, scale);

            // Now update the Matrix fields to match the new matrixEntry
            UpdateMatrixFields(matrixEntry);
        }

        isUpdatingEuler = false;

        // Update TextBox
        MatrixData = MatrixToString(matrixEntry);
    }

    private void UpdateMatrixFields(Matrix4x4 matrix)
    {
        M11Input = matrix.M11;
        M12Input = matrix.M12;
        M13Input = matrix.M13;
        M14Input = matrix.M14;
        M21Input = matrix.M21;
        M22Input = matrix.M22;
        M23Input = matrix.M23;
        M24Input = matrix.M24;
        M31Input = matrix.M31;
        M32Input = matrix.M32;
        M33Input = matrix.M33;
        M34Input = matrix.M34;
        M41Input = matrix.M41;
        M42Input = matrix.M42;
        M43Input = matrix.M43;
        M44Input = matrix.M44;

        MatrixData = MatrixToString(matrix);
    }

    private void UpdateEulerFields(Vector3 euler, Vector3 translate, Vector3 scale)
    {
        _transXInput = translate.X;
        _transYInput = translate.Y;
        _transZInput = translate.Z;
        _rotXInput = euler.X;
        _rotYInput = euler.Y;
        _rotZInput = euler.Z;
        _scaleXInput = scale.X;
        _scaleYInput = scale.Y;
        _scaleZInput = scale.Z;

        OnPropertyChanged(nameof(TransXInput));
        OnPropertyChanged(nameof(TransYInput));
        OnPropertyChanged(nameof(TransZInput));
        OnPropertyChanged(nameof(RotXInput));
        OnPropertyChanged(nameof(RotYInput));
        OnPropertyChanged(nameof(RotZInput));
        OnPropertyChanged(nameof(ScaleXInput));
        OnPropertyChanged(nameof(ScaleYInput));
        OnPropertyChanged(nameof(ScaleZInput));
    }

    private async void OpenModFile(object parameter)
    {
        var dlg = new OpenFileDialog();
        dlg.DefaultExt = ".fantome";
        dlg.Filter = "Fantome Mod File (*.fantome)|*fantome";
        dlg.Multiselect = true;

        if (dlg.ShowDialog() == true)
            foreach (var filePath in dlg.FileNames)
                using (var archive = ZipFile.OpenRead(filePath))
                {
                    var entry = archive.GetEntry("META/info.json");

                    // Initialize ModInfo with a default image
                    var info = new ModInfo
                    {
                        RouteImage = new BitmapImage(new Uri("/Images/no_thumbnail.png", UriKind.Relative))
                    };

                    // Read mod data and overwrite with actual information
                    if (entry != null)
                        using (var reader = new StreamReader(entry.Open()))
                        {
                            var jsonText = await reader.ReadToEndAsync();

                            // Deserialize the json and assign it to `info`
                            JsonConvert.PopulateObject(jsonText, info);
                        }

                    // If thumbnail image exists, replace the default image with this
                    var thumbnailEntry = archive.GetEntry("META/image.png") ?? archive.GetEntry("META/image.jpg");
                    if (thumbnailEntry != null)
                        using (var zipStream = thumbnailEntry.Open())
                        using (var memoryStream = new MemoryStream())
                        {
                            zipStream.CopyTo(memoryStream);
                            memoryStream.Position = 0;

                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = memoryStream;
                            bitmap.EndInit();

                            info.RouteImage = bitmap;
                        }

                    ModInfos.Add(info);
                }
    }

    private async void OpenLUTFile(object parameter)
    {
        try
        {
            var lutDialog = new OpenFileDialog
            {
                DefaultExt = ".cube",
                Filter = "Color Lookup Table (*.cube;*.png)|*.cube;*.png",
                Multiselect = false
            };

            if (lutDialog.ShowDialog() == true)
            {
                var lutFilePath = lutDialog.FileName; // The selected LUT file
                var lutApplier = new LutApplier();

                // Use the ProjectFolderPath
                var projectFolderPath = ProjectFolderPath; // Ensure this is bound properly
                if (string.IsNullOrWhiteSpace(projectFolderPath))
                {
                    var warningBox = new CustomMessageBox(
                        "Invalid Path",
                        PackIconKind.Warning,
                        "ProjectFolderPath is not set. Please select a valid path.",
                        "OK",
                        "",
                        Visibility.Collapsed);

                    warningBox.ShowDialog();
                    return;
                }

                // Define the specific folder paths
                var rootFolderPath = Path.Combine(projectFolderPath, "Map11", "ASSETS");
                var recoloredFolderPath = Path.Combine(projectFolderPath, "Map11", "ASSETS_recolored");

                if (!Directory.Exists(rootFolderPath))
                {
                    var alertBox = new CustomMessageBox(
                        "Folder Not Found",
                        PackIconKind.FolderAlert,
                        $"The folder '{rootFolderPath}' does not exist. Please verify the path.",
                        "OK",
                        "",
                        Visibility.Collapsed);

                    alertBox.ShowDialog();
                    return;
                }

                Directory.CreateDirectory(recoloredFolderPath);

                // Show progress bar and set loading state
                VisibilityProgressbar = Visibility.Visible;
                IsLoading = true;

                // Run the file processing method
                await ProcessAllTexAndDDSFilesAsync(rootFolderPath, recoloredFolderPath, lutFilePath, lutApplier);

                // Hide the progress bar and reset loading state
                VisibilityProgressbar = Visibility.Collapsed;
                IsLoading = false;

                // Show success message
                var infoBox = new CustomMessageBox(
                    "Success",
                    PackIconKind.Success,
                    "Your files are saved in 'ASSETS_recolored' folder, next to your project folder.",
                    "OK",
                    "",
                    Visibility.Collapsed);

                infoBox.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            // Reset properties and handle errors gracefully
            VisibilityProgressbar = Visibility.Collapsed;
            IsLoading = false;
            CurrentFileInfo = $"Error: {ex.Message}";
            var errorBox = new CustomMessageBox(
                "Error",
                PackIconKind.AlertCircleOutline,
                $"An error occurred: {ex.Message}",
                "OK",
                "",
                Visibility.Collapsed);

            errorBox.ShowDialog();
        }
    }

    private async Task ProcessAllTexAndDDSFilesAsync(string inputDirectory, string outputDirectory, string cubeFilePath,
        LutApplier lutApplier)
    {
        var ddsFiles = Directory.GetFiles(inputDirectory, "*.dds", SearchOption.AllDirectories).ToList();
        var texFiles = Directory.GetFiles(inputDirectory, "*.tex", SearchOption.AllDirectories).ToList();

        TotalFiles = ddsFiles.Count + texFiles.Count;
        CurrentProgress = 0;

        IsIndeterminate = false;

        const int maxDegreeOfParallelism = 8; // Adjust this based on your system

        // Process DDS files
        await ProcessInChunksAsync(ddsFiles,
            async file =>
            {
                await ProcessDDSFileAsync(file, inputDirectory, outputDirectory, cubeFilePath, lutApplier);
            }, maxDegreeOfParallelism);

        // Process TEX files
        await ProcessInChunksAsync(texFiles,
            async file =>
            {
                await ProcessTexFileAsync(file, inputDirectory, outputDirectory, cubeFilePath, lutApplier);
            }, maxDegreeOfParallelism);

        // Set final UI state
        CurrentFileInfo = "All files have been processed successfully!";
        VisibilityProgressbar = Visibility.Collapsed;
    }

    private async Task ProcessInChunksAsync(List<string> files, Func<string, Task> processFunc,
        int maxDegreeOfParallelism)
    {
        using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

        var tasks = new List<Task>();
        var processedFileCount = 0;

        foreach (var file in files)
        {
            await semaphore.WaitAsync(); // Limitparallelism 
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await processFunc(file); // Process the file
                }
                finally
                {
                    semaphore.Release(); // Release the semaphore

                    // Update progress safely
                    Interlocked.Increment(ref processedFileCount);
                    UpdateProgress(processedFileCount, files.Count);
                }
            }));
        }

        await Task.WhenAll(tasks); // Await all tasks
    }

    private void UpdateProgress(int processedFiles, int totalFiles)
    {
        // Calculate the progress percentage
        ProgressBarPercentage = (double)processedFiles / totalFiles * 100;

        // Update other progress-related properties
        Application.Current.Dispatcher.Invoke(() =>
        {
            CurrentProgress = processedFiles; // Actual file count processed
            VisibilityProgressbar = Visibility.Visible;

            // Update textual progress info (optional)
            CurrentFileInfo = $"Processed {processedFiles}/{totalFiles} files... ({ProgressBarPercentage:F2}%)";
        });
    }

    private async Task ProcessFilesInChunksAsync(List<string> files, int chunkSize, string cubeFilePath,
        LutApplier lutApplier)
    {
        var chunks = files
            .Select((file, index) => new { file, index })
            .GroupBy(x => x.index / chunkSize)
            .Select(group => group.Select(x => x.file).ToList())
            .ToList();

        foreach (var chunk in chunks)
            await Task.WhenAll(chunk.Select(async file =>
            {
                try
                {
                    // Define the processing logic here (e.g., for DDS files)
                    await lutApplier.ApplyLUTToDDSAsync(file, GetOutputPath(file), cubeFilePath);
                }
                catch (Exception ex)
                {
                    // Handle errors for specific files
                    Console.WriteLine($"Error processing file {file}: {ex.Message}");
                }
            }));
    }

    // Helper method to determine output path (for simplicity in this example)
    private string GetOutputPath(string inputPath)
    {
        return Path.Combine(Path.GetDirectoryName(inputPath)!, "Output", Path.GetFileName(inputPath));
    }

    private void ReportProgress(int processedFiles, int totalFiles)
    {
        if (processedFiles % Math.Max(1, totalFiles / 10) == 0)
            CurrentFileInfo = $"Processed {processedFiles}/{totalFiles} files";
    }

    private async Task ProcessDdsFileAsyncNoLut(string filePath, bool wasTexFile)
    {
        try
        {
            using (var image = new MagickImage(filePath))
            {
                var hasAlphaChannel = image.HasAlpha;
                var isAlphaChannelInvalid = hasAlphaChannel && IsAlphaChannelWhite(image);

                if (image.Compression == CompressionMethod.DXT5)
                {
                    if (!hasAlphaChannel || isAlphaChannelInvalid)
                    {
                        image.Settings.SetDefine("dds:compression", "dxt1");
                        image.Settings.SetDefine("dds:preserve-alpha", "false");
                    }
                    else
                    {
                        image.Settings.SetDefine("dds:compression", "dxt5");
                        image.Settings.SetDefine("dds:preserve-alpha", "true");
                    }
                }
                else if (image.Compression == CompressionMethod.DXT1)
                {
                    if (hasAlphaChannel && !isAlphaChannelInvalid)
                    {
                        image.Settings.SetDefine("dds:compression", "dxt5");
                        image.Settings.SetDefine("dds:preserve-alpha", "true");
                    }
                    else
                    {
                        image.Settings.SetDefine("dds:compression", "dxt1");
                        image.Settings.SetDefine("dds:preserve-alpha", "false");
                    }
                }
                else if (image.Compression == CompressionMethod.NoCompression)
                {
                    image.Settings.SetDefine("dds:compression", "none");
                }

                // Step 5: Configure mipmaps based on wasTexFile
                if (wasTexFile)
                    image.Settings.SetDefine("dds:mipmaps", "11");
                else
                    image.Settings.SetDefine("dds:mipmaps", "0");

                image.Write(filePath);
            }
        }
        catch (Exception ex)
        {
            //
        }
    }

    private async Task ProcessDDSFileAsync(string filePath, string inputDirectory, string outputDirectory,
        string cubeFilePath, LutApplier lutApplier)
    {
        // Calculate output file path, keeping folder structure intact
        var relativePath = Path.GetRelativePath(inputDirectory, filePath);
        var outputFilePath = Path.Combine(outputDirectory, relativePath);

        // Ensure the output directory for the file exists
        var outputDir = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        // Apply LUT directly to DDS file
        await lutApplier.ApplyLUTToDDSAsync(filePath, outputFilePath, cubeFilePath);
    }

    private async Task ProcessTexFileAsync(string texFilePath, string inputDirectory, string outputDirectory,
        string cubeFilePath, LutApplier lutApplier)
    {
        try
        {
            // Step 1: Convert TEX to DDS
            var ddsFilePath = await ConvertTexToDdsAsync(texFilePath);
            if (ddsFilePath == null) throw new Exception("Failed to convert TEX to DDS.");

            // Step 2: Determine the output path for the recolored DDS file
            var relativeTexPath = Path.GetRelativePath(inputDirectory, texFilePath); // Retaining folder structure
            var recoloredDdsPath = Path.Combine(outputDirectory, Path.ChangeExtension(relativeTexPath, ".dds"));

            // Ensure the output directory exists for the recolored DDS path
            var recoloredDdsDir = Path.GetDirectoryName(recoloredDdsPath);
            if (!string.IsNullOrEmpty(recoloredDdsDir) && !Directory.Exists(recoloredDdsDir))
                Directory.CreateDirectory(recoloredDdsDir);

            // Step 3: Apply LUT to the DDS file and save the processed file to the output folder
            await lutApplier.ApplyLUTToDDSAsync(ddsFilePath, recoloredDdsPath, cubeFilePath);

            // Step 4: Convert the processed DDS back to TEX, retaining the original TEX file name
            var outputTexPath = Path.Combine(outputDirectory, relativeTexPath);
            await ConvertDdsToTexAsync(recoloredDdsPath, outputTexPath);

            // Step 5: Clean up temporary DDS file
            if (File.Exists(ddsFilePath)) // Clean up the originally converted DDS
                File.Delete(ddsFilePath);
            if (File.Exists(recoloredDdsPath)) // Clean up the processed DDS
                File.Delete(recoloredDdsPath);
        }
        catch (Exception ex)
        {
            //
        }
    }

    private async Task<string?> ConvertTexToDdsAsync(string texFilePath)
    {
        try
        {
            // Define the output DDS path (same directory, replace .tex with .dds)
            var ddsFilePath = Path.ChangeExtension(texFilePath, ".dds");

            // Prepare the start info for the third-party converter
            var startInfo = new ProcessStartInfo
            {
                FileName = "OtherTools/tex2dds.exe",
                Arguments = $"\"{texFilePath}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Run the external process
            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();

                // Optionally capture logs
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();
                if (process.ExitCode != 0) throw new Exception($"Conversion failed for {texFilePath}: {error}");
            }

            // Verify the DDS output file was created
            if (File.Exists(ddsFilePath)) return ddsFilePath;

            throw new FileNotFoundException($"DDS file not found after conversion: {ddsFilePath}");
        }
        catch (Exception ex)
        {
            //
            return null;
        }
    }

    private async Task ConvertDdsToTexAsync(string ddsFilePath, string texFilePath)
    {
        // Ensure the output directory for the TEX file exists
        var outputDir = Path.GetDirectoryName(texFilePath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

        // Prepare the start info for the third-party converter
        var startInfo = new ProcessStartInfo
        {
            FileName = "OtherTools/tex2dds.exe",
            Arguments = $"\"{ddsFilePath}\"",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        // Run the external process
        using (var process = new Process { StartInfo = startInfo })
        {
            process.Start();

            // Optionally capture logs
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();
            if (process.ExitCode != 0) throw new Exception($"Conversion failed for {ddsFilePath}: {error}");
        }

        // Verify the TEX output file was created
        if (!File.Exists(texFilePath))
            throw new FileNotFoundException($"TEX file not found after conversion: {texFilePath}");
    }

    private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        var dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
            throw new DirectoryNotFoundException(
                $"Source directory does not exist or could not be found: {sourceDirName}");

        var dirs = dir.GetDirectories();

        Directory.CreateDirectory(destDirName);

        FileInfo[] files = dir.GetFiles();
        foreach (var file in files)
        {
            var tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, false);
        }

        if (copySubDirs)
            foreach (var subdir in dirs)
            {
                var tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
            }
    }

    private async void AddMap(object parameter)
    {
        // Open a folder browser dialog
        var dialog = new OpenFolderDialog();
        {
            // Check if the user selected a folder
            if (dialog.ShowDialog() == true)
            {
                // Get the folder path
                var selectedPath = dialog.FolderName;

                // Update the path
                ProjectFolderPath = selectedPath;
            }
        }
    }

    private async void GetMapFiles(object parameter)
    {
        LoadSettings();
        // Check if the path is set
        if (string.IsNullOrWhiteSpace(LeagueOfLegendsFolderPath) || !Directory.Exists(LeagueOfLegendsFolderPath))
        {
            var warningBox = new CustomMessageBox(
                "Warning",
                PackIconKind.Warning,
                "Please specify the correct League of Legends Game folder. You can find this setting under 'Main Menu -> Settings'.",
                "OK",
                "",
                Visibility.Collapsed
            );
            warningBox.ShowDialog();
            return;
        }

        if (string.IsNullOrWhiteSpace(ProjectFolderPath) || !Directory.Exists(ProjectFolderPath))
        {
            var warningBox = new CustomMessageBox(
                "Warning",
                PackIconKind.Warning,
                "Please specify the correct Project Map folder. You can find this setting under 'Add Map Project Folder' in the Mod Tools window by clicking 'Add'.",
                "OK",
                "",
                Visibility.Collapsed
            );
            warningBox.ShowDialog();
            return;
        }

        var messageBox = new CustomMessageBox(
            "Map Mod Creator",
            PackIconKind.Map,
            "Which Map do you want to create?",
            "Summoners Rift",
            "ARAM (Not working yet)"
        );
        messageBox.ShowDialog();

        // Check the user's selection
        if (messageBox.Result == MessageBoxResult.OK)
            // Map 11: Summoner's Rift
            CreateMap11();
        else if (messageBox.Result == MessageBoxResult.Cancel)
            // Map 12: ARAM
            //CreateMap12()
            ;
    }

    private async Task CreateMap11()
    {
        IsLoading = true; // Show the loading bar
        VisibilityProgressbar = Visibility.Visible; // Set progress bar visibility to visible
        IsIndeterminate = false; // Progress bar is not indeterminate (value-based progress)

        // Define paths for Map11.wad.client and Map11LEVELS.wad.client
        var mappath = Path.Combine(LeagueOfLegendsFolderPath, @"DATA\\FINAL\\Maps\\Shipping\\Map11.wad.client");
        var maplevelpath =
            Path.Combine(LeagueOfLegendsFolderPath, @"DATA\\FINAL\\Maps\\Shipping\\Map11LEVELS.wad.client");

        // Prepare hashtable from hashes.game.txt
        var hashtable = await Task.Run(() =>
        {
            Dictionary<ulong, string> table = new();
            foreach (var line in File.ReadLines("OtherTools/hashes/hashes.game.txt"))
            {
                var split = line.Split(' ', StringSplitOptions.TrimEntries);
                table.Add(Convert.ToUInt64(split[0], 16), split[1]);
            }

            return table;
        });

        // Track total files for progress calculation
        TotalFiles = 0;

        // Count total chunks for progress tracking
        TotalFiles += await Task.Run(() =>
        {
            using var wad = new WadFile(mappath);
            return wad.Chunks.Count;
        });

        TotalFiles += await Task.Run(() =>
        {
            using var wad = new WadFile(maplevelpath);
            return wad.Chunks.Count;
        });

        var processedFiles = 0;

        // Process Map11.wad.client - Move the logic to a background thread
        await Task.Run(() =>
        {
            using var wad = new WadFile(mappath);
            var hashFilePath = Path.Join(ProjectFolderPath, "Map11", "hashes.txt");

            // Ensure that the directory for 'hashes.txt' exists
            Directory.CreateDirectory(Path.GetDirectoryName(hashFilePath));

            using var hashWriter = new StreamWriter(hashFilePath, false);

            foreach (var (chunkHash, chunk) in wad.Chunks)
                if (hashtable.TryGetValue(chunkHash, out var chunkPath))
                {
                    // Update file info and progress on the UI thread
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentFileInfo = $"Exporting Map11: {chunkPath}";
                        CurrentProgress = ++processedFiles;
                    });

                    ExportFileWithHash(wad, "Map11", chunkPath, chunk, hashWriter);
                }
        });

        // Process Map11LEVELS.wad.client - Move the logic to a background thread
        await Task.Run(() =>
        {
            using var wad = new WadFile(maplevelpath);

            foreach (var (chunkHash, chunk) in wad.Chunks)
                if (hashtable.TryGetValue(chunkHash, out var chunkPath))
                {
                    // Update file info and progress on the UI thread
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentFileInfo = $"Exporting Map11LEVELS: {chunkPath}";
                        CurrentProgress = ++processedFiles;
                    });

                    ExportFileWithoutHash(wad, "Map11LEVELS", chunkPath, chunk);
                }
        });

        // Finalize progress and clean up
        ProcessAndCleanExport();
        CurrentFileInfo = "Export complete!";
        IsLoading = false;

        // Hide the progress bar
        VisibilityProgressbar = Visibility.Hidden;

        // Show completion message
        var infoBox = new CustomMessageBox(
            "Info",
            PackIconKind.Info,
            "Map11 and Map11LEVELS were successfully exported.",
            "OK",
            "",
            Visibility.Collapsed
        );

        infoBox.ShowDialog();
    }

    private void DeleteFoldersFromList(string basePath, string listFilePath)
    {
        // Check if the list file exists
        if (!File.Exists(listFilePath))
        {
            var errorBox = new CustomMessageBox(
                "Error",
                PackIconKind.Error,
                $"The list of folders to delete was not found: {listFilePath}",
                "OK",
                "",
                Visibility.Collapsed
            );
            errorBox.ShowDialog();
            return;
        }

        // Read paths from the text file
        var pathsToDelete = File.ReadAllLines(listFilePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Trim())
            .ToList();

        // Delete folders
        foreach (var relativePath in pathsToDelete)
        {
            // Absolute path based on the base path
            var fullPath = Path.Combine(basePath, relativePath.Replace('/', Path.DirectorySeparatorChar));

            // Check if the folder exists before attempting to delete
            if (Directory.Exists(fullPath))
                try
                {
                    Directory.Delete(fullPath, true); // Deletes the folder and all contents recursively
                }
                catch (Exception ex)
                {
                    //
                }
        }
    }

    private void ProcessAndCleanExport()
    {
        // Base directories for Map11 and Map11LEVELS
        var map11Folder = Path.Combine(ProjectFolderPath, "Map11");
        var map11LevelsFolder = Path.Combine(ProjectFolderPath, "Map11LEVELS");

        // Lists of folders to delete
        var map11DeleteListPath = @"Files\default_template_fullmap_map11.py";
        var map11LevelsDeleteListPath = @"Files\default_template_fullmap_map11levels.py";

        // Delete folders for Map11
        DeleteFoldersFromList(map11Folder, map11DeleteListPath);

        // Delete folders for Map11LEVELS
        DeleteFoldersFromList(map11LevelsFolder, map11LevelsDeleteListPath);
    }

    private void ExportFileWithHash(WadFile wad, string folderName, string chunkPath, WadChunk chunk,
        StreamWriter hashWriter)
    {
        var originalFilePath = chunkPath.ToLower();
        var fullPath = Path.Join(ProjectFolderPath, folderName, originalFilePath);

        // Check if file should be hashed
        var shouldHash =
            Path.GetDirectoryName(originalFilePath)?.EndsWith("data") == true &&
            Path.GetExtension(originalFilePath).Equals(".bin", StringComparison.OrdinalIgnoreCase);

        string finalFileName;

        if (shouldHash)
        {
            // Compute hashed file name
            var fileHash = ComputeHash(originalFilePath);
            finalFileName = $"{fileHash}.bin";
            fullPath = Path.Join(ProjectFolderPath, folderName, finalFileName);

            // Log hash mapping
            hashWriter.WriteLine($"{fileHash} = {originalFilePath}");
        }
        else
        {
            finalFileName = originalFilePath;
            fullPath = Path.Join(ProjectFolderPath, folderName, finalFileName);
        }

        // Ensure the directory exists
        var directoryPath = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directoryPath)) Directory.CreateDirectory(directoryPath);

        // Write the file
        using var chunkFileStream = File.Create(fullPath);
        using var chunkStream = wad.OpenChunk(chunk); // Access chunk data using WadFile
        chunkStream.CopyTo(chunkFileStream);
    }

    private void ExportFileWithoutHash(WadFile wad, string folderName, string chunkPath, WadChunk chunk)
    {
        var originalFilePath = chunkPath.ToLower();
        var fullPath = Path.Join(ProjectFolderPath, folderName, originalFilePath);

        // Ensure the directory exists
        var directoryPath = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directoryPath)) Directory.CreateDirectory(directoryPath);

        // Write the file
        using var chunkFileStream = File.Create(fullPath);
        using var chunkStream = wad.OpenChunk(chunk); // Access chunk data using WadFile
        chunkStream.CopyTo(chunkFileStream);
    }

    private void CreateMap12()
    {
        var infoBox = new CustomMessageBox(
            "Map12",
            PackIconKind.InformationOutline,
            "Creation of Map12 (ARAM) has been invoked.",
            "OK",
            "",
            Visibility.Collapsed);

        infoBox.ShowDialog();
    }

    private static string ComputeHash(string input)
    {
        var hashValue = Fnv1a.HashLower(input);
        return hashValue.ToString("x8");
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class ModInfo
    {
        public string Author { get; set; }
        public string Description { get; set; }
        public string Heart { get; set; }
        public string Home { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }

        public string ImagePath { get; set; }
        public BitmapImage RouteImage { get; set; }
    }
}