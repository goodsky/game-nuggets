using System;

public struct Point : IEquatable<Point>
{
    private readonly int x, y;

    public int X { get { return x; } }
    public int Y { get { return y; } }

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    // works for values up to 2^15
    public override int GetHashCode()
    {
        return (x << 15) + y;
    }

    public override bool Equals(object obj)
    {
        return obj is Point && Equals((Point)obj);
    }

    public bool Equals(Point p)
    {
        return x == p.x && y == p.y;
    }
}
