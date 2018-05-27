using System;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Configuration;
using System.Threading.Tasks;

namespace FeatureExtraction
{
    internal class Program
    {
        private const string ImageFileRoot = @"G:\Workspace\DS&Alg-Project1-Release\data\image";
        private const string ImageListPath = @"G:\Workspace\DS&Alg-Project1-Release\data\imagelist.txt";

        private const string OutputFeatureFullPath = @"G:\Workspace\DS&Alg-Project1-Release\data\hslbin3.txt";

        private const bool UseRgbBins = false;
        private const int RgbBins = 4;

        private const bool UseHslBins = true;
        private const int HslBins = 3;

        public static void Main(string[] args)
        {
            var images = File.ReadAllLines(ImageListPath);

            if (UseRgbBins)
            {
                var progress = 0;
                using (var writer = new StreamWriter(OutputFeatureFullPath))
                    foreach (var image in images)
                    {
                        var fullPath = Path.Combine(ImageFileRoot, image);
                        var bitmap = new Bitmap(fullPath);

                        var hist = Histogram.GetRgbHistogram(bitmap, RgbBins);
                        var sb = new StringBuilder();
                        sb.Append(string.Join(" ", hist[0].Select(d => ((int) (1000 * d)).ToString())));
                        sb.Append(' ');
                        sb.Append(string.Join(" ", hist[1].Select(d => ((int) (1000 * d)).ToString())));
                        sb.Append(' ');
                        sb.Append(string.Join(" ", hist[2].Select(d => ((int) (1000 * d)).ToString())));

                        writer.WriteLine(sb.ToString());
                        Console.WriteLine(progress++);
                    }
            }
            else if (UseHslBins)
            {
                var progress = 0;
                using (var writer = new StreamWriter(OutputFeatureFullPath))
                    foreach (var image in images)
                    {
                        var fullPath = Path.Combine(ImageFileRoot, image);
                        var bitmap = new Bitmap(fullPath);

                        var hist = Histogram.GetHslHistogram(bitmap, HslBins);
                        var sb = new StringBuilder();
                        sb.Append(string.Join(" ", hist[0].Select(d => ((int) (1000 * d)).ToString())));
                        sb.Append(' ');
                        sb.Append(string.Join(" ", hist[1].Select(d => ((int) (1000 * d)).ToString())));
                        sb.Append(' ');
                        sb.Append(string.Join(" ", hist[2].Select(d => ((int) (1000 * d)).ToString())));

                        writer.WriteLine(sb.ToString());
                        Console.WriteLine(progress++);
                    }
            }
        }
    }
}