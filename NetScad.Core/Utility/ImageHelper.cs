using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NetScad.Core.Utility
{
    public class ImageHelper
    {
        public static (double Width, double Height, double Depth, string filePath) GetPngDimensions(string filePath, bool autoSmoothFile = false)
        {
            if (autoSmoothFile)
                filePath = ApplyAutoSmooth(filePath);

            using var image = Image.Load<Rgba32>(filePath);
            var (width, height, maxZ) = GetSurfaceDimensions(image);
            return (width, height, maxZ, filePath); // Default 100mm for Z axis, OpenSCAD
        }

        public static string ApplyAutoSmooth(string inputPath, bool useGaussianBlur = true, float gaussianSigma = 2.0f, int boxRadius = 1)
        {
            // Generate outputPath by appending "_smooth" before the file extension
            string fileName = Path.GetFileNameWithoutExtension(inputPath);
            string extension = Path.GetExtension(inputPath);
            string directory = Path.GetDirectoryName(inputPath);
            string outputPath = Path.Combine(directory, $"{fileName}_smooth{extension}");

            using var image = Image.Load<Rgba32>(inputPath);
            image.Mutate(x =>
            {
                x.Grayscale(); // Ensure grayscale for heightmap
                if (useGaussianBlur)
                    x.GaussianBlur(gaussianSigma); // Smooth with Gaussian
                else
                    x.BoxBlur(boxRadius); // Smooth with Box
            });
            image.SaveAsPng(outputPath);

            return outputPath; // Return the generated path for further use
        }

        public static (float WidthMm, float HeightMm, float MaxZMm) GetSurfaceDimensions(Image<Rgba32> image)
        {
            float maxIntensity = 0;
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    var pixel = image[x, y];
                    float intensity = 0.2126f * pixel.R / 255f + 0.7152f * pixel.G / 255f + 0.0722f * pixel.B / 255f;
                    maxIntensity = Math.Max(maxIntensity, intensity);
                }

            float widthMm = image.Width;
            float heightMm = image.Height;
            float maxZMm = maxIntensity * 100f; // OpenSCAD’s 0–100 mm normalization

            return (widthMm, heightMm, maxZMm);
        }
    }
}
