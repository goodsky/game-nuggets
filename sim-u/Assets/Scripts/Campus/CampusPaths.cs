using Campus.GridTerrain;
using Common;
using GameData;
using System;
using System.Collections.Generic;

namespace Campus
{
    /// <summary>
    /// Collection of all paths on campus.
    /// </summary>
    public class CampusPaths
    {
        private GridMesh _terrain;
        private bool[,] _path;

        private int _emptyGrassSubmaterialIndex;

        public CampusPaths(CampusData campusData, GridMesh terrain)
        {
            _terrain = terrain;
            _path = new bool[_terrain.CountX, _terrain.CountZ];
            _emptyGrassSubmaterialIndex = campusData.Terrain.SubmaterialEmptyGrassIndex;
            SetupPathMapping(campusData.Terrain.SubmaterialPathsIndex);
        }

        /// <summary>
        /// Gets a value representing whether or not there is a path at grid position.
        /// </summary>
        /// <param name="pos">Grid position to query.</param>
        /// <returns>True if there is a path, false otherwise.</returns>
        public bool PathAtPosition(Point2 pos)
        {
            return _path[pos.x, pos.z];
        }

        /// <summary>
        /// Build a path at the position.
        /// </summary>
        /// <param name="line">The line to construct along.</param>
        /// <returns>The points on the terrain that have been modified.</returns>
        public IEnumerable<Point2> ConstructPath(AxisAlignedLine line)
        {
            foreach ((int lineIndex, Point2 point) in line.PointsAlongLine())
            {
                if (!_path[point.x, point.z])
                {
                    _path[point.x, point.z] = true;
                }
            }

            // Set the updated materials
            for (int scanX = Math.Min(line.Start.x, line.End.x) - 1; scanX <= Math.Max(line.Start.x, line.End.x) + 1; ++scanX)
                for (int scanZ = Math.Min(line.Start.z, line.End.z) - 1; scanZ <= Math.Max(line.Start.z, line.End.z) + 1; ++scanZ)
                    if (_terrain.GridInBounds(scanX, scanZ))
                        yield return new Point2(scanX, scanZ);
        }

        /// <summary>
        /// Remove a path at the position.
        /// </summary>
        /// <param name="pos">The position to remove the path at.</param>
        /// <returns>The points on the terrain that have been modified.</returns>
        public IEnumerable<Point2> DestroyPathAt(Point2 pos)
        {
            if (_path[pos.x, pos.z])
            {
                _path[pos.x, pos.z] = false;
            }

            for (int scanX = pos.x - 1; scanX <= pos.x + 1; ++scanX)
                for (int scanZ = pos.z - 1; scanZ <= pos.z + 1; ++scanZ)
                    if (_terrain.GridInBounds(scanX, scanZ))
                        yield return new Point2(scanX, scanZ);
        }

        /// <summary>
        /// Update the material of the grid to look like the path.
        /// </summary>
        readonly int[] adjacencyDx = new[] { 0, 1, 0, -1 };
        readonly int[] adjacentyDz = new[] { 1, 0, -1, 0 };
        public (int submaterialIndex, Rotation rotation) GetPathMaterial(int x, int z)
        {
            if (!_path[x, z])
            {
                return (_emptyGrassSubmaterialIndex, Rotation.deg0);
            }
            else
            {
                // check the 4 adjacent squares to pick the correct image
                int[] adj = new int[4];
                for (int i = 0; i < 4; ++i)
                {
                    int checkX = x + adjacencyDx[i];
                    int checkZ = z + adjacentyDz[i];
                    adj[i] =
                        (_terrain.GridInBounds(checkX, checkZ) &&
                         _path[checkX, checkZ])
                         ? 1 : 0;
                }

                return (_subMaterial[adj[0], adj[1], adj[2], adj[3]], _rotation[adj[0], adj[1], adj[2], adj[3]]);
            }
        }

