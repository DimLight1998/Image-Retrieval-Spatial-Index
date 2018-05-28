using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpatialIndex.RTree
{
    public class Point
    {
        public Point(int dimension, ICollection<double> coordinate)
        {
            Debug.Assert(dimension == coordinate.Count);

            Dimension = dimension;
            Coordinate = coordinate.ToList();
        }

        public int Dimension { get; }
        public List<double> Coordinate { get; }
    }
}