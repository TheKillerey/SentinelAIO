using System.Runtime.InteropServices;
using System.Windows;
using SentinelAIO.Library;
using SentinelAIO.ViewModel;

namespace SentinelAIO.Windows;

public partial class ModTools : Window
{
    public ModTools()
    {
        DataContext = ModToolsViewModel.Instance;
        InitializeComponent();
    }

    private void GoHomeBtn_OnClick(object sender, RoutedEventArgs e)
    {
        new MainWindow().Show();
        DiscordManager.Instance.UpdatePresence("Mod Tools Selection", "Home");
        Close();
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is string selectedMesh)
            if (DataContext is ModToolsViewModel viewModel)
                viewModel.SelectedMeshName = selectedMesh; // Update ViewModel with selected mesh name
    }


    private void CopyBtn_OnClick(object sender, RoutedEventArgs e)
    {
        const int maxRetries = 10;
        var retries = 0;
        var success = false;

        while (!success)
            try
            {
                Clipboard.SetDataObject(OutputTb.Text, false);
                success = true;
            }
            catch (Exception ex) when (ex is COMException || ex is ExternalException)
            {
                if (++retries >= maxRetries) throw;

                Thread.Sleep(100);
            }
    }
}