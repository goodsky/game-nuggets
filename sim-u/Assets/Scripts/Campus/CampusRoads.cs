using Campus.GridTerrain;
using Common;
using GameData;
using System;
using System.Collections.Generic;

namespace Campus
{
    /// <summary>
    /// Collection of all roads on campus.
    /// </summary>
    public class CampusRoads
    {
        private readonly GridMesh _terrain;
        private readonly bool[,] _road;
        private readonly int _roadSubmaterialStartIndex;

        public CampusRoads(CampusData campusData, GridMesh terrain)
        {
            _terrain = terrain;
            _road = new bool[campusData.Terrain.GridCountX + 1, campusData.Terrain.GridCountZ + 1];
            SetupRoadMapping(
                emptyGrassIndex: campusData.Terrain.SubmaterialEmptyGrassIndex,
                invalidIndex: campusData.Terrain.SubmaterialInvalidIndex,
                roadStartIndex: campusData.Terrain.SubmaterialRoadsIndex);

            _roadSubmaterialStartIndex = campusData.Terrain.SubmaterialRoadsIndex;
        }

        /// <summary>
        /// Gets a value representing whether or not there is a road at grid position.
        /// Note: The Roads values stored in this class are vertex based, but his 
        ///       query is grid based.
        /// </summary>
        /// <param name="pos">Grid position to query.</param>
        /// <returns>True if there is a raod, false otherwise.</returns>
        public bool RoadAtGrid(Point2 pos)
        {
            return  _road[pos.x, pos.z] ||
                    _road[pos.x + 1, pos.z] ||
                    _road[pos.x, pos.z + 1] ||
                    _road[pos.x + 1, pos.z + 1];
        }

        /// <summary>
        /// Gets a value representing whether or not there is a road at vertex position.
        /// </summary>
        /// <param name="pos">Vertex position to query.</param>
        /// <returns>True if there is a road, false otherwise.</returns>
        public bool RoadAtVertex(Point2 pos)
        {
            return _road[pos.x, pos.z];
        }

        /// <summary>
        /// Gets a value representing whether or not the position is valid for a crosswalk.
        /// </summary>
        /// <param name="pos">Grid position to query.</param>
        /// <returns>True if the road is valid for a crosswalk, false otherwise.</returns>
        public bool IsValidForCrosswalk(Point2 pos)
        {
            // TODO: This is a shortcut for now.
            //       Later we could make this check for "hypothetical" roads as well during road construction.
            //       Later we could make this check that there are two valid road positions (a full valid crossing).
            (int submaterial, var _, var __) = GetRoadMaterial(pos);
            return submaterial == _roadSubmaterialStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentStraightVertex;
        }

        /// <summary>
        /// Build a road along the provided line
        /// </summary>
        /// <param name="line">An axis-aligned vertex line to build road at.</param>
        /// <returns>The points on the terrain that have been modified.</returns>
        public IEnumerable<Point2> ConstructRoad(AxisAlignedLine line)
        {
            foreach ((int lineIndex, Point2 vertexPoint) in line.GetPointsAlongLine())
            {
                if (!_road[vertexPoint.x, vertexPoint.z])
                {
                    _road[vertexPoint.x, vertexPoint.z] = true;
                }
            }

            // Return all the potentially modified grids around the road for updating.
            // This scan has an extra grid on each side due to updates to intersection status.
            for (int scanX = Math.Min(line.Start.x, line.End.x) - 2; scanX <= Math.Max(line.Start.x, line.End.x) + 1; ++scanX)
                for (int scanZ = Math.Min(line.Start.z, line.End.z) - 2; scanZ <= Math.Max(line.Start.z, line.End.z) + 1; ++scanZ)
                    if (_terrain.GridInBounds(scanX, scanZ))
                        yield return new Point2(scanX, scanZ);
        }

