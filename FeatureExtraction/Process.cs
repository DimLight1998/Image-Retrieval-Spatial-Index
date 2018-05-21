﻿using System;
using System.Drawing;

namespace FeatureExtraction
{
    /// <summary>
    ///     图像预处理相关的函数
    /// </summary>
    public static class Process
    {
        /// <summary>
        ///     得到一个图像灰度化后的图像
        /// </summary>
        /// <param name="bitmap">需要灰度化的图像</param>
        /// <returns>灰度化后的图像</returns>
        public static Bitmap GetGreyBitmap(Bitmap bitmap)
        {
            var processing = new Bitmap(bitmap.Width, bitmap.Height);
            for (var i = 0; i < bitmap.Width; i++)
            for (var j = 0; j < bitmap.Height; j++)
                processing.SetPixel(i, j, GetGreyColor(bitmap.GetPixel(i, j)));
            return processing;
        }

        /// <summary>
        ///     得到一个图像二值化后的图像
        /// </summary>
        /// <param name="bitmap">需要二值化的图像</param>
        /// <returns>二值化后的图像</returns>
        /// <see cref="http://www.ruanyifeng.com/blog/2013/03/similar_image_search_part_ii.html" />
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

        private static Color GetGreyColor(Color color)
        {
            var grey = (int) (color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
            return Color.FromArgb(grey, grey, grey);
        }
    }
}