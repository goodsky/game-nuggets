using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TugOfWar.GameObject.Derps;

namespace TugOfWar.GameObject
{
    public class Geometry
    {
        // Make Cardinal Direction Unit Vectors
        public static myVector[] cardinalUnitVectors = {    new myVector(1.0, 0.0), new myVector(1 / Math.Sqrt(2), 1 / Math.Sqrt(2)),
                                                            new myVector(0.0, 1.0), new myVector(-1 / Math.Sqrt(2), 1 / Math.Sqrt(2)), 
                                                            new myVector(-1.0, 0.0), new myVector(-1 / Math.Sqrt(2), -1 / Math.Sqrt(2)),
                                                            new myVector(0.0, -1.0), new myVector(1 / Math.Sqrt(2), -1 / Math.Sqrt(2)) };

        // Find the nearest cardinal direction to a vector (for snapping movement and sprites to 8 frames)
        public static int GetNearestCardinalDir(myVector v)
        {
            int ret = 0;
            for (int i = 1; i < 8; ++i)
            {
                if (v.dot(Geometry.cardinalUnitVectors[i]) > v.dot(Geometry.cardinalUnitVectors[ret]))
                    ret = i;
            }

            return ret;
        }

        //////////////////////////////
        // Inter-Derp Circle Casting - for team derp collisions
        // Returns: double which is the t parameter value until a collision (1.0 means no collision)
        //////////////////////////////
        // The dist threshhold is around root2 * BLOCK_WIDTH distance from the derp (so it will only check very close derps)
        // NOTICE: this could probably be smaller in the future, if using speed I calculate the mathimatically smallest radius I need to check
        private static double distSquaredThreshholdX = 2 * Field.BLOCK_WIDTH * Field.BLOCK_WIDTH;
        private static double bufferDistance = 1.0;
        static public double DerpCircleCast(Derp d1, Derp d2, myVector v)
        {
            double distX = d2.x - d1.x;
            double distY = d2.y - d1.y;

            // make sure we are close before we do any of the computational checking
            if (distX * distX + distY * distY > distSquaredThreshholdX)
                return 1.0;

            // Intersect distance
            double radius = d1.stats.radius + d2.stats.radius;

            // solve the quadratic to find the intersection of two circles
            double A = v.x * v.x + v.y * v.y;
            double B = 2 * (d1.x * v.x - d2.x * v.x + d1.y * v.y - d2.y * v.y);
            double C = (d1.x * d1.x) + (d2.x * d2.x) - 2 * d2.x * d1.x + (d1.y * d1.y) + (d2.y * d2.y) - 2 * d2.y * d1.y - (radius * radius);

            // unreal answers or a t that is greater than 1.0 means no collision
            double discr = (B * B) - 4 * A * C;
            if (discr < 0.0)
                return 1.0; //imaginary answer

            double t = (-B - Math.Sqrt(discr)) / (2 * A);
            if (t > 1.0)
                return 1.0; // no collision with this step size

            // this is detecting a collision behind us
            // NOTICE: there may be issues here in the future... if someone moves into us? (wait... they can't though. we guarentee that no one will move somewhere they can't... I think...)
            if (t < 0.0)
                return 1.0;

            // BUFFER
            // if we let them get right up next to each other, sometimes the floating point error is enough to let them slip through (I think)
            // add a buffer if there is a collision
            if (t < 1.0 - 1e-6)
            {
                t -= bufferDistance / Math.Sqrt(v.x * v.x + v.y * v.y);
                t = Math.Max(t, 0.0);
            }

            return t;
        }

