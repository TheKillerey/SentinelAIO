using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using SentinelAIO.Commands;
using SentinelAIO.Library;
using SentinelAIO.Windows;

namespace SentinelAIO.ViewModel;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private const string SettingsFileName = "Settings.json";
    private static MainWindowViewModel _instance;
    private bool _autoDownloadsEnabled = true;
    private string _fileInfos = string.Empty;
    private bool _isDialogOpen;
    private string _leagueOfLegendsFolderPath = string.Empty;
    private bool _tempAutoDownloadsEnabled;


    public MainWindowViewModel()
    {
        LoadSettings();
        OpenSettingsCommand = new RelayCommand(_ => OpenSettingsDialog());
        SaveSettingsCommand = new RelayCommand(_ => AcceptSettingsDialog());
        ExitCommand = new RelayCommand(_ => CancelSettingsDialog());
        ModToolsCommand = new RelayCommand(_ => GoModTools());
        BrowseFolderCommand = new RelayCommand(_ => BrowseFolder());
        // Retrieve project version dynamically
        AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
    }

    public string AppVersion { get; }

    public ICommand ModToolsCommand { get; }
    public ICommand BrowseFolderCommand { get; }

    public static MainWindowViewModel Instance => _instance ?? (_instance = new MainWindowViewModel());

    public string FileInfos
    {
        get => _fileInfos;
        set
        {
            if (_fileInfos != value)
            {
                _fileInfos = value;
                OnPropertyChanged();
            }
        }
    }

    public string LeagueOfLegendsFolderPath
    {
        get => _leagueOfLegendsFolderPath;
        set
        {
            if (_leagueOfLegendsFolderPath != value)
            {
                _leagueOfLegendsFolderPath = value;
                OnPropertyChanged();
            }
        }
    }


    public bool AutoDownloadsEnabled
    {
        get => _autoDownloadsEnabled;
        set
        {
            _autoDownloadsEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool TempAutoDownloadsEnabled
    {
        get => _tempAutoDownloadsEnabled;
        set
        {
            _tempAutoDownloadsEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool IsDialogOpen
    {
        get => _isDialogOpen;
        set
        {
            _isDialogOpen = value;
            OnPropertyChanged();
        }
    }


    public ICommand OpenSettingsCommand { get; }
    public ICommand SaveSettingsCommand { get; }
    public ICommand ExitCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SaveSettings()
    {
        var settings = new
        {
            AutoDownloadsEnabled,
            LeagueOfLegendsFolderPath
        };
        var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText(SettingsFileName, json);
    }

    private void LoadSettings()
    {
        if (File.Exists(SettingsFileName))
        {
            var json = File.ReadAllText(SettingsFileName);
            var settings = JsonConvert.DeserializeAnonymousType(json,
                new { AutoDownloadsEnabled = false, LeagueOfLegendsFolderPath = string.Empty });

            AutoDownloadsEnabled = settings.AutoDownloadsEnabled;
            LeagueOfLegendsFolderPath = settings.LeagueOfLegendsFolderPath; // Laden des Ordnerpfads
        }
    }

    private void BrowseFolder()
    {
        // WPF-spezifische Methode zum Anzeigen eines Ordnerauswahldialogs
        var dialog = new OpenFolderDialog
        {
            Title = "Choose League of Legends GAME Folder"
        };

        if (dialog.ShowDialog() == true) LeagueOfLegendsFolderPath = dialog.FolderName;
    }

    private void OpenSettingsDialog()
    {
        TempAutoDownloadsEnabled = AutoDownloadsEnabled; // Store current value in temp variable
        IsDialogOpen = true;
    }

    private void AcceptSettingsDialog()
    {
        AutoDownloadsEnabled = TempAutoDownloadsEnabled; // Commit temp variable value to actual property
        SaveSettings(); // Save settings
        IsDialogOpen = false;
    }

    private void CancelSettingsDialog()
    {
        TempAutoDownloadsEnabled = AutoDownloadsEnabled; // Revert temp variable value to the original
        IsDialogOpen = false;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void GoModTools()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var modToolsWindow = new ModTools();
            DiscordManager.Instance.UpdatePresence("Mod Tools - Home", "Mod Tools");
            modToolsWindow.Show();

            // Close the MainWindow
            (Application.Current as App)?.MainWindow.Close();
        });
    }
}