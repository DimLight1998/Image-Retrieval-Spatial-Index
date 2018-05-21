using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpatialIndex.RTree
{
    /// <summary>
    ///     高维的点，单浮点精度
    /// </summary>
    public class Point
    {
        /// <summary>
        ///     用指定坐标构造一个指定维度的点
        /// </summary>
        /// <param name="dimension">点的维度</param>
        /// <param name="coordinate">点的坐标，长度应为点的维度</param>
        public Point(int dimension, IReadOnlyCollection<float> coordinate)
        {
            Debug.Assert(dimension == coordinate.Count);

            Dimension = dimension;
            Coordinate = coordinate.ToList();
        }

        public int Dimension { get; }
        public List<float> Coordinate { get; }
    }
}