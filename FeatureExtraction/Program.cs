using System;
using System.Drawing;

namespace FeatureExtraction
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var bitmap = Image.FromFile(@"G:\Workspace\DS&Alg-Project1-Release\data\image\n01613177_69.JPEG") as Bitmap;
            var rgbHists = Histogram.GetHslHistogram(bitmap, 30);
            foreach (var hist in rgbHists)
            {
                foreach (var d in hist) Console.WriteLine(d + " ");
                Console.WriteLine("\n");
            }
        }
    }
}