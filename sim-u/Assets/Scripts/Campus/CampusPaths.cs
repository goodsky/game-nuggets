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

            SetupPathMapping(
                invalidIndex: campusData.Terrain.SubmaterialInvalidIndex,
                startIndex: campusData.Terrain.SubmaterialPathsIndex);
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
            foreach ((int lineIndex, Point2 point) in line.GetPointsAlongLine())
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
        public (int submaterialIndex, SubmaterialRotation rotation, SubmaterialInversion inversion) GetPathMaterial(Point2 pos)
        {
            if (!_path[pos.x, pos.z])
            {
                return (_emptyGrassSubmaterialIndex, SubmaterialRotation.deg0, SubmaterialInversion.None);
            }
            else
            {
                // check the 4 adjacent squares to pick the correct image
                int[] adj = new int[4];
                for (int i = 0; i < 4; ++i)
                {
                    int checkX = pos.x + GridConverter.AdjacentGridDx[i];
                    int checkZ = pos.z + GridConverter.AdjacentGridDz[i];
                    adj[i] =
                        (_terrain.GridInBounds(checkX, checkZ) &&
                         _path[checkX, checkZ])
                         ? 1 : 0;
                }

                return (_mat[adj[0], adj[1], adj[2], adj[3]], _rot[adj[0], adj[1], adj[2], adj[3]], SubmaterialInversion.None);
            }
        }

        // mapping from adjacent paths to the material + rotation
        // [top, right, bottom, left]
        //  |A |
        //--+--+--
        // D|  |B
        //--+--+--
        //  |C |
        private int[,,,] _mat;
        private SubmaterialRotation[,,,] _rot;
        private void SetupPathMapping(int invalidIndex, int startIndex)
        {
            _mat = new int[2, 2, 2, 2];
            _rot = new SubmaterialRotation[2, 2, 2, 2];

            // initialize with invalid material
            for (int i0 = 0; i0 < 2; ++i0)
                for (int i1 = 0; i1 < 2; ++i1)
                    for (int i2 = 0; i2 < 2; ++i2)
                        for (int i3 = 0; i3 < 2; ++i3)
                            _mat[i0, i1, i2, i3] = invalidIndex;

            // no adjacent ---
            _mat[0, 0, 0, 0] = startIndex + (int)PathSubmaterialIndex.NoAdjacent;
            _rot[0, 0, 0, 0] = SubmaterialRotation.deg0;

            // one adjacent ---
            // top
            _mat[1, 0, 0, 0] = startIndex + (int)PathSubmaterialIndex.OneAdjacent;
            _rot[1, 0, 0, 0] = SubmaterialRotation.deg0;
            // right
            _mat[0, 1, 0, 0] = startIndex + (int)PathSubmaterialIndex.OneAdjacent;
            _rot[0, 1, 0, 0] = SubmaterialRotation.deg90;
            // bottom
            _mat[0, 0, 1, 0] = startIndex + (int)PathSubmaterialIndex.OneAdjacent;
            _rot[0, 0, 1, 0] = SubmaterialRotation.deg180;
            // left
            _mat[0, 0, 0, 1] = startIndex + (int)PathSubmaterialIndex.OneAdjacent;
            _rot[0, 0, 0, 1] = SubmaterialRotation.deg270;

            // two adjacent (angled) ---
            // top & right
            _mat[1, 1, 0, 0] = startIndex + (int)PathSubmaterialIndex.TwoAdjacentAngled;
            _rot[1, 1, 0, 0] = SubmaterialRotation.deg0;
            // right & bottom
            _mat[0, 1, 1, 0] = startIndex + (int)PathSubmaterialIndex.TwoAdjacentAngled;
            _rot[0, 1, 1, 0] = SubmaterialRotation.deg90;
            // bottom & left
            _mat[0, 0, 1, 1] = startIndex + (int)PathSubmaterialIndex.TwoAdjacentAngled;
            _rot[0, 0, 1, 1] = SubmaterialRotation.deg180;
            // left & top
            _mat[1, 0, 0, 1] = startIndex + (int)PathSubmaterialIndex.TwoAdjacentAngled;
            _rot[1, 0, 0, 1] = SubmaterialRotation.deg270;

            // two adjacent (straight) ---
            // top & bottom
            _mat[1, 0, 1, 0] = startIndex + (int)PathSubmaterialIndex.TwoAdjacentStraight;
            _rot[1, 0, 1, 0] = SubmaterialRotation.deg0;
            // right & left
            _mat[0, 1, 0, 1] = startIndex + (int)PathSubmaterialIndex.TwoAdjacentStraight;
            _rot[0, 1, 0, 1] = SubmaterialRotation.deg90;

            // three adjacent ---
            // not left
            _mat[1, 1, 1, 0] = startIndex + (int)PathSubmaterialIndex.ThreeAdjacent;
            _rot[1, 1, 1, 0] = SubmaterialRotation.deg0;
            // not top
            _mat[0, 1, 1, 1] = startIndex + (int)PathSubmaterialIndex.ThreeAdjacent;
            _rot[0, 1, 1, 1] = SubmaterialRotation.deg90;
            // not right
            _mat[1, 0, 1, 1] = startIndex + (int)PathSubmaterialIndex.ThreeAdjacent;
            _rot[1, 0, 1, 1] = SubmaterialRotation.deg180;
            // not bottom
            _mat[1, 1, 0, 1] = startIndex + (int)PathSubmaterialIndex.ThreeAdjacent;
            _rot[1, 1, 0, 1] = SubmaterialRotation.deg270;

            // four adjacent ---
            _mat[1, 1, 1, 1] = startIndex + (int)PathSubmaterialIndex.FourAdjacent;
            _rot[1, 1, 1, 1] = SubmaterialRotation.deg0;
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
