using Campus.GridTerrain;
using Common;
using GameData;
using System.Collections.Generic;
using System.Linq;

namespace Campus
{
    /// <summary>
    /// Collection of all parking lots on campus.
    /// </summary>
    public class CampusParkingLots
    {
        private readonly CampusManager _campusManager;
        private readonly GridMesh _terrain;
        private readonly ParkingLot[,] _lotAtGridPosition;

        private readonly int _startIndex;
        private readonly int _invalidIndex;
        private readonly int _emptyIndex;

        public CampusParkingLots(CampusData campusData, GameAccessor accessor)
        {
            _campusManager = accessor.CampusManager;
            _terrain = accessor.Terrain;
            _lotAtGridPosition = new ParkingLot[_terrain.CountX, _terrain.CountZ];

            _startIndex = campusData.Terrain.SubmaterialParkingLotsIndex;
            _invalidIndex = campusData.Terrain.SubmaterialInvalidIndex;
            _emptyIndex = campusData.Terrain.SubmaterialEmptyGrassIndex;
        }

        /// <summary>
        /// Gets the internal save state for campus parking lots.
        /// </summary>
        public ParkingLotSaveState[] SaveGameState()
        {
            return Utils.GetDistinct(_lotAtGridPosition)
                .Select(lot =>
                {
                    return new ParkingLotSaveState
                    {
                        StartX = lot.Footprint.Start.x,
                        StartZ = lot.Footprint.Start.z,
                        EndX = lot.Footprint.End.x,
                        EndZ = lot.Footprint.End.z,
                    };
                })
                .ToArray();
        }

        /// <summary>
        /// Load the save game state.
        /// </summary>
        public void LoadGameState(ParkingLotSaveState[] parkingLotState)
        {
            if (parkingLotState != null)
            {
                foreach (ParkingLotSaveState savedParkingLot in parkingLotState)
                {
                    Rectangle footprint = new Rectangle(
                        new Point2(savedParkingLot.StartX, savedParkingLot.StartZ),
                        new Point2(savedParkingLot.EndX, savedParkingLot.EndZ));

                    _campusManager.ConstructParkingLot(footprint);
                }
            }
        }

        /// <summary>
        /// Checks if a parking lot exists at a given grid point.
        /// </summary>
        /// <param name="pos">Grid position to query.</param>
        /// <returns>True if a lot exists at position, false otherwise.</returns>
        public bool IsParkingLotAtPosition(Point2 pos)
        {
            return _lotAtGridPosition[pos.x, pos.z] != null;
        }

        /// <summary>
        /// Checks if a point is on the edge of a parking lot.
        /// The edge of parking lots are a magical special place.
        /// Roads and paths can combine here. That's why this check exists.
        /// </summary>
        /// <param name="pos">Grid position to query.</param>
        /// <returns>True if a lot edge exists at position, false otherwise.</returns>
        public bool IsOnEdgeOfParkingLot(Point2 pos, bool disallowCorners = false)
        {
            ParkingLot lot = _lotAtGridPosition[pos.x, pos.z];
            if (lot != null)
            {
                Rectangle rect = lot.Footprint;

                // Roads aren't allowed to build on corners. Therefore this.
                if (disallowCorners &&
                    (pos.x == rect.MinX || pos.x == rect.MaxX) &&
                    (pos.z == rect.MinZ || pos.z == rect.MaxZ))
                    return false;

                return
                    (pos.x == rect.MinX && rect.MinZ <= pos.z && pos.z <= rect.MaxZ) ||
                    (pos.x == rect.MaxX && rect.MinZ <= pos.z && pos.z <= rect.MaxZ) ||
                    (pos.z == rect.MinZ && rect.MinX <= pos.x && pos.x <= rect.MaxX) ||
                    (pos.z == rect.MaxZ && rect.MinX <= pos.x && pos.x <= rect.MaxX);
            }

            return false;
        }

