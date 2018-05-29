using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;

namespace FeatureExtraction
{
    public static class Process
    {
        public static Bitmap GetGreyBitmap(Bitmap bitmap)
        {
            var processing = new Bitmap(bitmap.Width, bitmap.Height);
            for (var i = 0; i < bitmap.Width; i++)
            for (var j = 0; j < bitmap.Height; j++)
                processing.SetPixel(i, j, GetGreyColor(bitmap.GetPixel(i, j)));
            return processing;
        }

        public static Bitmap GetBinaryBitmap(Bitmap bitmap)
        {
            var greyBitmap = GetGreyBitmap(bitmap);

            var pixelPerStep = new double[256];
            for (var i = 0; i < 256; i++)
                pixelPerStep[i] = 0;

            for (var i = 0; i < greyBitmap.Width; i++)
            for (var j = 0; j < greyBitmap.Height; j++)
                pixelPerStep[greyBitmap.GetPixel(i, j).G]++;

            var prefixSum = new double[256];
            prefixSum[0] = pixelPerStep[0];
            for (var i = 1; i < 256; i++)
                prefixSum[i] = prefixSum[i - 1] + pixelPerStep[i];

            var postfixSum = new double[256];
            postfixSum[255] = pixelPerStep[255];
            for (var i = 254; i >= 0; i--)
                postfixSum[i] = postfixSum[i + 1] + pixelPerStep[i];

            var prefixAvg = new double[256];
            prefixAvg[0] = 0;
            for (var i = 1; i < 256; i++)
                prefixAvg[i] = (prefixAvg[i - 1] * prefixSum[i - 1] + i * pixelPerStep[i]) / prefixSum[i];

            var postfixAvg = new double[256];
            postfixAvg[255] = 255;
            for (var i = 254; i >= 0; i--)
                postfixAvg[i] = (postfixAvg[i + 1] * postfixSum[i + 1] + i * pixelPerStep[i]) / postfixSum[i];

            var totalPixels = bitmap.Width * bitmap.Height;
            for (var i = 0; i < 256; i++)
                postfixSum[i] /= totalPixels;
            for (var i = 0; i < 256; i++)
                prefixAvg[i] /= totalPixels;

            var maxDiff = 0d;
            var bestThreshold = 0;
            for (var threshold = 1; threshold < 255; threshold++)
            {
                var diff = prefixSum[threshold - 1] * postfixSum[threshold] *
                           Math.Pow(prefixAvg[threshold - 1] - postfixAvg[threshold], 2);
                if (diff <= maxDiff) continue;
                maxDiff = diff;
                bestThreshold = threshold;
            }

            for (var i = 0; i < greyBitmap.Width; i++)
            for (var j = 0; j < greyBitmap.Height; j++)
                greyBitmap.SetPixel(i, j, greyBitmap.GetPixel(i, j).R < bestThreshold ? Color.Black : Color.White);

            return greyBitmap;
        }

        public static List<double> GetBinaryFeature(Bitmap bitmap, int numCols, int numRows)
        {
            var ret = new List<double>();
            var binaryBitmap = GetBinaryBitmap(bitmap);
            var horiLen = binaryBitmap.Width / numCols;
            var vertLen = binaryBitmap.Height / numRows;
            for (var i = 0; i < numCols; i++)
            for (var j = 0; j < numRows; j++)
            {
                var colStart = i * horiLen;
                var rowStart = j * vertLen;

                var blackCount = 0;
                var total = horiLen * vertLen;
                for (var c = colStart; c < colStart + horiLen; c++)
                for (var r = rowStart; r < rowStart + vertLen; r++)
                {
                    if (bitmap.GetPixel(c, r).R < 128)
                        blackCount++;
                }

                var ratio = (double)blackCount / total;
                ret.Add(ratio);
            }

            return ret;
        }

        private static Color GetGreyColor(Color color)
        {
            var grey = (int) (color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
            return Color.FromArgb(grey, grey, grey);
        }
    }
}