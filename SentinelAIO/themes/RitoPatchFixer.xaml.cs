using System.Windows;
using System.Windows.Controls;
using SentinelAIO.ViewModel;

namespace SentinelAIO.Themes;

public partial class RitoPatchFixer : Window
{
    public RitoPatchFixer()
    {
        InitializeComponent();
        DataContext = new RitoPatchFixerViewModel();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void LogTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Cast sender to TextBox and scroll to the end of the content
        if (sender is TextBox textBox) textBox.ScrollToEnd();
    }
}