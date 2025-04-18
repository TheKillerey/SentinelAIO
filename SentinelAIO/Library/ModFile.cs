using System.Windows.Media.Imaging;

namespace SentinelAIO.Library;

public class ModFile
{
    public string Title { get; set; }
    public string Version { get; set; }
    public string AuthorName { get; set; }
    public string Description { get; set; }
    public BitmapImage Thumbnail { get; set; }
}