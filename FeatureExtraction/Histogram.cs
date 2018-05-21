using System.Collections.Generic;
using System.Drawing;

namespace FeatureExtraction
{
    /// <summary>
    ///     颜色直方图有关的函数
    /// </summary>
    public static class Histogram
    {
        /// <summary>
        ///     得到给定图像的 RGB 颜色直方图
        /// </summary>
        /// <param name="bitmap">需要直方图的图像</param>
        /// <param name="numBins">每个通道中直方图的桶数</param>
        /// <returns>一个具有三个元素的列表，每个元素都是一个数组（代表直方图），三个元素分别代表 R，G，B 通道的直方图</returns>
        /// <remarks>每个通道产生的直方图都有 <c>numBins</c> 个桶，每个数的值为这个桶中的元素占所有元素的比</remarks>
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

        /// <summary>
        ///     得到给定图像的 HSL 颜色直方图
        /// </summary>
        /// <param name="bitmap">需要直方图的图像</param>
        /// <param name="numBins">每个通道中直方图的桶数</param>
        /// <returns>一个具有三个元素的列表，每个元素都是一个数组（代表直方图），三个元素分别代表 H，S，L 通道的直方图</returns>
        /// <remarks>每个通道产生的直方图都有 <c>numBins</c> 个桶，每个数的值为这个桶中的元素占所有元素的比</remarks>
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