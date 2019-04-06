using System;
using UnityEngine.Assertions;

namespace GridTerrain
{
    /// <summary>
    /// 2D Point that has a safe HashCode.
    /// Valid for values up to 2^10.
    /// </summary>
    public struct Point2 : IEquatable<Point2>
    {
        private readonly int _x, _y;

        public int x { get { return _x; } }
        public int y { get { return _y; } }

        public Point2(int x, int y)
        {
            this._x = x;
            this._y = y;

            Assert.IsTrue(x >= 0 && x < (1 << 10) && y >= 0 && y < (1 << 10), string.Format("Point out of range ({0}, {1})", x, y));
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", _x, _y);
        }

        public override int GetHashCode()
        {
            // works for values up to 2^10
            return _x + (_y << 10);
        }

        public override bool Equals(object obj)
        {
            return obj is Point2 && Equals((Point2)obj);
        }

        public bool Equals(Point2 p)
        {
            return this == p;
        }

        public static bool operator ==(Point2 p1, Point2 p2)
        {
            return p1._x == p2._x && p1._y == p2._y;
        }
        public static bool operator !=(Point2 x, Point2 y)
        {
            return !(x == y);
        }
    }

    /// <summary>
    /// 3D Point that has a safe HashCode.
    /// Valid for values up to 2^10.
    /// </summary>
    public struct Point3 : IEquatable<Point3>
    {
        public static Point3 Null = new Point3(null);

        private readonly int _x, _y, _z;

        public int x { get { return _x; } }
        public int y { get { return _y; } }
        public int z { get { return _z; } }

        public Point3(int x, int y, int z)
        {
            this._x = x;
            this._y = y;
            this._z = z;

            Assert.IsTrue(x >= 0 && x < (1 << 10) && y >= 0 && y < (1 << 10) && z >= 0 && z < (1 << 10), string.Format("Point out of range ({0}, {1}, {2})", x, y, z));
        }

        private Point3(object _)
        {
            _x = -1; _y = -1; _z = -1;
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2})", _x, _y, _z);
        }

        public override int GetHashCode()
        {
            // works for values up to 2^10
            return _x + (_y << 10) + (_z << 20);
        }

        public override bool Equals(object obj)
        {
            return obj is Point3 && Equals((Point3)obj);
        }

        public bool Equals(Point3 p)
        {
            return this == p;
        }

        public static bool operator ==(Point3 p1, Point3 p2)
        {
            return p1._x == p2._x && p1._y == p2._y && p1._z == p2._z;
        }
        public static bool operator !=(Point3 x, Point3 y)
        {
            return !(x == y);
        }
    }
}
