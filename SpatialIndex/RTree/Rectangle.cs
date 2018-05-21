using System;
using System.Collections.Generic;

namespace SpatialIndex.RTree
{
    /// <summary>
    /// 高维的一个矩形，单浮点精度
    /// </summary>
    public class Rectangle
    {
        /// <summary>
        /// 用指定的两个对角点构造出一个矩形
        /// </summary>
        /// <param name="dimension">矩形的维度</param>
        /// <param name="point1">矩形的任一个顶点的坐标，应具有相同维度</param>
        /// <param name="point2"><c>point1</c> 的对角顶点的坐标，应具有相同维度</param>
        /// <exception cref="NotImplementedException"></exception>
        public Rectangle(int dimension, Point point1, Point point2)
        {
            throw new NotImplementedException();
        }
    }
}