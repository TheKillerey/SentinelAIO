using System.Windows;
using MaterialDesignThemes.Wpf;

namespace SentinelAIO.Themes;

public partial class CustomMessageBox : Window
{
    public CustomMessageBox(string title, PackIconKind messageIconKind, string message, string okButtonText = "OK",
        string cancelButtonText = "Cancel", Visibility isCancelButtonEnabled = Visibility.Visible)
    {
        InitializeComponent();

        // Setze die dynamischen Werte für Titel, Nachricht und Buttons
        Title = title;
        MessageIcon.Kind = messageIconKind;
        MessageText.Text = message;
        BtnOk.Content = okButtonText;
        BtnCancel.Content = cancelButtonText;

        // Set default cancel button state
        BtnCancel.Visibility = isCancelButtonEnabled;

        // Standardwert für das Ergebnis
        Result = MessageBoxResult.None;
    }

    // Das Ergebnis der Benutzeraktion (z. B. OK oder Abbrechen)
    public MessageBoxResult Result { get; private set; }

    public void EnableCancelButton(bool isEnabled)
    {
        BtnCancel.IsEnabled = isEnabled;
    }

    // Schaltflächen-Ereignisse
    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.OK;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.Cancel;
        Close();
    }
}