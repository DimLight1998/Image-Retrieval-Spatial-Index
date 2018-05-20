using System.Collections.Generic;
using System.Drawing;

namespace FeatureExtraction
{
    public static class Histogram
    {
        public static IList<double[]> GetRgbHistogram(Bitmap bitmap, int numBins)
        {
            var rHist = new double[numBins];
            var gHist = new double[numBins];
            var bHist = new double[numBins];

            for (var i = 0; i < numBins; i++)
                rHist[i] = gHist[i] = bHist[i] = 0;

            for (var i = 0; i < bitmap.Width; i++)
            for (var j = 0; j < bitmap.Height; j++)
            {
                var color = bitmap.GetPixel(i, j);
                rHist[numBins * color.R / 256]++;
                gHist[numBins * color.G / 256]++;
                bHist[numBins * color.B / 256]++;
            }

            var pixels = bitmap.Width * bitmap.Height;
            for (var i = 0; i < numBins; i++)
            {
                rHist[i] /= pixels;
                gHist[i] /= pixels;
                bHist[i] /= pixels;
            }

            return new List<double[]> {rHist, gHist, bHist};
        }

        public static IList<double[]> GetHslHistogram(Bitmap bitmap, int numBins)
        {
            var hHist = new double[numBins];
            var sHist = new double[numBins];
            var lHist = new double[numBins];

            for (var i = 0; i < numBins; i++)
                hHist[i] = sHist[i] = lHist[i] = 0;

            for (var i = 0; i < bitmap.Width; i++)
            for (var j = 0; j < bitmap.Height; j++)
            {
                var color = bitmap.GetPixel(i, j);
                var hue = color.GetHue();
                var saturation = color.GetSaturation();
                var lightness = color.GetBrightness();
                hHist[(int) (numBins * hue / 360)]++;
                sHist[(int) (numBins * saturation * 100 / 101)]++;
                lHist[(int) (numBins * lightness * 100 / 101)]++;
            }

            var pixels = bitmap.Width * bitmap.Height;
            for (var i = 0; i < numBins; i++)
            {
                hHist[i] /= pixels;
                sHist[i] /= pixels;
                lHist[i] /= pixels;
            }

            return new List<double[]> {hHist, sHist, lHist};
        }
    }
}