        /// <summary>
        /// Remove a path at the position.
        /// </summary>
        /// <param name="pos">The position to remove the path at.</param>
        /// <returns>The points on the terrain that have been modified.</returns>
        public IEnumerable<Point2> DestroyRoadAt(Point2 pos)
        {
            for (int i = 0; i < 4; ++i)
            {
                int vertX = pos.x + GridConverter.GridToVertexDx[i];
                int vertZ = pos.z + GridConverter.GridToVertexDz[i];
                _road[vertX, vertZ] = false;
            }

            // Set the updated materials and whether the grid squares are anchored
            // NB: This search must be one wider than you think due to the way roads are set up.
            for (int scanX = pos.x - 2; scanX <= pos.x + 2; ++scanX)
            {
                for (int scanZ = pos.z - 2; scanZ <= pos.z + 2; ++scanZ)
                {
                    if (_terrain.GridInBounds(scanX, scanZ))
                    {
                        Point2 scan = new Point2(scanX, scanZ);

                        // Crosswalks may need to be destroyed.
                        if (Game.Campus.GetGridUse(scan) == CampusGridUse.Crosswalk &&
                            !IsValidForCrosswalk(scan))
                        {
                            Game.Campus.DestroyAt(scan, filter: CampusGridUse.Crosswalk);
                        }

                        yield return scan;
                    }
                }
            }
        }

        /// <summary>
        /// Update the material of the grid to look like the path.
        /// </summary>
        public (int submaterialIndex, SubmaterialRotation rotation, SubmaterialInversion inversion) GetRoadMaterial(Point2 pos, bool isPathPresent = false)
        {
            // check the 4 adjacent road vertices to pick the correct image
            int[] adj = new int[4];
            for (int i = 0; i < 4; ++i)
            {
                int checkX = pos.x + GridConverter.GridToVertexDx[i];
                int checkZ = pos.z + GridConverter.GridToVertexDz[i];

                adj[i] = 0;

                if (_terrain.VertexInBounds(checkX, checkZ) && _road[checkX, checkZ])
                {
                    int adjacentRoadVertexCount = 0;
                    for (int j = 0; j < 4; ++j)
                    {
                        int interCheckX = checkX + GridConverter.AdjacentVertexDx[j];
                        int interCheckZ = checkZ + GridConverter.AdjacentVertexDz[j];

                        if (_terrain.VertexInBounds(interCheckX, interCheckZ) && _road[interCheckX, interCheckZ])
                        {
                            ++adjacentRoadVertexCount;
                        }
                    }

                    // roads can have intersections or non-intersection vertices
                    // more than 2 paths out of a vertex means it's an intersection
                    adj[i] = adjacentRoadVertexCount > 2 ? 2 : 1;
                }
            }

            int SubmaterialIndex = _mat[adj[0], adj[1], adj[2], adj[3]];
            if (isPathPresent && IsValidForCrosswalk(pos))
            {
                SubmaterialIndex = _roadSubmaterialStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentStraightVertexWithCrosswalk;
            }

            return (SubmaterialIndex, _rot[adj[0], adj[1], adj[2], adj[3]], _inv[adj[0], adj[1], adj[2], adj[3]]);
        }

