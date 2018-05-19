using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FeatureExtraction
{
    public static class RgbHistogram
    {
        /// <summary>
        ///     Get normalized histrogram for r, g and b channels.
        /// </summary>
        /// <param name="image">Image to be processed.</param>
        /// <param name="numBins">Number of bins in each channel.</param>
        /// <returns></returns>
        /// Normalized histrogram for r, g and b channels.
        public static List<double[]> GetRgbHistogram(Image<Rgba32> image, int numBins)
        {
            if (numBins < 1 || numBins > 255)
                throw new Exception("Error bin number.");

            var rHist = new double[numBins];
            var gHist = new double[numBins];
            var bHist = new double[numBins];

            for (var i = 0; i < numBins; i++)
                rHist[i] = gHist[i] = bHist[i] = 0;

            for (var i = 0; i < image.Height; i++)
            for (var j = 0; j < image.Width; j++)
            {
                rHist[numBins * image[j, i].R / 256]++;
                gHist[numBins * image[j, i].G / 256]++;
                bHist[numBins * image[j, i].B / 256]++;
            }

            for (var i = 0; i < numBins; i++)
            {
                rHist[i] /= image.Height * image.Width;
                gHist[i] /= image.Height * image.Width;
                bHist[i] /= image.Height * image.Width;
            }

            return new List<double[]> {rHist, gHist, bHist};
        }
    }
}