        // The above situation in a more general form
        static public double MovingCirlcePointCast(myLineSegment move, double radius, myPoint point)
        {
            double distX = move.p1.x - point.x;
            double distY = move.p1.y - point.y;

            // make sure we are close before we do any of the computational checking
            if (distX * distX + distY * distY > distSquaredThreshholdX)
                return 1.0;

            // solve the quadratic to find the intersection of two circles
            myPoint p1 = move.p1;
            myPoint p2 = point;
            myVector v = move.v;

            double A = v.x * v.x + v.y * v.y;
            double B = 2 * (p1.x * v.x - p2.x * v.x + p1.y * v.y - p2.y * v.y);
            double C = (p1.x * p1.x) + (p2.x * p2.x) - 2 * p2.x * p1.x + (p1.y * p1.y) + (p2.y * p2.y) - 2 * p2.y * p1.y - (radius * radius);

            // unreal answers or a t that is greater than 1.0 means no collision
            double discr = (B * B) - 4 * A * C;
            if (discr < 0.0)
                return 1.0; //imaginary answer

            double t = (-B - Math.Sqrt(discr)) / (2 * A);
            if (t > 1.0)
                return 1.0; // no collision with this step size

            // this is detecting a collision behind us
            // NOTICE: there may be issues here in the future... if someone moves into us? (wait... they can't though. we guarentee that no one will move somewhere they can't... I think...)
            if (t < 0.0)
                return 1.0;

            // BUFFER
            // if we let them get right up next to each other, sometimes the floating point error is enough to let them slip through (I think)
            // add a buffer if there is a collision
            if (t < 1.0 - 1e-6)
            {
                t -= bufferDistance / Math.Sqrt(v.x * v.x + v.y * v.y);
                t = Math.Max(t, 0.0);
            }

            return t;
        }

