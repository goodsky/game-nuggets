using Campus.GridTerrain;
using Common;
using GameData;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Campus
{
    /// <summary>
    /// Okay, I would call these "Paths" but I already used that word.
    /// Connections between roads, paths and buildings.
    /// </summary>
    public class CampusConnections
    {
        private readonly GridMesh _terrain;
        private readonly CampusManager _campusManager;

        private Dictionary<RoadDestination, IList<RoadConnection>> _roadConnections;
        private Dictionary<PathDestination, IList<PathConnection>> _pathConnections;

        public CampusConnections(CampusData campusData, GameAccessor accessor)
        {
            _terrain = accessor.Terrain;
            _campusManager = accessor.CampusManager;

            _roadConnections = new Dictionary<RoadDestination, IList<RoadConnection>>();
            _pathConnections = new Dictionary<PathDestination, IList<PathConnection>>();
        }

        public IList<RoadConnection> GetConnections(RoadDestination roadDestination)
        {
            if (_roadConnections.TryGetValue(roadDestination, out IList<RoadConnection> connections))
            {
                return connections;
            }

            return new List<RoadConnection>(0);
        }

        public IList<PathConnection> GetConnections(PathDestination pathDestination)
        {
            if (_pathConnections.TryGetValue(pathDestination, out IList<PathConnection> connections))
            {
                return connections;
            }

            return new List<PathConnection>(0);
        }

        public void Recompute()
        {
            var roadConnections = new Dictionary<RoadDestination, IList<RoadConnection>>();
            var pathConnections = new Dictionary<PathDestination, IList<PathConnection>>();

            var roadConnectionsStopwatch = Stopwatch.StartNew();

            IEnumerable<Point2> rSrcs = GetRoadSources();
            foreach (Point2 rSrc in rSrcs)
            {
                IEnumerable<RoadConnection> rConnections = CalculateRoadConnections(rSrc);
                foreach (RoadConnection rConnection in rConnections)
                {
                    IList<RoadConnection> rConnectionList;
                    if (!roadConnections.TryGetValue(rConnection.Destination, out rConnectionList))
                    {
                        rConnectionList = roadConnections[rConnection.Destination] = new List<RoadConnection>();
                    }

                    rConnectionList.Add(rConnection);
                }
            }
            roadConnectionsStopwatch.Stop();
            var pathConnectionsStopwatch = Stopwatch.StartNew();

            pathConnectionsStopwatch.Stop();

            GameLogger.Debug("Recomputed CampusConnections. Road Connections: Count={0} Elapsed={1}ms; Path Connections: Count={2} Elapsed={3}ms;",
                roadConnections.Values.Sum(l => l.Count),
                roadConnectionsStopwatch.ElapsedMilliseconds,
                pathConnections.Values.Sum(l => l.Count),
                pathConnectionsStopwatch.ElapsedMilliseconds);
            
            _roadConnections = roadConnections;
            _pathConnections = pathConnections;
        }

        private IEnumerable<Point2> GetRoadSources()
        {
            var rSrc = new List<Point2>();

            // Road sources are any road vertex that is along the edge of the map.
            for (int x = 0; x <= _terrain.CountX; ++x)
            {
                var p0 = new Point2(x, 0);
                if (_campusManager.GetVertexUse(p0).HasFlag(CampusGridUse.Road))
                {
                    rSrc.Add(p0);
                }

                var pN = new Point2(x, _terrain.CountZ);
                if (_campusManager.GetVertexUse(pN).HasFlag(CampusGridUse.Road))
                {
                    rSrc.Add(pN);
                }
            }

            for (int z = 0; z <= _terrain.CountZ; ++z)
            {
                var p0 = new Point2(0, z);
                if (_campusManager.GetVertexUse(p0).HasFlag(CampusGridUse.Road))
                {
                    rSrc.Add(p0);
                }

                var pN = new Point2(_terrain.CountX, z);
                if (_campusManager.GetVertexUse(pN).HasFlag(CampusGridUse.Road))
                {
                    rSrc.Add(pN);
                }
            }

            return rSrc;
        }

        private IEnumerable<RoadConnection> CalculateRoadConnections(Point2 rSrc)
        {
            var q = new Queue<Point2>();
            var prevMap = new Dictionary<Point2, Point2>();
            var connections = new Dictionary<RoadDestination, RoadConnection>();

            q.Enqueue(rSrc);
            prevMap[rSrc] = Point2.Null;

            while (q.Count > 0)
            {
                Point2 cur = q.Dequeue();

                ParkingInfo parkingInfo;
                // NB: This is using a vertex Point2 to query a grid.
                if (cur.x < _terrain.CountX && cur.z < _terrain.CountZ &&
                    (parkingInfo = _campusManager.GetParkingInfoAtGrid(cur)) != null &&
                    !connections.ContainsKey(parkingInfo))
                {
                    var connection = new List<Point2>();

                    Point2 retrace = cur;
                    do
                    {
                        connection.Add(retrace);
                        retrace = prevMap[retrace];
                    } while (retrace != Point2.Null);

                    connections[parkingInfo] = new RoadConnection(parkingInfo, connection);
                }

                for (int i = 0; i < 4; ++i)
                {
                    int vertX = cur.x + GridConverter.AdjacentVertexDx[i];
                    int vertZ = cur.z + GridConverter.AdjacentVertexDz[i];
                    if (vertX < 0 || vertX > _terrain.CountX || vertZ < 0 || vertZ > _terrain.CountZ)
                        continue;

                    var next = new Point2(vertX, vertZ);
                    if (!prevMap.ContainsKey(next) &&
                        _campusManager.GetVertexUse(next).HasFlag(CampusGridUse.Road))
                    {
                        // Keep a mapping of the previous point to reconstruct path.
                        q.Enqueue(next);
                        prevMap[next] = cur;
                    }
                }
            }

            return connections.Values;
        }
    }

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
