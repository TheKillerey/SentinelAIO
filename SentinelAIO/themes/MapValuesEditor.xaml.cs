using System.Windows;
using SentinelAIO.Library;

namespace SentinelAIO.Themes;

public partial class MapValuesEditor : Window
{
    public MapValuesEditor(MapReplacementValues existingValues)
    {
        InitializeComponent();

        // Initialize the MapValues object with existing values
        MapValues = existingValues ?? new MapReplacementValues();

        // Bind the DataContext for use in XAML
        DataContext = MapValues;
    }

    public MapReplacementValues MapValues { get; }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        // Close the window and indicate that changes were saved
        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        // Close the window without saving
        DialogResult = false;
        Close();
    }
}