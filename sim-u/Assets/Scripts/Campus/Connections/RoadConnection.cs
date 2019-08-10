using Common;
using System.Collections.Generic;

namespace Campus
{
    public class RoadConnection
    {
        public RoadConnection(RoadDestination dst, IList<Point2> connection)
        {
            Destination = dst;
            VertexConnection = connection;
        }

        public RoadDestination Destination { get; }

        public IList<Point2> VertexConnection { get; }
    }
}
