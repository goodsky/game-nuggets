using System;

namespace Common
{
    public struct Rectangle
    {
        public Point2 Start { get; private set; }
        public Point2 End { get; private set; }

        public int MinX { get { return Math.Min(Start.x, End.x); } }
        public int MaxX { get { return Math.Max(Start.x, End.x); } }
        public int MinZ { get { return Math.Min(Start.z, End.z); } }
        public int MaxZ { get { return Math.Max(Start.z, End.z); } }

        public int SizeX { get { return MaxX - MinX + 1; } }
        public int SizeZ { get { return MaxZ - MinZ + 1; } }

        public Rectangle(Point2 start)
        {
            Start = End = start;
        }

        public Rectangle(Point2 start, Point2 end)
            : this(start)
        {
            UpdateEndPoint(end);
        }

        public Rectangle(Point3 start)
            : this(new Point2(start.x, start.z))
        {
        }

        public Rectangle(Point3 start, Point3 end)
            : this(start)
        {
            UpdateEndPoint(end);
        }

        public void UpdateEndPoint(Point2 newEnd)
        {
            End = newEnd;
        }

        public void UpdateEndPoint(Point3 newEnd)
        {
            UpdateEndPoint(new Point2(newEnd.x, newEnd.z));
        }

        public bool IsPointInRectangle(Point2 point)
        {
            return
                MinX <= point.x && point.x <= MaxX &&
                MinZ <= point.z && point.z <= MaxZ;
        }
    }
}
