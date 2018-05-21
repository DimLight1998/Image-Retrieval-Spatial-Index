using System;
using System.Collections.Generic;

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
        /// <exception cref="NotImplementedException"></exception>
        public Point(int dimension, List<int> coordinate)
        {
            throw new NotImplementedException();
        }
    }
}