        // mapping from adjacent road vertices to the material + rotation
        // [top-right, bottom-right, bottom-left, top-left]
        // TL--TR
        // |   |
        // BL--BR
        // *** adjacent roads can either be an non-intersection [1] or intersection [2]
        // *** this is a beautiful artisanal 4 dimensional array. it will cost you extra.
        private int[,,,] _mat;
        private SubmaterialRotation[,,,] _rot;
        private SubmaterialInversion[,,,] _inv;
        private void SetupRoadMapping(int emptyGrassIndex, int invalidIndex, int roadStartIndex)
        {
            _mat = new int[3, 3, 3, 3];
            _rot = new SubmaterialRotation[3, 3, 3, 3];
            _inv = new SubmaterialInversion[3, 3, 3, 3];

            // initialize with invalid material
            for (int i0 = 0; i0 < 3; ++i0)
                for (int i1 = 0; i1 < 3; ++i1)
                    for (int i2 = 0; i2 < 3; ++i2)
                        for (int i3 = 0; i3 < 3; ++i3)
                            _mat[i0, i1, i2, i3] = invalidIndex;

            // no adjacent ---
            _mat[0, 0, 0, 0] = emptyGrassIndex;
            _rot[0, 0, 0, 0] = SubmaterialRotation.deg0;

            // one adjacent ---
            // top-right
            _mat[1, 0, 0, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.OneAdjacentVertex;
            _rot[1, 0, 0, 0] = SubmaterialRotation.deg0;
            // bottom-right
            _mat[0, 1, 0, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.OneAdjacentVertex;
            _rot[0, 1, 0, 0] = SubmaterialRotation.deg90;
            // bottom-left
            _mat[0, 0, 1, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.OneAdjacentVertex;
            _rot[0, 0, 1, 0] = SubmaterialRotation.deg180;
            // top-left
            _mat[0, 0, 0, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.OneAdjacentVertex;
            _rot[0, 0, 0, 1] = SubmaterialRotation.deg270;

            // two adjacent non-intersection (straight) ---
            // top-right & bottom-right
            _mat[1, 1, 0, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentStraightVertex;
            _rot[1, 1, 0, 0] = SubmaterialRotation.deg0;
            // bottom-right & bottom-left
            _mat[0, 1, 1, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentStraightVertex;
            _rot[0, 1, 1, 0] = SubmaterialRotation.deg90;
            // bottom-left & top-left
            _mat[0, 0, 1, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentStraightVertex;
            _rot[0, 0, 1, 1] = SubmaterialRotation.deg180;
            // top-left & top-right
            _mat[1, 0, 0, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentStraightVertex;
            _rot[1, 0, 0, 1] = SubmaterialRotation.deg270;

            // two adjacent (angled) ---
            // top-right & bottom-left
            _mat[1, 0, 1, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentAngledVertex;
            _rot[1, 0, 1, 0] = SubmaterialRotation.deg0;
            // bottom-right & top-left
            _mat[0, 1, 0, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentAngledVertex;
            _rot[0, 1, 0, 1] = SubmaterialRotation.deg90;

            // three adjacent ---
            // top-right & bottom-right & bottom-left
            _mat[1, 1, 1, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentVertex;
            _rot[1, 1, 1, 0] = SubmaterialRotation.deg0;
            // bottom-right & bottom-left & top-left
            _mat[0, 1, 1, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentVertex;
            _rot[0, 1, 1, 1] = SubmaterialRotation.deg90;
            // bottom-left & top-left & top-right
            _mat[1, 0, 1, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentVertex;
            _rot[1, 0, 1, 1] = SubmaterialRotation.deg180;
            // top-left & top-right & bottom-right
            _mat[1, 1, 0, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentVertex;
            _rot[1, 1, 0, 1] = SubmaterialRotation.deg270;

            // two adjacent intersection (I) (straight) ---
            // (I)top-right & bottom-right
            _mat[2, 1, 0, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentStraightIntersectionVertex;
            _rot[2, 1, 0, 0] = SubmaterialRotation.deg0;
            _inv[2, 1, 0, 0] = SubmaterialInversion.None;
            // top-right & (I)bottom-right
            _mat[1, 2, 0, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentStraightIntersectionVertex;
            _rot[1, 2, 0, 0] = SubmaterialRotation.deg0;
            _inv[1, 2, 0, 0] = SubmaterialInversion.InvertZ;
            // (I)bottom-right & bottom-left
            _mat[0, 2, 1, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentStraightIntersectionVertex;
            _rot[0, 2, 1, 0] = SubmaterialRotation.deg90;
            _inv[0, 2, 1, 0] = SubmaterialInversion.None;
            // bottom-right & (I)bottom-left
            _mat[0, 1, 2, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentStraightIntersectionVertex;
            _rot[0, 1, 2, 0] = SubmaterialRotation.deg90;
            _inv[0, 1, 2, 0] = SubmaterialInversion.InvertX;
            // (I)bottom-left & top-left
            _mat[0, 0, 2, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentStraightIntersectionVertex;
            _rot[0, 0, 2, 1] = SubmaterialRotation.deg180;
            _inv[0, 0, 2, 1] = SubmaterialInversion.None;
            // bottom-left & (I)top-left
            _mat[0, 0, 1, 2] = roadStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentStraightIntersectionVertex;
            _rot[0, 0, 1, 2] = SubmaterialRotation.deg180;
            _inv[0, 0, 1, 2] = SubmaterialInversion.InvertZ;
            // (I)top-left & top-right
            _mat[1, 0, 0, 2] = roadStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentStraightIntersectionVertex;
            _rot[1, 0, 0, 2] = SubmaterialRotation.deg270;
            _inv[1, 0,0, 2] = SubmaterialInversion.None;
            // top-left & (I)top-right
            _mat[2, 0, 0, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.TwoAdjacentStraightIntersectionVertex;
            _rot[2, 0, 0, 1] = SubmaterialRotation.deg270;
            _inv[2, 0, 0, 1] = SubmaterialInversion.InvertX;

            // three adjacent center intersection (I) ---
            // top-right & (I)bottom-right & bottom-left
            _mat[1, 2, 1, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentCenterIntersectionVertex;
            _rot[1, 2, 1, 0] = SubmaterialRotation.deg0;
            _inv[1, 2, 1, 0] = SubmaterialInversion.None;
            // bottom-right & (I)bottom-left & top-left
            _mat[0, 1, 2, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentCenterIntersectionVertex;
            _rot[0, 1, 2, 1] = SubmaterialRotation.deg90;
            _inv[0, 1, 2, 1] = SubmaterialInversion.None;
            // bottom-left & (I)top-left & top-right
            _mat[1, 0, 1, 2] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentCenterIntersectionVertex;
            _rot[1, 0, 1, 2] = SubmaterialRotation.deg180;
            _inv[1, 0, 1, 2] = SubmaterialInversion.None;
            // top-left & (I)top-right & bottom-right
            _mat[2, 1, 0, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentCenterIntersectionVertex;
            _rot[2, 1, 0, 1] = SubmaterialRotation.deg270;
            _inv[2, 1, 0, 1] = SubmaterialInversion.None;

            // three adjacent corner intersection (I) ---
            // (I)top-right & bottom-right & bottom-left
            _mat[2, 1, 1, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentCornerIntersectionVertex;
            _rot[2, 1, 1, 0] = SubmaterialRotation.deg0;
            _inv[2, 1, 1, 0] = SubmaterialInversion.None;
            // top-right & bottom-right & (I)bottom-left
            _mat[1, 1, 2, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentCornerIntersectionVertex;
            _rot[1, 1, 2, 0] = SubmaterialRotation.deg90;
            _inv[1, 1, 2, 0] = SubmaterialInversion.InvertX;
            // (I)bottom-right & bottom-left & top-left
            _mat[0, 2, 1, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentCornerIntersectionVertex;
            _rot[0, 2, 1, 1] = SubmaterialRotation.deg90;
            _inv[0, 2, 1, 1] = SubmaterialInversion.None;
            // bottom-right & bottom-left & (I)top-left
            _mat[0, 1, 1, 2] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentCornerIntersectionVertex;
            _rot[0, 1, 1, 2] = SubmaterialRotation.deg180;
            _inv[0, 1, 1, 2] = SubmaterialInversion.InvertZ;
            // (I)bottom-left & top-left & top-right
            _mat[1, 0, 2, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentCornerIntersectionVertex;
            _rot[1, 0, 2, 1] = SubmaterialRotation.deg180;
            _inv[1, 0, 2, 1] = SubmaterialInversion.None;
            // bottom-left & top-left & (I)top-right
            _mat[2, 0, 1, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentCornerIntersectionVertex;
            _rot[2, 0, 1, 1] = SubmaterialRotation.deg270;
            _inv[2, 0, 1, 1] = SubmaterialInversion.InvertX;
            // (I)top-left & top-right & bottom-right
            _mat[1, 1, 0, 2] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentCornerIntersectionVertex;
            _rot[1, 1, 0, 2] = SubmaterialRotation.deg270;
            _inv[1, 1, 0, 2] = SubmaterialInversion.None;
            // top-left & top-right & (I)bottom-right
            _mat[1, 2, 0, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentCornerIntersectionVertex;
            _rot[1, 2, 0, 1] = SubmaterialRotation.deg0;
            _inv[1, 2, 0, 1] = SubmaterialInversion.InvertZ;

            // three adjacent 2 straight intersection (I) ---
            // (I)top-right & (I)bottom-right & bottom-left
            _mat[2, 2, 1, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentStraightIntersectionVertex;
            _rot[2, 2, 1, 0] = SubmaterialRotation.deg0;
            _inv[2, 2, 1, 0] = SubmaterialInversion.None;
            // top-right & (I)bottom-right & (I)bottom-left
            _mat[1, 2, 2, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentStraightIntersectionVertex;
            _rot[1, 2, 2, 0] = SubmaterialRotation.deg90;
            _inv[1, 2, 2, 0] = SubmaterialInversion.InvertX;
            // (I)bottom-right & (I)bottom-left & top-left
            _mat[0, 2, 2, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentStraightIntersectionVertex;
            _rot[0, 2, 2, 1] = SubmaterialRotation.deg90;
            _inv[0, 2, 2, 1] = SubmaterialInversion.None;
            // bottom-right & (I)bottom-left & (I)top-left
            _mat[0, 1, 2, 2] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentStraightIntersectionVertex;
            _rot[0, 1, 2, 2] = SubmaterialRotation.deg180;
            _inv[0, 1, 2, 2] = SubmaterialInversion.InvertZ;
            // (I)bottom-left & (I)top-left & top-right
            _mat[1, 0, 2, 2] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentStraightIntersectionVertex;
            _rot[1, 0, 2, 2] = SubmaterialRotation.deg180;
            _inv[1, 0, 2, 2] = SubmaterialInversion.None;
            // bottom-left & (I)top-left & (I)top-right
            _mat[2, 0, 1, 2] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentStraightIntersectionVertex;
            _rot[2, 0, 1, 2] = SubmaterialRotation.deg270;
            _inv[2, 0, 1, 2] = SubmaterialInversion.InvertX;
            // (I)top-left & (I)top-right & bottom-right
            _mat[2, 1, 0, 2] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentStraightIntersectionVertex;
            _rot[2, 1, 0, 2] = SubmaterialRotation.deg270;
            _inv[2, 1, 0, 2] = SubmaterialInversion.None;
            // top-left & (I)top-right & (I)bottom-right
            _mat[2, 2, 0, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentStraightIntersectionVertex;
            _rot[2, 2, 0, 1] = SubmaterialRotation.deg0;
            _inv[2, 2, 0, 1] = SubmaterialInversion.InvertZ;

            // three adjacent 2 angled intersection (I) ---
            // (I)top-right & bottom-right & (I)bottom-left
            _mat[2, 1, 2, 0] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentAngledIntersectionVertex;
            _rot[2, 1, 2, 0] = SubmaterialRotation.deg0;
            _inv[2, 1, 2, 0] = SubmaterialInversion.None;
            // (I)bottom-right & bottom-left & (I)top-left
            _mat[0, 2, 1, 2] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentAngledIntersectionVertex;
            _rot[0, 2, 1, 2] = SubmaterialRotation.deg90;
            _inv[0, 2, 1, 2] = SubmaterialInversion.None;
            // (I)bottom-left & top-left & (I)top-right
            _mat[2, 0, 2, 1] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentAngledIntersectionVertex;
            _rot[2, 0, 2, 1] = SubmaterialRotation.deg180;
            _inv[2, 0, 2, 1] = SubmaterialInversion.None;
            // (I)top-left & top-right & (I)bottom-right
            _mat[1, 2, 0, 2] = roadStartIndex + (int)RoadsSubmaterialIndex.ThreeAdjacentAngledIntersectionVertex;
            _rot[1, 2, 0, 2] = SubmaterialRotation.deg270;
            _inv[1, 2, 0, 2] = SubmaterialInversion.None;
        }

        /// <summary>
        /// This enum encodes the expected order of submaterials on the paths/roads sprite sheet.
        /// </summary>
        private enum RoadsSubmaterialIndex
        {
            OneAdjacentVertex = 0,
            TwoAdjacentStraightVertex = 1,
            TwoAdjacentAngledVertex = 2,
            ThreeAdjacentVertex = 3,
            TwoAdjacentStraightIntersectionVertex = 4,
            ThreeAdjacentCenterIntersectionVertex = 5,
            ThreeAdjacentCornerIntersectionVertex = 6,
            ThreeAdjacentStraightIntersectionVertex = 7,
            ThreeAdjacentAngledIntersectionVertex = 8,
            TwoAdjacentStraightVertexWithCrosswalk = 9,
        }
    }
}
