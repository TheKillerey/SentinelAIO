using System.Drawing;

namespace SentinelAIO.Library;

public class MaskChecker
{
    /// <summary>
    ///     Überprüft, ob eine Bitmap nur Grautöne enthält.
    /// </summary>
    /// <param name="bitmap">Die zu überprüfende Bitmap.</param>
    /// <returns>True, wenn das Bild nur Grautöne enthält; andernfalls False.</returns>
    public bool IsMaskImage(Bitmap bitmap)
    {
        for (var y = 0; y < bitmap.Height; y++)
        for (var x = 0; x < bitmap.Width; x++)
        {
            var pixelColor = bitmap.GetPixel(x, y);

            // Check if we have grey color
            if (!IsGrayScale(pixelColor)) return false;
        }

        return true; // All pixel's where grey
    }

    /// <summary>
    ///     Überprüft, ob eine Farbe ein Grauton ist.
    /// </summary>
    /// <param name="color">Die zu überprüfende Farbe.</param>
    /// <returns>True, wenn die Farbe ein Grauton ist; andernfalls False.</returns>
    private bool IsGrayScale(Color color)
    {
        // Ein Pixel gilt als Grauton, wenn (R,G,B) die gleiche Werte haben
        return color.R == color.G && color.G == color.B;
    }
}