        /// <summary>
        /// Build a parking lot in the rectangle.
        /// </summary>
        /// <param name="rectangle">The parking lot footprint.</param>
        /// <returns>The points on the terrain that have been modified.</returns>
        public IEnumerable<Point2> ConstructParkingLot(Rectangle rectangle)
        {
            var parkingLot = new ParkingLot(rectangle);

            for (int x = rectangle.MinX; x <= rectangle.MaxX; ++x)
            {
                for (int z = rectangle.MinZ; z <= rectangle.MaxZ; ++z)
                {
                    _lotAtGridPosition[x, z] = parkingLot;
                }
            }

            // Register the sidewalk around the edge
            Point2 p1 = new Point2(rectangle.MinX, rectangle.MinZ);
            Point2 p2 = new Point2(rectangle.MinX, rectangle.MaxZ);
            Point2 p3 = new Point2(rectangle.MaxX, rectangle.MaxZ);
            Point2 p4 = new Point2(rectangle.MaxX, rectangle.MinZ);
            _campusManager.ConstructPath(new AxisAlignedLine(p1, p2));
            _campusManager.ConstructPath(new AxisAlignedLine(p2, p3));
            _campusManager.ConstructPath(new AxisAlignedLine(p3, p4));
            _campusManager.ConstructPath(new AxisAlignedLine(p4, p1));

            // Register the road inside the lot
            for (int x = rectangle.MinX + 1; x <= rectangle.MaxX; ++x)
            {
                // NB: Roads are built on vertices. Only fill the inner vertices.
                Point2 vp1 = new Point2(x, rectangle.MinZ + 1);
                Point2 vp2 = new Point2(x, rectangle.MaxZ);
                _campusManager.ConstructRoad(new AxisAlignedLine(vp1, vp2));
            }

            // Return all the potentially modified grids around the parking lot for updating.
            for (int scanX = rectangle.MinX - 1; scanX <= rectangle.MaxX + 1; ++scanX)
                for (int scanZ = rectangle.MinZ - 1; scanZ <= rectangle.MaxZ + 1; ++scanZ)
                    if (_terrain.GridInBounds(scanX, scanZ))
                        yield return new Point2(scanX, scanZ);
        }

        /// <summary>
        /// Remove a parking lot that exists at the requested position.
        /// </summary>
        /// <param name="pos">The position to remove the parking lot at.</param>
        /// <returns>The points on the terrain that have been modified.</returns>
        public IEnumerable<Point2> DestroyParkingLotAt(Point2 pos)
        {
            ParkingLot parkingLot = _lotAtGridPosition[pos.x, pos.z];
            if (parkingLot == null)
            {
                yield break;
            }

            for (int x = parkingLot.Footprint.MinX; x <= parkingLot.Footprint.MaxX; ++x)
            {
                for (int z = parkingLot.Footprint.MinZ; z <= parkingLot.Footprint.MaxZ; ++z)
                {
                    _lotAtGridPosition[x, z] = null;

                    // Clean up the path and the road while we are at it
                    _campusManager.DestroyAt(new Point2(x, z), CampusGridUse.Path);
                    _campusManager.DestroyAt(new Point2(x, z), CampusGridUse.Road);
                }
            }

            // Return all the potentially modified grids around the parking lot for updating.
            for (int scanX = parkingLot.Footprint.MinX - 1; scanX <= parkingLot.Footprint.MaxX + 1; ++scanX)
                for (int scanZ = parkingLot.Footprint.MinZ - 1; scanZ <= parkingLot.Footprint.MaxZ + 1; ++scanZ)
                    if (_terrain.GridInBounds(scanX, scanZ))
                        yield return new Point2(scanX, scanZ);
        }

