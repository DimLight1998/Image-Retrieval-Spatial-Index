using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SpatialIndex.RTree
{
    /// <summary>
    ///     高维的一个矩形，单浮点精度
    /// </summary>
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public class Rectangle
    {
        /// <summary>
        ///     用指定的两个对角点构造出一个矩形
        /// </summary>
        /// <param name="dimension">矩形的维度</param>
        /// <param name="point1">矩形的任一个顶点的坐标，应具有相同维度</param>
        /// <param name="point2"><c>point1</c> 的对角顶点的坐标，应具有相同维度</param>
        public Rectangle(int dimension, Point point1, Point point2)
        {
            Debug.Assert(dimension == point1.Dimension && dimension == point2.Dimension);

            Dimension = dimension;
            for (var i = 0; i < dimension; i++)
            {
                MaxBoundries.Add(Math.Max(point1.Coordinate[i], point2.Coordinate[i]));
                MinBoundries.Add(Math.Min(point1.Coordinate[i], point2.Coordinate[i]));
            }
        }

        public int Dimension { get; }
        public List<float> MaxBoundries { get; } = new List<float>();
        public List<float> MinBoundries { get; } = new List<float>();

        /// <summary>
        ///     与给定矩形在任意维度上某一条边“对齐”时返回 true
        /// </summary>
        public bool OverlapsWith(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            for (var i = 0; i < Dimension; i++)
                if (MaxBoundries[i] == rectangle.MaxBoundries[i] || MinBoundries[i] == rectangle.MinBoundries[i])
                    return true;
            return false;
        }

        /// <summary>
        ///     与给定矩形的存在非空交集时即返回 true（不要求交集体积大于 0）
        /// </summary>
        public bool IntersectsWith(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            for (var i = 0; i < Dimension; i++)
                if (MaxBoundries[i] < rectangle.MinBoundries[i] || MinBoundries[i] > rectangle.MaxBoundries[i])
                    return false;
            return true;
        }

        /// <summary>
        ///     包含给定矩形时返回 true（不要求严格包含，边重合也算）
        /// </summary>
        public bool Contains(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            for (var i = 0; i < Dimension; i++)
                if (MaxBoundries[i] < rectangle.MaxBoundries[i] || MinBoundries[i] > rectangle.MinBoundries[i])
                    return false;
            return true;
        }

        /// <summary>
        ///     被给定矩形包含时返回 true（不要求严格包含，边重合也算）
        /// </summary>
        public bool IsContainedBy(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            for (var i = 0; i < Dimension; i++)
                if (MinBoundries[i] < rectangle.MinBoundries[i] || MaxBoundries[i] > rectangle.MaxBoundries[i])
                    return false;
            return true;
        }

        /// <summary>
        ///     计算矩形内部任一点到给定点的最小距离
        /// </summary>
        public float MinimalDistanceTo(Point point)
        {
            Debug.Assert(point.Dimension == Dimension);

            var distanceSquared = 0f;
            for (var i = 0; i < Dimension; i++)
                if (point.Coordinate[i] < MinBoundries[i])
                    distanceSquared += (float) Math.Pow(MinBoundries[i] - point.Coordinate[i], 2);
                else if (point.Coordinate[i] > MaxBoundries[i])
                    distanceSquared += (float) Math.Pow(point.Coordinate[i] - MaxBoundries[i], 2);

            return (float) Math.Sqrt(distanceSquared);
        }

        /// <summary>
        ///     计算矩形内部任一点到给定矩形内部任一点的最小距离
        /// </summary>
        public float MinimalDistanceTo(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            var distanceSquared = 0f;

            for (var i = 0; i < Dimension; i++)
                if (MinBoundries[i] > rectangle.MaxBoundries[i])
                    distanceSquared += (float) Math.Pow(MinBoundries[i] - rectangle.MaxBoundries[i], 2);
                else if (MaxBoundries[i] < rectangle.MinBoundries[i])
                    distanceSquared += (float) Math.Pow(rectangle.MinBoundries[i] - MaxBoundries[i], 2);

            return (float) Math.Sqrt(distanceSquared);
        }

        /// <summary>
        ///     计算矩形内部任一点到给定点的最大距离
        /// </summary>
        public float MaximalDistanceTo(Point point)
        {
            Debug.Assert(point.Dimension == Dimension);

            var distanceSquared = 0f;
            for (var i = 0; i < Dimension; i++)
            {
                var diff1 = Math.Abs(point.Coordinate[i] - MaxBoundries[i]);
                var diff2 = Math.Abs(point.Coordinate[i] - MinBoundries[i]);
                distanceSquared += (float) Math.Pow(Math.Max(diff1, diff2), 2);
            }

            return (float) Math.Sqrt(distanceSquared);
        }

        /// <summary>
        ///     计算矩形内部任一点到给定矩形内部任一点的最大距离
        /// </summary>
        public float MaximalDistanceTo(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            var distanceSquared = 0f;

            for (var i = 0; i < Dimension; i++)
            {
                var diff1 = Math.Abs(rectangle.MaxBoundries[i] - MinBoundries[i]);
                var diff2 = Math.Abs(rectangle.MinBoundries[i] - MaxBoundries[i]);
                distanceSquared += (float) Math.Pow(Math.Max(diff1, diff2), 2);
            }

            return (float) Math.Sqrt(distanceSquared);
        }

        /// <summary>
        ///     计算矩形体积
        /// </summary>
        public float GetArea()
        {
            var area = 1.0f;
            for (var i = 0; i < Dimension; i++)
                area *= MaxBoundries[i] - MinBoundries[i];
            return area;
        }

        /// <summary>
        ///     计算与这个矩形合并后面积的增量（矩形不作变化）
        /// </summary>
        public float GetEnlargement(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            return Union(rectangle).GetArea() - GetArea();
        }

        /// <summary>
        ///     将当前矩形与给定矩形合并
        /// </summary>
        public void AddRectangle(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            for (var i = 0; i < Dimension; i++)
            {
                MinBoundries[i] = Math.Min(MinBoundries[i], rectangle.MinBoundries[i]);
                MaxBoundries[i] = Math.Max(MaxBoundries[i], rectangle.MaxBoundries[i]);
            }
        }

        /// <summary>
        ///     获得当前矩形与给定矩形合并后的矩形（矩形不作变化）
        /// </summary>
        public Rectangle Union(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            var ret = Copy();
            ret.AddRectangle(rectangle);
            return ret;
        }

        /// <summary>
        ///     复制当前矩形
        /// </summary>
        public Rectangle Copy()
        {
            var point1 = new Point(Dimension, MaxBoundries);
            var point2 = new Point(Dimension, MinBoundries);
            return new Rectangle(Dimension, point1, point2);
        }

        /// <summary>
        ///     判断两个矩形是否在几何上相等
        /// </summary>
        public bool ContentEquals(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            for (var i = 0; i < Dimension; i++)
                if (rectangle.MaxBoundries[i] != MaxBoundries[i] || rectangle.MinBoundries[i] != MinBoundries[i])
                    return false;
            return true;
        }

        /// <summary>
        ///     判断两个矩形是否指向同一个对象
        /// </summary>
        public bool ReferenceEquals(Rectangle rectangle)
        {
            return Equals(rectangle);
        }
    }
}