using Campus.GridTerrain;
using Common;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Campus
{
    /// <summary>
    /// Collection of all roads on campus.
    /// </summary>
    public class CampusRoads
    {
        private GridMesh _terrain;
        private bool[,] _road;

        readonly int[] gridToVertexDx = new[] { 1, 1, 0, 0 };
        readonly int[] gridToVertexDz = new[] { 1, 0, 0, 1 };

        private int[] vertexToGridDx = new int[] { -1, -1, 0, 0 };
        private int[] vertexToGridDz = new int[] { -1, 0, -1, 0 };

        public CampusRoads(CampusData campusData, GridMesh terrain)
        {
            _terrain = terrain;
            _road = new bool[campusData.Terrain.GridCountX + 1, campusData.Terrain.GridCountZ + 1];
            SetupRoadMapping(
                emptyGrassIndex: campusData.Terrain.SubmaterialEmptyGrassIndex,
                roadStartIndex: campusData.Terrain.SubmaterialRoadsIndex);
        }

        /// <summary>
        /// Gets a value representing whether or not there is a road at grid position.
        /// Note: The Roads values stored in this class are vertex based, but his 
        ///       query is grid based.
        /// </summary>
        /// <param name="pos">Grid position to query.</param>
        /// <returns>True if there is a raod, false otherwise.</returns>
        public bool RoadAtPosition(Point2 pos)
        {
            return  _road[pos.x, pos.z] ||
                    _road[pos.x + 1, pos.z] ||
                    _road[pos.x, pos.z + 1] ||
                    _road[pos.x + 1, pos.z + 1];
        }

        /// <summary>
        /// Build a road along the provided line
        /// </summary>
        /// <param name="line">An axis-aligned vertex line to build road at.</param>
        /// <returns>The points on the terrain that have been modified.</returns>
        public IEnumerable<Point2> ConstructRoad(AxisAlignedLine line)
        {
            foreach ((int lineIndex, Point2 vertexPoint) in line.PointsAlongLine())
            {
                if (!_road[vertexPoint.x, vertexPoint.z])
                {
                    _road[vertexPoint.x, vertexPoint.z] = true;
                }
            }

            // Return all the potentially modified grids around the road for updating.
            for (int scanX = Math.Min(line.Start.x, line.End.x) - 1; scanX <= Math.Max(line.Start.x, line.End.x); ++scanX)
                for (int scanZ = Math.Min(line.Start.z, line.End.z) - 1; scanZ <= Math.Max(line.Start.z, line.End.z); ++scanZ)
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
                int vertX = pos.x + gridToVertexDx[i];
                int vertZ = pos.z + gridToVertexDz[i];
                _road[vertX, vertZ] = false;
            }

            // Set the updated materials and whether the grid squares are anchored
            // NB: This search must be one wider than you think due to the way roads are set up.
            for (int scanX = pos.x - 2; scanX <= pos.x + 2; ++scanX)
                for (int scanZ = pos.z - 2; scanZ <= pos.z + 2; ++scanZ)
                    if (_terrain.GridInBounds(scanX, scanZ))
                        yield return new Point2(scanX, scanZ);
        }

        /// <summary>
        /// Update the material of the grid to look like the path.
        /// </summary>
        public (int submaterialIndex, Rotation rotation) GetRoadMaterial(int x, int z)
        {
            // check the 4 adjacent road vertices to pick the correct image
            int[] adj = new int[4];
            for (int i = 0; i < 4; ++i)
            {
                int checkX = x + gridToVertexDx[i];
                int checkZ = z + gridToVertexDz[i];
                adj[i] =
                    (_terrain.VertexInBounds(checkX, checkZ) &&
                    _road[checkX, checkZ])
                        ? 1 : 0;
            }

            return (_subMaterial[adj[0], adj[1], adj[2], adj[3]], _rotation[adj[0], adj[1], adj[2], adj[3]]);
        }

        // mapping from adjacent road vertices to the material + rotation
        // [top-right, bottom-right, bottom-left, top-left]
        // D--A
        // |  |
        // C--B
        private int[,,,] _subMaterial;
        private Rotation[,,,] _rotation;
        private void SetupRoadMapping(int emptyGrassIndex, int roadStartIndex)
        {
            _subMaterial = new int[2, 2, 2, 2];
            _rotation = new Rotation[2, 2, 2, 2];

            // no adjacent ---
            _subMaterial[0, 0, 0, 0] = emptyGrassIndex;
            _rotation[0, 0, 0, 0] = Rotation.deg0;

            // one adjacent ---
            // top-right
            _subMaterial[1, 0, 0, 0] = roadStartIndex + (int)PathSubmaterialIndex.OneAdjacentVertex;
            _rotation[1, 0, 0, 0] = Rotation.deg0;
            // bottom-right
            _subMaterial[0, 1, 0, 0] = roadStartIndex + (int)PathSubmaterialIndex.OneAdjacentVertex;
            _rotation[0, 1, 0, 0] = Rotation.deg90;
            // bottom-left
            _subMaterial[0, 0, 1, 0] = roadStartIndex + (int)PathSubmaterialIndex.OneAdjacentVertex;
            _rotation[0, 0, 1, 0] = Rotation.deg180;
            // top-left
            _subMaterial[0, 0, 0, 1] = roadStartIndex + (int)PathSubmaterialIndex.OneAdjacentVertex;
            _rotation[0, 0, 0, 1] = Rotation.deg270;

            // two adjacent (straight) ---
            // top-right & bottom-right
            _subMaterial[1, 1, 0, 0] = roadStartIndex + (int)PathSubmaterialIndex.TwoAdjacentStraightVertex;
            _rotation[1, 1, 0, 0] = Rotation.deg0;
            // bottom-right & bottom-left
            _subMaterial[0, 1, 1, 0] = roadStartIndex + (int)PathSubmaterialIndex.TwoAdjacentStraightVertex;
            _rotation[0, 1, 1, 0] = Rotation.deg90;
            // bottom-left & top-left
            _subMaterial[0, 0, 1, 1] = roadStartIndex + (int)PathSubmaterialIndex.TwoAdjacentStraightVertex;
            _rotation[0, 0, 1, 1] = Rotation.deg180;
            // top-left & top-right
            _subMaterial[1, 0, 0, 1] = roadStartIndex + (int)PathSubmaterialIndex.TwoAdjacentStraightVertex;
            _rotation[1, 0, 0, 1] = Rotation.deg270;

            // two adjacent (angled) ---
            // top-right & bottom-left
            _subMaterial[1, 0, 1, 0] = roadStartIndex + (int)PathSubmaterialIndex.TwoAdjacentAngledVertex;
            _rotation[1, 0, 1, 0] = Rotation.deg0;
            // bottom-right & top-left
            _subMaterial[0, 1, 0, 1] = roadStartIndex + (int)PathSubmaterialIndex.TwoAdjacentAngledVertex;
            _rotation[0, 1, 0, 1] = Rotation.deg90;

            // three adjacent ---
            // not top-left
            _subMaterial[1, 1, 1, 0] = roadStartIndex + (int)PathSubmaterialIndex.ThreeAdjacentVertex;
            _rotation[1, 1, 1, 0] = Rotation.deg0;
            // not top-right
            _subMaterial[0, 1, 1, 1] = roadStartIndex + (int)PathSubmaterialIndex.ThreeAdjacentVertex;
            _rotation[0, 1, 1, 1] = Rotation.deg90;
            // not bottom-right
            _subMaterial[1, 0, 1, 1] = roadStartIndex + (int)PathSubmaterialIndex.ThreeAdjacentVertex;
            _rotation[1, 0, 1, 1] = Rotation.deg180;
            // not bottom-left
            _subMaterial[1, 1, 0, 1] = roadStartIndex + (int)PathSubmaterialIndex.ThreeAdjacentVertex;
            _rotation[1, 1, 0, 1] = Rotation.deg270;

            // four adjacent ---
            _subMaterial[1, 1, 1, 1] = roadStartIndex + (int)PathSubmaterialIndex.FourAdjacentVertex;
            _rotation[1, 1, 1, 1] = Rotation.deg0;
        }

        /// <summary>
        /// This enum encodes the expected order of submaterials on the paths/roads sprite sheet.
        /// </summary>
        private enum PathSubmaterialIndex
        {
            OneAdjacentVertex = 0,
            TwoAdjacentStraightVertex = 1,
            TwoAdjacentAngledVertex = 2,
            ThreeAdjacentVertex = 3,
            FourAdjacentVertex = 4
        }
    }
}
