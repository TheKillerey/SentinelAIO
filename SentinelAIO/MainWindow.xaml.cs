using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DiscordRPC;
using MaterialDesignThemes.Wpf;
using SentinelAIO.Library;
using SentinelAIO.ViewModel;
using SentinelAIO.Windows;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using Path = System.IO.Path;

namespace SentinelAIO;

public partial class MainWindow : Window
{
    private const string LiveHashes = "https://raw.communitydragon.org/data/hashes/lol/";
    private const string HashesDirectory = "OtherTools/hashes";

    private readonly string[] filenames =
    {
        "hashes.game.txt",
        "hashes.lcu.txt",
        "hashes.binfields.txt",
        "hashes.bintypes.txt",
        "hashes.binhashes.txt",
        "hashes.binentries.txt"
    };

    private readonly HttpClient httpClient = new();

    private readonly Queue<string> urlsToDownload;
    private readonly WebClient webClient = new();
    private string currentFilePath;

    // keep track of current url and destination path
    private string currentUrl;

    // Discord RPC fields
    private DiscordRpcClient rpcClient;


    public MainWindow()
    {
        DataContext = MainWindowViewModel.Instance;

        InitializeComponent();

        // Retrieve project version dynamically
        AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";


        InfoBox.MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3)); // choose appropriate timespan
        DiscordManager.Instance.UpdatePresence("Mod Tools Selection", "Home");
        EnsureHashesDirectoryExists();
        var mainWindowViewModel = MainWindowViewModel.Instance;


        if (mainWindowViewModel.AutoDownloadsEnabled)
        {
            urlsToDownload = new Queue<string>(filenames.Select(f => $"{LiveHashes}{f}"));

            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;

            DownloadProgressBar.Value = 0;
            DownloadProgressBar.Visibility = Visibility.Hidden;
            UpdateHashesText.Visibility = Visibility.Hidden;

            // Start the download

            StartDownloadNextFileFromList();
        }

        else
        {
            DownloadProgressBar.Value = 0;
            DownloadProgressBar.Visibility = Visibility.Hidden;
            UpdateHashesText.Visibility = Visibility.Hidden;
        }
    }

    public string AppVersion { get; set; }


    private void EnsureHashesDirectoryExists()
    {
        if (!Directory.Exists(HashesDirectory)) Directory.CreateDirectory(HashesDirectory);
    }

    private async void StartDownloadNextFileFromList()
    {
        if (urlsToDownload.Count > 0)
        {
            currentUrl = urlsToDownload.Dequeue();
            currentFilePath = Path.Combine(HashesDirectory, Path.GetFileName(currentUrl));

            DownloadProgressBar.Visibility = Visibility.Visible;
            UpdateHashesText.Visibility = Visibility.Visible;
            if (File.Exists(currentFilePath) && await IsNewerVersionAvailable(currentUrl, currentFilePath))
            {
                // File exists and a newer version is available, download it
                //ShowCustomMessageBox("File exists and a newer version is available, download it", "INFO :3");
                ((MainWindowViewModel)DataContext).FileInfos = $"Update Hashes: {Path.GetFileName(currentUrl)}";
                webClient.DownloadFileAsync(new Uri(currentUrl), currentFilePath);
            }
            else
            {
                if (!File.Exists(currentFilePath))
                {
                    ((MainWindowViewModel)DataContext).FileInfos = $"Download Hashes: {Path.GetFileName(currentUrl)}";
                    webClient.DownloadFileAsync(new Uri(currentUrl), currentFilePath);
                }
                else
                {
                    // File exists and it is up-to-date
                    // Go to next download
                    //ShowCustomMessageBox("File exists and it is up-to-date", "INFO :3");
                    ((MainWindowViewModel)DataContext).FileInfos = $"Hash File exists: {Path.GetFileName(currentUrl)}";
                    WebClient_DownloadFileCompleted(null, null);
                }
            }
        }
        else
        {
            // All files have been downloaded, hide the progress bar
            DownloadProgressBar.Visibility = Visibility.Hidden;
            UpdateHashesText.Visibility = Visibility.Hidden;
            InfoBox.MessageQueue.Enqueue("Updating Hashes done!");
        }
    }

    private async Task<bool> IsNewerVersionAvailable(string url, string filePath)
    {
        var response = await httpClient.GetAsync(new Uri(url), HttpCompletionOption.ResponseHeadersRead);
        if (response.Content.Headers.LastModified is var lm && lm.HasValue)
        {
            var lastWriteTime = File.GetLastWriteTime(filePath);
            return lm.Value.LocalDateTime > lastWriteTime;
        }

        return false;
    }

    private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        DownloadProgressBar.Value = e.ProgressPercentage;
    }

    private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        if (e?.Error == null)
            // If there's no error download next file in the queue
            StartDownloadNextFileFromList();
        else
            // An error occurred during download
            ShowCustomMessageBox("Error downloading file. " + e.Error.Message, "ERROR!");
        // Optionally restart the current download
        // StartDownloadNextFileFromList(); 
    }

    public void ShowCustomMessageBox(string message, string title)
    {
        var customMessageBox = new Window
        {
            ResizeMode = ResizeMode.NoResize,
            Width = 300,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            WindowStyle = WindowStyle.None,
            Background = new SolidColorBrush(Color.FromRgb(4, 25, 26))
        };

        var border = new Border // Add a border to the window
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(52, 140, 132)), // border color
            BorderThickness = new Thickness(2)
        };

        var textBlockTitle = new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)), // text color
            FontSize = 16,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 10, 0, 20)
        };

        var textBlockMessage = new TextBlock
        {
            Text = message,
            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)), // text color
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var buttonOk = new Button
        {
            Content = "OK",
            BorderBrush = new SolidColorBrush(Color.FromRgb(52, 140, 132)),
            BorderThickness = new Thickness(2),
            Width = 75, HorizontalAlignment = HorizontalAlignment.Center,
            Background = new SolidColorBrush(Color.FromRgb(12, 106, 78)), // button color
            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)) // button text color
        };

        var stackPanel = new StackPanel { Margin = new Thickness(20) };
        stackPanel.Children.Add(textBlockTitle);
        stackPanel.Children.Add(textBlockMessage);
        stackPanel.Children.Add(buttonOk);

        border.Child = stackPanel;
        customMessageBox.Content = border;

        buttonOk.Click += (s, e) => customMessageBox.Close();

        customMessageBox.ShowDialog();
    }

    private void ModToolsBtn_OnClickModToolsBtn_Click(object sender, RoutedEventArgs e)
    {
        new ModTools().Show();
        DiscordManager.Instance.UpdatePresence("Mod Tools - Home", "Mod Tools");
        Close();
    }
}