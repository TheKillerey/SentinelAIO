using System.Windows;
using System.Windows.Controls;
using SentinelAIO.ViewModel;

namespace SentinelAIO.themes;

public partial class ManifestDownloader : Window
{
    public ManifestDownloader()
    {
        InitializeComponent();
    }

    private void UIElement_OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
            foreach (var file in files)
            {
                var viewModel = DataContext as ManifestDownloaderViewModel;
                if (viewModel != null && file.EndsWith(".wad.client")) viewModel.ExtractFileCommand.Execute(file);
            }
    }

    private void LogTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Cast sender to TextBox and scroll to the end of the content
        if (sender is TextBox textBox) textBox.ScrollToEnd();
    }
}