        ////////////////////////////////////
        // Derp-LineSegment Circle Casting - for field tile collisions
        // Returns: true if collision, false otherwise
        // out double t: this will be the parameter t, how far we can move along our vector before we get within Derp's radius of the line segment
        ////////////////////////////////////
        public static bool DerpLineSegmentCast(Derp d, myVector v, myLineSegment wall, out double t)
        {
            myLineSegment derpMoveLine = new myLineSegment(d.x, d.y, v);

            t = derpMoveLine.SegmentIntersectionWithRadius(wall, d.stats.radius);

            return (t < 1.0 - 1e-6);
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // Point Class with doubles
    public class myPoint
    {
        public double x, y;

        public myPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double distance2(myPoint o)
        {
            return (o.x - x) * (o.x - x) + (o.y - y) * (o.y - y);
        }

        public double distance(myPoint o)
        {
            return Math.Sqrt(distance2(o));
        }
    }

    // Vector Class with doubles instead of floats
    public class myVector
    {
        public double x, y;

        public myVector(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public myVector(myPoint p1, myPoint p2)
        {
            x = p2.x - p1.x;
            y = p2.y - p1.y;
        }

        public double dot(myVector o)
        {
            return x * o.x + y * o.y;
        }

        public double cross(myVector o)
        {
            return x * o.y - y * o.x;
        }

        public double mag2()
        {
            return x * x + y * y;
        }

        public double mag()
        {
            return Math.Sqrt(x * x + y * y);
        }

        // convert this vector to a unit vector
        public void toUnit()
        {
            double myMag = mag();
            x /= myMag;
            y /= myMag;
        }
    }

    // LineSegment Class
    public class myLineSegment
    {
        public myPoint p1, p2;
        public myVector v;

        public myLineSegment(double x1, double y1, double x2, double y2)
        {
            p1 = new myPoint(x1, y1);
            p2 = new myPoint(x2, y2);

            v = new myVector(x2 - x1, y2 - y1);
        }

        public myLineSegment(double x, double y, myVector v)
        {
            p1 = new myPoint(x, y);
            p2 = new myPoint(x + v.x, y + v.y);

            this.v = v;
        }

        public myLineSegment(myPoint p1, myPoint p2)
        {
            this.p1 = p1;
            this.p2 = p2;

            if (p1 != null && p2 != null)
                v = new myVector(p2.x - p1.x, p2.y - p1.y);
        }

        // Update this segment based on new information
        public void Update(myPoint p1, myPoint p2)
        {
            this.p1 = p1;
            this.p2 = p2;

            v = new myVector(p2.x - p1.x, p2.y - p1.y);
        }

        // return the squared distanc to the point (remove squareroots)
        public double DistToPoint2(myPoint p)
        {
            // my length squared
            double l2 = (p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y);

            // find the parameterized value of where the nearest point would be
            // (dot product of my vector to the vector from my start to the point)
            myVector toPoint = new myVector(p1, p);
            double t = v.dot(toPoint) / l2;

            if (t < 0.0) return p1.distance2(p);
            if (t > 1.0) return p2.distance2(p);

            // it's on the line segment, project to the position
            myPoint projPoint = new myPoint(p1.x + t * v.x, p1.y + t * v.y);
            return projPoint.distance2(p);
        }

        public double DistToPoint(myPoint p)
        {
            return Math.Sqrt(DistToPoint2(p));
        }

        // Check for an intersection with the other line segment, assuming that my line segement has a 'radius'
        // return the parameter value of where the farthest we can move along this line segment before a collision
        private double CollisionBuffer = 2.0;
        public double SegmentIntersectionWithRadius(myLineSegment o, double radius)
        {
            // get determinate and check for parallel lines
            double D = v.cross(o.v);
            if (Math.Abs(D) < 1e-6)
            {
                // Are we already within the line radius?
                if (o.DistToPoint2(p1) < radius * radius)
                {
                    return 0.0;
                }
                // Check if we are now moving onto the line segement.
                else if (NonIntersectingSegmentDistance(o, 0.0, radius) < 0.1)
                {
                    // return 1.0;
                    return Math.Min(Geometry.MovingCirlcePointCast(this, radius, o.p1), Geometry.MovingCirlcePointCast(this, radius, o.p2));
                }
                // No collision
                else
                {
                    return 1.0;
                }
            }

            // relative vector from the other segments starting point to my own
            myVector relVect = new myVector(p1, o.p1);
            double t = relVect.cross(o.v) / D;
            double u = relVect.cross(v) / D;

            // find if the intersection happens along the other line segment
            if (u < 0.0 || u > 1.0)
                return NonIntersectingSegmentDistance(o, t, radius);

            // find if the intersection happens along this line segment
            if (t < 0.0 || t > 1.0)
                return NonIntersectingSegmentDistance(o, t, radius);

            // An intersection is happening! Scoot the t parameter back a bit for the radius and buffer then return it
            // Scoot t back so the radius is equal to our radius
            double sintheta = Math.Abs(o.v.cross(v)) / (v.mag() * o.v.mag());
            t -=  (radius / sintheta) / v.mag();

            // also remove the collision buffer, then return it
            t -= (CollisionBuffer / v.mag());
            return (t < 0.0 ? 0.0 : t);
        }

        // used for the segment intersection with radius code,
        // if there is not a collision, find the nearest endpoint via point-line distance
        // then don't forget that we move back the parameter t by enough to add a buffer from the collision
        private double NonIntersectingSegmentDistance(myLineSegment o, double t, double radius)
        {
            double mindist2 = double.MaxValue;

            mindist2 = Math.Min(mindist2, o.DistToPoint2(p1));
            mindist2 = Math.Min(mindist2, o.DistToPoint2(p2));
            mindist2 = Math.Min(mindist2, DistToPoint2(o.p1));
            mindist2 = Math.Min(mindist2, DistToPoint2(o.p2));

            if (mindist2 < radius * radius)
            {
                // there is a collision without intersection, move up as far as we can without collision
                // note: we use the t term that we passed in, and subtract enough so that the perpendicular distance equals radius
                
                // also note: if t == 0 that means that the collision is already happening without a move... we're stuck?
                //if (t < 1e-6)
                //    return 0.0;

                // Scoot t back so the radius is equal to our radius
                double sintheta = Math.Abs(o.v.cross(v)) / (v.mag() * o.v.mag());
                t -= (radius / sintheta) / v.mag();

                // also remove the collision buffer, then return it
                t -= (CollisionBuffer / v.mag());
                return (t < 0.0 ? 0.0 : t);
            }

            // if there is still no collision, just return the full 1.0 parameter value
            return 1.0;
        }
    }

    //////////////////////////////////////////////////////////////////////////////
    // Simple Utility classes
    class SimpleNode
    {
        public int x, y, step;

        public SimpleNode(int x, int y, int step)
        {
            this.x = x;
            this.y = y;
            this.step = step;
        }
    }
}
