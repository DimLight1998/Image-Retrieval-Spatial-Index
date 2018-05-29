using System.Drawing;

namespace FeatureExtraction
{
    internal class Program
    {
        public static void Main(string[] args)
        {
//            var images = File.ReadAllLines(@"G:\Workspace\DS&Alg-Project1-Release\data\imagelist.txt").ToList();
//            var features = new List<Tuple<string, string>>(images.Count);
//
//            var count = 0;
//            var featureLock = new object();
//
//            Parallel.ForEach(images, image =>
//            {
//                var feature = Process.GetBinaryFeature(
//                    new Bitmap($@"G:\Workspace\DS&Alg-Project1-Release\data\image\{image}"), 4, 4);
//
//                lock (featureLock)
//                {
//                    features.Add(new Tuple<string, string>(image, string.Join(" ", feature)));
//                }
//
//                Console.WriteLine(count++);
//            });
//
//            features.Sort((lhs, rhs) => images.IndexOf(lhs.Item1).CompareTo(images.IndexOf(rhs.Item2)));
//
//            File.WriteAllLines(@"G:\Workspace\DS&Alg-Project1-Release\data\bin4x4.txt",
//                from feature in features select feature.Item2);
//
//            var lines = File.ReadAllLines(@"G:\Workspace\DS&Alg-Project1-Release\data\bin4x4.txt");
//            var converted = lines.ToList().Select(s =>
//            {
//                var slices = s.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
//                var ints = slices.Select(str => (int) (1000 * double.Parse(str)));
//                return string.Join(" ", ints);
//            });
//
//            File.WriteAllLines(@"G:\Workspace\DS&Alg-Project1-Release\data\bin4x4i.txt", converted);

            var bit = Process.GetBinaryBitmap(
                new Bitmap(@"G:\Workspace\DS&Alg-Project1-Release\data\image\n11669921_1884.JPEG"));
            bit.Save("a.jpg");
        }
    }
}