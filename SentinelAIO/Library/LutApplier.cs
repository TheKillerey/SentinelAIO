using System.IO;
using ImageMagick;

namespace SentinelAIO.Library;

public class LutApplier
{
    /// <summary>
    ///     Detects whether the alpha channel is completely white.
    /// </summary>
    /// <param name="image">The MagickImage to analyze.</param>
    /// <returns>True if all alpha values are white (255); otherwise, false.</returns>
    private bool IsAlphaChannelWhite(MagickImage image)
    {
        try
        {
            // Extract the alpha channel of the image
            using (var alphaChannel = image.Separate(Channels.Alpha).First())
            {
                // Check if all pixels in the alpha channel are white
                var histogram = alphaChannel.Histogram();
                foreach (var entry in histogram)
                    // A pixel is not white if the value is less than the maximum
                    if (entry.Key.ToByteArray()[0] < 255)
                        return false; // Non-white pixel found

                return true; // All pixels are white
            }
        }
        catch
        {
            return false; // In case of error, return false
        }
    }

    /// <summary>
    ///     Applies a 3D LUT from a .cube file to a .dds image using Magick.NET asynchronously.
    /// </summary>
    /// <param name="inputDdsPath">Path to the input .dds file.</param>
    /// <param name="outputDdsPath">Path to save the output .dds file.</param>
    /// <param name="cubeFilePath">Path to the .cube LUT file.</param>
    public async Task ApplyLUTToDDSAsync(string inputDdsPath, string outputDdsPath, string cubeFilePath)
    {
        if (string.IsNullOrWhiteSpace(inputDdsPath))
            throw new ArgumentException("Input DDS path cannot be null or empty.", nameof(inputDdsPath));

        if (string.IsNullOrWhiteSpace(outputDdsPath))
            throw new ArgumentException("Output DDS path cannot be null or empty.", nameof(outputDdsPath));

        if (string.IsNullOrWhiteSpace(cubeFilePath))
            throw new ArgumentException("LUT .cube file path cannot be null or empty.", nameof(cubeFilePath));

        if (!File.Exists(inputDdsPath)) throw new FileNotFoundException($"The file '{inputDdsPath}' does not exist.");

        if (!File.Exists(cubeFilePath))
            throw new FileNotFoundException($"The LUT file '{cubeFilePath}' does not exist.");

        await Task.Run(() =>
        {
            using (var image = new MagickImage(inputDdsPath))
            {
                // Dynamically handle the raw LUT image based on file extension
                using (var rawLutImage = Path.GetExtension(cubeFilePath).ToLower() switch
                       {
                           ".cube" => new MagickImage($"cube:{cubeFilePath}"), // Handle as LUT (cube format)
                           ".png" => new MagickImage($"image:{cubeFilePath}"), // Handle as an image
                           _ => throw new NotSupportedException(
                               "Unsupported LUT file format. Please provide a .cube or .png file.")
                       })
                {
                    if (image.Format != MagickFormat.Unknown)
                    {
                        // Check if the input image has an alpha channel (DXT5) or not (DXT1)
                        var hasAlphaChannel = image.HasAlpha;

                        // Detect if alpha channel is either absent or completely white
                        var isAlphaChannelInvalid = hasAlphaChannel && IsAlphaChannelWhite(image);

                        IMagickImage<byte>? alphaChannel = null;

                        // If alpha channel exists, extract and preserve it
                        if (hasAlphaChannel) alphaChannel = image.Separate(Channels.Alpha).FirstOrDefault();

                        // Apply the LUT to the image (affects RGB channels only)
                        image.HaldClut(rawLutImage);

                        // If alpha channel exists, reapply it back to the image
                        if (hasAlphaChannel && alphaChannel is not null)
                            image.Composite(alphaChannel, CompositeOperator.CopyAlpha);

                        // DDS-specific compression settings
                        if (image.Compression == CompressionMethod.DXT5)
                        {
                            if (!hasAlphaChannel || isAlphaChannelInvalid)
                            {
                                image.Settings.SetDefine("dds:compression", "dxt1");
                                image.Settings.SetDefine("dds:preserve-alpha", "false");
                                image.Settings.SetDefine("dds:mipmaps", "11");
                            }
                            else
                            {
                                image.Settings.SetDefine("dds:compression", "dxt5");
                                image.Settings.SetDefine("dds:preserve-alpha", "true");
                                image.Settings.SetDefine("dds:mipmaps", "11");
                            }
                        }
                        else if (image.Compression == CompressionMethod.DXT1)
                        {
                            if (hasAlphaChannel && !isAlphaChannelInvalid)
                            {
                                image.Settings.SetDefine("dds:compression", "dxt5");
                                image.Settings.SetDefine("dds:preserve-alpha", "true");
                                image.Settings.SetDefine("dds:mipmaps", "11");
                            }
                            else
                            {
                                image.Settings.SetDefine("dds:compression", "dxt1");
                                image.Settings.SetDefine("dds:preserve-alpha", "false");
                                image.Settings.SetDefine("dds:mipmaps", "11");
                            }
                        }
                        else if (image.Compression == CompressionMethod.NoCompression)
                        {
                            // Uncompressed
                            image.Settings.SetDefine("dds:compression", "none");
                        }

                        // Save the processed image back to DDS format
                        image.Write(outputDdsPath, MagickFormat.Dds);
                    }
                }
            }
        });
    }
}