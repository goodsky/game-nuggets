﻿using Campus.GridTerrain;
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

        /// <summary>
        /// Gets the current list of connections to the desired destination.
        /// </summary>
        /// <param name="roadDestination">The road destination to query for. (e.g. parking lot)</param>
        /// <returns>All valid connections to this destination.</returns>
        public IList<RoadConnection> GetConnections(RoadDestination roadDestination)
        {
            if (_roadConnections.TryGetValue(roadDestination, out IList<RoadConnection> connections))
            {
                return connections;
            }

            return new List<RoadConnection>(0);
        }

        /// <summary>
        /// Gets the current list of connections to the desired destination.
        /// </summary>
        /// <param name="pathDestination">The path destination to query. (e.g. classroom building)</param>
        /// <returns>All valid connections to this destination.</returns>
        public IList<PathConnection> GetConnections(PathDestination pathDestination)
        {
            if (_pathConnections.TryGetValue(pathDestination, out IList<PathConnection> connections))
            {
                return connections;
            }

            return new List<PathConnection>(0);
        }

        /// <summary>
        /// Recompute all the campus connections.
        /// This will calculate the connections from roads to parking lots and parking lots to buildings.
        /// </summary>
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

            var pSrcs = GetPathSources();
            // For paths we are actually searching from destination -> source
            foreach ((PathDestination dst, Point2 pSrc) in pSrcs)
            {
                IEnumerable<PathConnection> pConnections = CalculatePathConnections(dst, pSrc);
                foreach (PathConnection pConnection in pConnections)
                {
                    IList<PathConnection> pConnectionList;
                    if (!pathConnections.TryGetValue(pConnection.Destination, out pConnectionList))
                    {
                        pConnectionList = pathConnections[pConnection.Destination] = new List<PathConnection>();
                    }

                    pConnectionList.Add(pConnection);
                }
            }
            pathConnectionsStopwatch.Stop();

            GameLogger.Debug("Recomputed CampusConnections. Road Connections: Count={0} Elapsed={1}ms; Path Connections: Count={2} Elapsed={3}ms;",
                roadConnections.Values.Sum(l => l.Count),
                roadConnectionsStopwatch.ElapsedMilliseconds,
                pathConnections.Values.Sum(l => l.Count),
                pathConnectionsStopwatch.ElapsedMilliseconds);
            
            _roadConnections = roadConnections;
            _pathConnections = pathConnections;
        }

        /// <summary>
        /// Gets the Road Source nodes on the map.
        /// Road Sources are any road vertex that is on the edge of the map.
        /// </summary>
        private IEnumerable<Point2> GetRoadSources()
        {
            var rSrcs = new List<Point2>();
            for (int x = 0; x <= _terrain.CountX; ++x)
            {
                var p0 = new Point2(x, 0);
                if (_campusManager.GetVertexUse(p0).HasFlag(CampusGridUse.Road))
                {
                    rSrcs.Add(p0);
                }

                var pN = new Point2(x, _terrain.CountZ);
                if (_campusManager.GetVertexUse(pN).HasFlag(CampusGridUse.Road))
                {
                    rSrcs.Add(pN);
                }
            }

            for (int z = 0; z <= _terrain.CountZ; ++z)
            {
                var p0 = new Point2(0, z);
                if (_campusManager.GetVertexUse(p0).HasFlag(CampusGridUse.Road))
                {
                    rSrcs.Add(p0);
                }

                var pN = new Point2(_terrain.CountX, z);
                if (_campusManager.GetVertexUse(pN).HasFlag(CampusGridUse.Road))
                {
                    rSrcs.Add(pN);
                }
            }

            return rSrcs;
        }

        /// <summary>
        /// Gets the Path Source nodes on the map.
        /// Path "Sources" are actually human destinations.
        /// We start searching from the entry points of buildings.
        /// </summary>
        private IEnumerable<(PathDestination destination, Point2 point)> GetPathSources()
        {
            var pSrcs = new List<(PathDestination buildingInfo, Point2 footprint)>();

            foreach (BuildingInfo building in _campusManager.GetBuildingInfo(checkConnections: false))
            {
                foreach (Point2 entry in building.EntryPoints)
                {
                    if (_campusManager.GetGridUse(entry).HasFlag(CampusGridUse.Path))
                    {
                        pSrcs.Add((building, entry));
                    }
                }
            }

            return pSrcs;
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

        private IEnumerable<PathConnection> CalculatePathConnections(PathDestination destinationInfo, Point2 rSrc)
        {
            var q = new Queue<Point2>();
            var prevMap = new Dictionary<Point2, Point2>();
            var connections = new Dictionary<RoadDestination, PathConnection>();

            q.Enqueue(rSrc);
            prevMap[rSrc] = Point2.Null;

            while (q.Count > 0)
            {
                Point2 cur = q.Dequeue();

                ParkingInfo parkingInfo;
                if ((parkingInfo = _campusManager.GetParkingInfoAtGrid(cur)) != null &&
                    !connections.ContainsKey(parkingInfo))
                {
                    var connection = new List<Point2>();

                    Point2 retrace = cur;
                    do
                    {
                        connection.Add(retrace);
                        retrace = prevMap[retrace];
                    } while (retrace != Point2.Null);

                    connections[parkingInfo] = new PathConnection(parkingInfo, destinationInfo, connection);
                }

                for (int i = 0; i < 4; ++i)
                {
                    int vertX = cur.x + GridConverter.AdjacentGridDx[i];
                    int vertZ = cur.z + GridConverter.AdjacentGridDz[i];
                    if (vertX < 0 || vertX >= _terrain.CountX || vertZ < 0 || vertZ >= _terrain.CountZ)
                        continue;

                    var next = new Point2(vertX, vertZ);
                    if (!prevMap.ContainsKey(next) &&
                        _campusManager.GetGridUse(next).HasFlag(CampusGridUse.Path))
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
}
