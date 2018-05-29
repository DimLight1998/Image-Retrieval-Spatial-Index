using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpatialIndex.RTree;

namespace Experiments
{
    internal static class Program
    {
        private const int NumImages = 5613;
        private const string ImageFilePath = @"Extra\imagelist.txt";

        private static readonly Dictionary<int, Tuple<string, int>> StrategyMap =
            new Dictionary<int, Tuple<string, int>>
            {
                {1, new Tuple<string, int>(@"Extra\color_moment.txt", 9)},
                {2, new Tuple<string, int>(@"Extra\hslbin2.txt", 6)},
                {3, new Tuple<string, int>(@"Extra\hslbin3.txt", 9)},
                {4, new Tuple<string, int>(@"Extra\hslbin4.txt", 12)},
                {5, new Tuple<string, int>(@"Extra\hslbin5.txt", 15)},
                {6, new Tuple<string, int>(@"Extra\hslbin6.txt", 18)},
                {7, new Tuple<string, int>(@"Extra\rgbbin2.txt", 6)},
                {8, new Tuple<string, int>(@"Extra\rgbbin3.txt", 9)},
                {9, new Tuple<string, int>(@"Extra\rgbbin4.txt", 12)},
                {10, new Tuple<string, int>(@"Extra\rgbbin5.txt", 15)},
                {11, new Tuple<string, int>(@"Extra\rgbbin6.txt", 18)},
                {12, new Tuple<string, int>(@"Extra\bin4x4.txt", 16)}
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

        private static double GetAccuracy(int strategy, int numTopK)
        {
            const int minEntry = 12;
            const int maxEntry = 30;

            var featurePath = StrategyMap[strategy].Item1;
            var featureDim = StrategyMap[strategy].Item2;

            var tree = new RTree<string>(maxEntry, minEntry);

            var featureLines = File.ReadAllLines(featurePath);
            var points = featureLines.Select(s =>
            {
                var slices = s.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                return new Point(featureDim, slices.Select(double.Parse).ToList());
            }).ToList();

            var imageNames = File.ReadAllLines(ImageFilePath);

            for (var i = 0; i < NumImages; i++)
                tree.AddRecord(new Rectangle(featureDim, points[i], points[i]), imageNames[i]);

            var totalAccuracy = 0d;
            for (var i = 0; i < NumImages; i++)
            {
                var results = tree.GetKNearestItems(points[i], numTopK);
                results.Remove(imageNames[i]);
                var total = results.Count;
                var count = 0;
                foreach (var result in results)
                    if (result.Split('_')[0] == imageNames[i].Split('_')[0])
                        count++;

                var accuracy = (double) count / total;
                totalAccuracy += accuracy;
            }

            return totalAccuracy / NumImages;
        }

        private static double GetRecall(int strategy)
        {
            const int minEntry = 12;
            const int maxEntry = 30;
            var tree = new RTree<string>(maxEntry, minEntry);

            var featurePath = StrategyMap[strategy].Item1;
            var featureDim = StrategyMap[strategy].Item2;

            var featureLines = File.ReadAllLines(featurePath);
            var points = featureLines.Select(s =>
            {
                var slices = s.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                return new Point(featureDim, slices.Select(double.Parse).ToList());
            }).ToList();

            var imageNames = File.ReadAllLines(ImageFilePath);

            for (var i = 0; i < NumImages; i++)
                tree.AddRecord(new Rectangle(featureDim, points[i], points[i]), imageNames[i]);

            var dict = new Dictionary<string, int>();
            for (var i = 0; i < NumImages; i++)
            {
                var category = imageNames[i].Split('_')[0];
                if (dict.Keys.Contains(category)) dict[category]++;
                else dict[category] = 1;
            }

            var totalRecall = 0d;
            for (var i = 0; i < NumImages; i++)
            {
                var category = imageNames[i].Split('_')[0];
                var results = tree.GetKNearestItems(points[i], dict[category]);
                var count = 0;
                foreach (var result in results)
                    if (result.Split('_')[0] == category)
                        count++;

                var recall = (double) count / dict[category];
                totalRecall += recall;

                if (i % 100 == 0)
                    Console.Error.WriteLine(i);
            }

            return totalRecall / NumImages;
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
                return new Point(featureDim, slices.Select(double.Parse).ToList());
            }).ToList();

            var imageNames = File.ReadAllLines(ImageFilePath);

            for (var i = 0; i < rtreeSize; i++)
                tree.AddRecord(new Rectangle(featureDim, points[nums[i]], points[nums[i]]), imageNames[nums[i]]);

            var mbr = tree.GetRootMbr();
            var ratio = Math.Pow(1d / 2, 1d / featureDim);

            const int queryTimes = 1024;
            var countSum = 0;

            for (var i = 0; i < queryTimes; i++)
            {
                var queryMinBound = Enumerable.Repeat(0d, featureDim).ToList();
                var queryMaxBound = Enumerable.Repeat(0d, featureDim).ToList();

                for (var dim = 0; dim < featureDim; dim++)
                {
                    var length = mbr.MaxBoundries[dim] - mbr.MinBoundries[dim];
                    var sideLength = length * ratio;
                    var start = mbr.MinBoundries[dim] + new Random(DateTime.Now.Millisecond).NextDouble() *
                                (mbr.MaxBoundries[dim] - sideLength - mbr.MinBoundries[dim]);
                    var end = start + sideLength;
                    queryMinBound[dim] = start;
                    queryMaxBound[dim] = end;
                }

                var queryRect = new Rectangle(featureDim, queryMinBound, queryMaxBound);
                tree.GetContainedItems(queryRect, out var count);
                countSum += count;
            }

            return countSum / queryTimes;
        }

        public static void Main(string[] args)
        {
            var type = args[0];
            switch (type)
            {
                case "gnda":
                {
                    var size = int.Parse(args[1]);
                    var strategy = int.Parse(args[2]);
                    var minEntry = int.Parse(args[3]);
                    var maxEntry = int.Parse(args[4]);
                    Console.WriteLine(GetNumDiskAccess(size, strategy, minEntry, maxEntry));
                    break;
                }
                case "accu":
                {
                    var strategy = int.Parse(args[1]);
                    var numTopK = int.Parse(args[2]);
                    Console.WriteLine(GetAccuracy(strategy, numTopK));
                    break;
                }
                case "recl":
                {
                    var strategy = int.Parse(args[1]);
                    Console.WriteLine(GetRecall(strategy));
                    break;
                }
                default:
                {
                    Console.WriteLine("Error!\n");
                    break;
                }
            }
        }
    }
}