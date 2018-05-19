using System;
using SixLabors.ImageSharp;

namespace FeatureExtraction
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var image = Image.Load(@"G:\Workspace\DS&Alg-Project1-Release\data\image\n01613177_69.JPEG"))
            {
                var histgrams = RgbHistogram.GetRgbHistogram(image, 20);
                foreach (var histgram in histgrams)
                {
                    foreach (var i in histgram) Console.Write($"{i:F5} ");
                    Console.WriteLine();
                }
            }
        }
    }
}