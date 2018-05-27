using System;
using System.CodeDom;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SpatialIndex.RTree;

namespace Experiments
{
    internal class Program
    {
        private const int TopK = 100;
        private const int FeatureDimension = 9;
        private const string DataPath = @"G:\Workspace\DS&Alg-Project1-Release\data";
        private const string ImageFileName = "imagelist.txt";
        private const string FeatureFileName = "color_moment.txt";
        private const int RTreeMaxEntry = 30;
        private const int RTreeMinEntry = 12;

        public static void Main(string[] args)
        {
            var imageList = File.ReadAllLines(Path.Combine(DataPath, ImageFileName));
            var featureList = File.ReadAllLines(Path.Combine(DataPath, FeatureFileName));

            Debug.Assert(imageList.Length == featureList.Length);

            var rTree = new RTree<string>(RTreeMaxEntry, RTreeMinEntry);
            for (var i = 0; i < imageList.Length; i++)
            {
                var feature = featureList[i];
                var name = imageList[i];
                var slices = feature.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                var featureArr = (from slice in slices select float.Parse(slice)).ToList();
                var point = new Point(9, featureArr);
                rTree.AddRecord(new Rectangle(FeatureDimension, point, point), name);
            }

            //todo Change length / 100
            for (var i = 0; i < imageList.Length / 100; i++)
            {
                var name = imageList[i];
                var category = name.Split('_')[0];
                var feature = featureList[i];
                var slices = feature.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                var featureArr = (from slice in slices select float.Parse(slice)).ToList();
                var point = new Point(FeatureDimension, featureArr);

                var topResults = rTree.GetKNearestItems(point, TopK);
                var correctCount = 0;
                topResults.ForEach(s =>
                {
                    var resultCategory = s.Split('_')[0];
                    if (resultCategory == category)
                        correctCount++;
                });
                Console.WriteLine(name + ' ' + (double) correctCount / TopK);
            }
        }
    }
}