using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpatialIndex.RTree;

namespace Experiments
{
    internal static class Program
    {
        private const int NumImages = 5613;
        private const string ImageFilePath = @"G:\Workspace\DS&Alg-Project1-Release\data\imagelist.txt";

        private static readonly Dictionary<int, Tuple<string, int>> StrategyMap =
            new Dictionary<int, Tuple<string, int>>
            {
                {1, new Tuple<string, int>(@"G:\Workspace\DS&Alg-Project1-Release\data\color_moment.txt", 9)},
                {2, new Tuple<string, int>(@"G:\Workspace\DS&Alg-Project1-Release\data\hslbin2.txt", 6)},
                {3, new Tuple<string, int>(@"G:\Workspace\DS&Alg-Project1-Release\data\hslbin3.txt", 9)},
                {4, new Tuple<string, int>(@"G:\Workspace\DS&Alg-Project1-Release\data\hslbin4.txt", 12)},
                {5, new Tuple<string, int>(@"G:\Workspace\DS&Alg-Project1-Release\data\hslbin5.txt", 15)},
                {6, new Tuple<string, int>(@"G:\Workspace\DS&Alg-Project1-Release\data\hslbin6.txt", 18)},
                {7, new Tuple<string, int>(@"G:\Workspace\DS&Alg-Project1-Release\data\rgbbin2.txt", 6)},
                {8, new Tuple<string, int>(@"G:\Workspace\DS&Alg-Project1-Release\data\rgbbin3.txt", 9)},
                {9, new Tuple<string, int>(@"G:\Workspace\DS&Alg-Project1-Release\data\rgbbin4.txt", 12)},
                {10, new Tuple<string, int>(@"G:\Workspace\DS&Alg-Project1-Release\data\rgbbin5.txt", 15)},
                {11, new Tuple<string, int>(@"G:\Workspace\DS&Alg-Project1-Release\data\rgbbin6.txt", 18)}
            };

        private static void Shuffle<T>(this IList<T> list)
        {
            var rng = new Random(DateTime.Now.Millisecond);
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private static int GetNumDiskAccess(int rtreeSize, int strategy, int minEntry, int maxEntry)
        {
            var featurePath = StrategyMap[strategy].Item1;
            var featureDim = StrategyMap[strategy].Item2;

            var nums = Enumerable.Range(0, NumImages).ToList();
            nums.Shuffle();

            var tree = new RTree<string>(maxEntry, minEntry);

            var featureLines = File.ReadAllLines(featurePath);
            var points = featureLines.Select(s =>
            {
                var slices = s.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                return new Point(featureDim, slices.Select(float.Parse).ToList());
            }).ToList();
            var imageNames = File.ReadAllLines(ImageFilePath);

            for (var i = 0; i < rtreeSize; i++)
                tree.AddRecord(new Rectangle(featureDim, points[nums[i]], points[nums[i]]), imageNames[nums[i]]);
        }


        public static void Main(string[] args)
        {
        }
    }
}