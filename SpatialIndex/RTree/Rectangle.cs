using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SpatialIndex.RTree
{
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public class Rectangle
    {
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

        public Rectangle(int dimension, List<float> minBoundries, List<float> maxBoundries)
        {
            Debug.Assert(dimension == minBoundries.Count && dimension == maxBoundries.Count);

            Dimension = dimension;
            MaxBoundries = maxBoundries.ToList();
            MinBoundries = minBoundries.ToList();
        }

        public int Dimension { get; }
        public List<float> MaxBoundries { get; set; } = new List<float>();
        public List<float> MinBoundries { get; set; } = new List<float>();

        public bool OverlapsWith(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            for (var i = 0; i < Dimension; i++)
                if (MaxBoundries[i] == rectangle.MaxBoundries[i] || MinBoundries[i] == rectangle.MinBoundries[i])
                    return true;
            return false;
        }

        public bool IntersectsWith(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            for (var i = 0; i < Dimension; i++)
                if (MaxBoundries[i] < rectangle.MinBoundries[i] || MinBoundries[i] > rectangle.MaxBoundries[i])
                    return false;
            return true;
        }

        public bool Contains(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            for (var i = 0; i < Dimension; i++)
                if (MaxBoundries[i] < rectangle.MaxBoundries[i] || MinBoundries[i] > rectangle.MinBoundries[i])
                    return false;
            return true;
        }

        public bool IsContainedBy(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            for (var i = 0; i < Dimension; i++)
                if (MinBoundries[i] < rectangle.MinBoundries[i] || MaxBoundries[i] > rectangle.MaxBoundries[i])
                    return false;
            return true;
        }

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

        public float GetArea()
        {
            var area = 1.0f;
            for (var i = 0; i < Dimension; i++)
                area *= MaxBoundries[i] - MinBoundries[i];
            return area;
        }

        public float GetEnlargement(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            return Union(rectangle).GetArea() - GetArea();
        }

        public void AddRectangle(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            for (var i = 0; i < Dimension; i++)
            {
                MinBoundries[i] = Math.Min(MinBoundries[i], rectangle.MinBoundries[i]);
                MaxBoundries[i] = Math.Max(MaxBoundries[i], rectangle.MaxBoundries[i]);
            }
        }

        public Rectangle Union(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            var ret = Copy();
            ret.AddRectangle(rectangle);
            return ret;
        }

        public Rectangle Copy()
        {
            var point1 = new Point(Dimension, MaxBoundries);
            var point2 = new Point(Dimension, MinBoundries);
            return new Rectangle(Dimension, point1, point2);
        }

        public bool ContentEquals(Rectangle rectangle)
        {
            Debug.Assert(rectangle.Dimension == Dimension);

            for (var i = 0; i < Dimension; i++)
                if (rectangle.MaxBoundries[i] != MaxBoundries[i] || rectangle.MinBoundries[i] != MinBoundries[i])
                    return false;
            return true;
        }

        public bool ReferenceEquals(Rectangle rectangle)
        {
            return Equals(rectangle);
        }
    }
}