        /// <summary>
        /// Update the material of the grid to look like a parking lot.
        /// </summary>
        public (int submaterialIndex, SubmaterialRotation rotation, SubmaterialInversion inversion) GetParkingLotMaterial(Point2 pos)
        {
            ParkingLot parkingLot = _lotAtGridPosition[pos.x, pos.z];

            if (parkingLot == null)
            {
                return (_emptyIndex, SubmaterialRotation.deg0, SubmaterialInversion.None);
            }

            // check 4 adjacent grids for parking lots
            // check 4 adjacent grids for parking spots (for drawing lines)
            // check 4 adjacent grids for paths
            // check 4 adjacent vertices for roads
            bool[] adjLots = new bool[4];
            bool[] adjSpot = new bool[5];
            bool[] adjPath = new bool[4];
            bool[] adjRoad = new bool[4];
            for (int i = 0; i < 4; ++i)
            {
                int gridX = pos.x + GridConverter.AdjacentGridDx[i];
                int gridZ = pos.z + GridConverter.AdjacentGridDz[i];

                int spotX = gridX - parkingLot.Footprint.MinX;
                int spotZ = gridZ - parkingLot.Footprint.MinZ;

                int vertX = pos.x + GridConverter.GridToVertexDx[i];
                int vertZ = pos.z + GridConverter.GridToVertexDz[i];

                adjLots[i] = _terrain.GridInBounds(gridX, gridZ) && parkingLot.Footprint.IsPointInRectangle(new Point2(gridX, gridZ));
                adjSpot[i] = _terrain.GridInBounds(gridX, gridZ) && parkingLot.Footprint.IsPointInRectangle(new Point2(gridX, gridZ)) && parkingLot.LotLines[spotX, spotZ];
                adjPath[i] = _terrain.GridInBounds(gridX, gridZ) && (_campusManager.GetGridUse(new Point2(gridX, gridZ)) & CampusGridUse.Path) == CampusGridUse.Path;
                adjRoad[i] = _terrain.VertexInBounds(vertX, vertZ) && (_campusManager.GetVertexUse(new Point2(vertX, vertZ)) & CampusGridUse.Road) == CampusGridUse.Road;
            }

            // spots also need to check if they themselves are a parking spot
            adjSpot[4] = parkingLot.LotLines[pos.x - parkingLot.Footprint.MinX, pos.z - parkingLot.Footprint.MinZ];

            // NB: The logic for parking lots is more complex than other managers.
            //     Therefore it has not been precomputed into a multidimensional array.
            (var submaterial, var rotation, var inversion) = CalculateSubmaterial(adjLots, adjSpot, adjPath, adjRoad);

            int submaterialIndex;
            if (submaterial == ParkingLotSubmaterialIndex.Invalid)
            {
                submaterialIndex = _invalidIndex;
            }
            else
            {
                submaterialIndex = _startIndex + (int)submaterial;
            }

            return (submaterialIndex, rotation, inversion);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adjLots">Adjacency array of parking lots.</param>
        /// <param name="adjSpot">Adjacency array of parking spots.</param>
        /// <param name="adjPath">Adjacency array of paths.</param>
        /// <param name="adjRoad">Adjacency array of roads.</param>
        /// <returns></returns>
        private (ParkingLotSubmaterialIndex submaterial, SubmaterialRotation rotation, SubmaterialInversion inversion) CalculateSubmaterial(bool[] adjLots, bool[] adjSpot, bool[] adjPath, bool[] adjRoad)
        {
            for (int i = 0; i < 4; ++i)
            {
                int i0 = i;
                int i1 = (i + 1) % 4;
                int i2 = (i + 2) % 4;
                int i3 = (i + 3) % 4;

                // four adjacent parking lots -------------------------
                if (adjLots[i0] && adjLots[i1] && adjLots[i2] && adjLots[i3])
                {
                    // parking spot lines
                    if (adjSpot[4] /* NB: Index 4 is special and references the current grid instead of adjacent grids */)
                    {
                        // three adjacent parking spots (edge) - the only supported format
                        if (adjSpot[i0] && adjSpot[i1] && adjSpot[i2] && !adjSpot[i3])
                        {
                            return (ParkingLotSubmaterialIndex.LotParkingSpots, (SubmaterialRotation)i, SubmaterialInversion.None);
                        }
                    }
                    // blank parking lot
                    else
                    {
                        return (ParkingLotSubmaterialIndex.LotBlank, SubmaterialRotation.deg0, SubmaterialInversion.None);
                    }
                }

                // three adjacent parking lots (edge) ---------------------------
                if (adjLots[i0] && adjLots[i1] && adjLots[i2] && !adjLots[i3])
                {
                    // one road (i2)
                    if (adjRoad[i2] && !adjRoad[i3])
                    {
                        int altInv = 1 + ((i + 1) % 2);
                        return (ParkingLotSubmaterialIndex.StraightEdgeRoad, (SubmaterialRotation)i, (SubmaterialInversion)altInv);
                    }
                    // one road (i3)
                    if (!adjRoad[i2] && adjRoad[i3])
                    {
                        return (ParkingLotSubmaterialIndex.StraightEdgeRoad, (SubmaterialRotation)i, SubmaterialInversion.None);
                    }
                    // one path
                    if (adjPath[i3] && !adjRoad[i2] && !adjRoad[i3])
                    {
                        return (ParkingLotSubmaterialIndex.StraightEdgeOnePath, (SubmaterialRotation)i, SubmaterialInversion.None);
                    }
                    // no path or road
                    if (!adjPath[i3] && !adjRoad[i2] && !adjRoad[i3])
                    {
                        return (ParkingLotSubmaterialIndex.StraightEdge, (SubmaterialRotation)i, SubmaterialInversion.None);
                    }
                }

                // two adjacent parking lots (corner) ----------------------------
                if (adjLots[i0] && adjLots[i1] && !adjLots[i2] && !adjLots[i3])
                {
                    // two paths
                    if (adjPath[i2] && adjPath[i3])
                    {
                        return (ParkingLotSubmaterialIndex.CornerEdgeTwoPath, (SubmaterialRotation)i, SubmaterialInversion.None);
                    }
                    // one path (i2)
                    if (adjPath[i2] && !adjPath[i3])
                    {
                        return (ParkingLotSubmaterialIndex.CornerEdgeOnePath, (SubmaterialRotation)i, SubmaterialInversion.None);
                    }
                    // one path (i3)
                    if (!adjPath[i2] && adjPath[i3])
                    {
                        int iRotated = (i + 3) % 4;
                        int altInv = 1 + (i % 2);
                        return (ParkingLotSubmaterialIndex.CornerEdgeOnePath, (SubmaterialRotation)iRotated, (SubmaterialInversion)altInv);
                    }
                    // no path
                    if (!adjPath[i2] && !adjPath[i3])
                    {
                        return (ParkingLotSubmaterialIndex.CornerEdge, (SubmaterialRotation)i, SubmaterialInversion.None);
                    }
                }
            }

            return (ParkingLotSubmaterialIndex.Invalid, SubmaterialRotation.deg0, SubmaterialInversion.None);
        }

        /// <summary>
        /// This enum encodes the expected order of submaterials on the paths/roads sprite sheet.
        /// </summary>
        private enum ParkingLotSubmaterialIndex
        {
            CornerEdge = 0,
            CornerEdgeOnePath = 1,
            CornerEdgeTwoPath = 2,
            StraightEdge = 3,
            LotBlank = 4,
            LotParkingSpots = 5,
            StraightEdgeOnePath = 6,
            StraightEdgeRoad = 7,
            Invalid
        }

        /// <summary>
        /// Keeper of Parking Lot information.
        /// </summary>
        private class ParkingLot
        {
            public ParkingLot(Rectangle footprint)
            {
                Footprint = footprint;
                LotLines = GenerateLotLines(footprint);
            }

            public Rectangle Footprint { get; private set; }
            public bool[,] LotLines { get; private set; }

            private bool[,] GenerateLotLines(Rectangle footprint)
            {
                bool[,] lotSpaces = new bool[footprint.SizeX, footprint.SizeZ];

                AxisAlignment alignment = footprint.SizeX > footprint.SizeZ ? AxisAlignment.XAxis : AxisAlignment.ZAxis;

                int minorAxisSize, majorAxisSize;
                if (alignment == AxisAlignment.XAxis)
                {
                    minorAxisSize = footprint.SizeZ;
                    majorAxisSize = footprint.SizeX;
                }
                else
                {
                    minorAxisSize = footprint.SizeX;
                    majorAxisSize = footprint.SizeZ;
                }

                // adjust the blank space between even and odd size lots
                int adjustBlankSpaces = (minorAxisSize % 2);
                for (int i = 0; i < minorAxisSize; ++i)
                {
                    if ((i + adjustBlankSpaces) % 3 == 0)
                    {
                        continue; // leave a blank space between parking lots
                    }

                    for (int j = 0; j < majorAxisSize; ++j)
                    {
                        if (alignment == AxisAlignment.XAxis)
                        {
                            lotSpaces[j, i] = true;
                        }
                        else
                        {
                            lotSpaces[i, j] = true;
                        }
                    }
                }

                return lotSpaces;
            }
        }
    }
}