        // mapping from adjacent paths to the material + rotation
        // [top, right, bottom, left]
        //  |A |
        //--+--+--
        // D|  |B
        //--+--+--
        //  |C |
        private int[,,,] _subMaterial;
        private Rotation[,,,] _rotation;
        private void SetupPathMapping(int startIndex)
        {
            _subMaterial = new int[2, 2, 2, 2];
            _rotation = new Rotation[2, 2, 2, 2];

            // no adjacent ---
            _subMaterial[0, 0, 0, 0] = startIndex + (int)PathSubmaterialIndex.NoAdjacent;
            _rotation   [0, 0, 0, 0] = Rotation.deg0;

            // one adjacent ---
            // top
            _subMaterial[1, 0, 0, 0] = startIndex + (int)PathSubmaterialIndex.OneAdjacent;
            _rotation   [1, 0, 0, 0] = Rotation.deg0;
            // right
            _subMaterial[0, 1, 0, 0] = startIndex + (int)PathSubmaterialIndex.OneAdjacent;
            _rotation   [0, 1, 0, 0] = Rotation.deg90;
            // bottom
            _subMaterial[0, 0, 1, 0] = startIndex + (int)PathSubmaterialIndex.OneAdjacent;
            _rotation   [0, 0, 1, 0] = Rotation.deg180;
            // left
            _subMaterial[0, 0, 0, 1] = startIndex + (int)PathSubmaterialIndex.OneAdjacent;
            _rotation   [0, 0, 0, 1] = Rotation.deg270;

            // two adjacent (angled) ---
            // top & right
            _subMaterial[1, 1, 0, 0] = startIndex + (int)PathSubmaterialIndex.TwoAdjacentAngled;
            _rotation   [1, 1, 0, 0] = Rotation.deg0;
            // right & bottom
            _subMaterial[0, 1, 1, 0] = startIndex + (int)PathSubmaterialIndex.TwoAdjacentAngled;
            _rotation   [0, 1, 1, 0] = Rotation.deg90;
            // bottom & left
            _subMaterial[0, 0, 1, 1] = startIndex + (int)PathSubmaterialIndex.TwoAdjacentAngled;
            _rotation   [0, 0, 1, 1] = Rotation.deg180;
            // left & top
            _subMaterial[1, 0, 0, 1] = startIndex + (int)PathSubmaterialIndex.TwoAdjacentAngled;
            _rotation   [1, 0, 0, 1] = Rotation.deg270;

            // two adjacent (straight) ---
            // top & bottom
            _subMaterial[1, 0, 1, 0] = startIndex + (int)PathSubmaterialIndex.TwoAdjacentStraight;
            _rotation   [1, 0, 1, 0] = Rotation.deg0;
            // right & left
            _subMaterial[0, 1, 0, 1] = startIndex + (int)PathSubmaterialIndex.TwoAdjacentStraight;
            _rotation   [0, 1, 0, 1] = Rotation.deg90;

            // three adjacent ---
            // not left
            _subMaterial[1, 1, 1, 0] = startIndex + (int)PathSubmaterialIndex.ThreeAdjacent;
            _rotation   [1, 1, 1, 0] = Rotation.deg0;
            // not top
            _subMaterial[0, 1, 1, 1] = startIndex + (int)PathSubmaterialIndex.ThreeAdjacent;
            _rotation   [0, 1, 1, 1] = Rotation.deg90;
            // not right
            _subMaterial[1, 0, 1, 1] = startIndex + (int)PathSubmaterialIndex.ThreeAdjacent;
            _rotation   [1, 0, 1, 1] = Rotation.deg180;
            // not bottom
            _subMaterial[1, 1, 0, 1] = startIndex + (int)PathSubmaterialIndex.ThreeAdjacent;
            _rotation   [1, 1, 0, 1] = Rotation.deg270;

            // four adjacent ---
            _subMaterial[1, 1, 1, 1] = startIndex + (int)PathSubmaterialIndex.FourAdjacent;
            _rotation   [1, 1, 1, 1] = Rotation.deg0;
        }

        /// <summary>
        /// This enum encodes the expected order of submaterials on the paths/roads sprite sheet.
        /// </summary>
        private enum PathSubmaterialIndex
        {
            NoAdjacent = 0,
            OneAdjacent = 1,
            TwoAdjacentAngled = 2,
            TwoAdjacentStraight = 3,
            ThreeAdjacent = 4,
            FourAdjacent = 5
        }
    }
}
