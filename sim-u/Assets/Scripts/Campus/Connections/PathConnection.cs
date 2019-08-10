using Common;
using System.Collections.Generic;

namespace Campus
{
    public class PathConnection
    {
        public PathConnection(RoadDestination src, PathDestination dst, IList<Point2> connection)
        {
            Source = src;
            Destination = dst;
            VertexConnection = connection;
        }

        public RoadDestination Source { get; }

        public PathDestination Destination { get; }

        public IList<Point2> VertexConnection { get; }
    }
}
