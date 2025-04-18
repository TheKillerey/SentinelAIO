using System.Windows;
using ShowMeTheXAML;

namespace SentinelAIO;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        XamlDisplay.Init();
        base.OnStartup(e);
    }
}