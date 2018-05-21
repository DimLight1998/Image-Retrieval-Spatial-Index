using System.Drawing;

namespace FeatureExtraction
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var bitmap =
                Image.FromFile(@"G:\Workspace\DS&Alg-Project1-Release\data\image\n04583620_4657.JPEG") as Bitmap;
            var grey = Process.GetBinaryBitmap(bitmap);
            grey.Save("test.png");
        }
    }
}