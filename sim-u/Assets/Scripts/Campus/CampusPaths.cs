using Campus.GridTerrain;
using Common;
using GameData;
using System;
using UnityEngine;
using UnityEngine.Assertions;

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
            return
                pos.x >= 0 && pos.x < _terrain.CountX &&
                pos.z >= 0 && pos.z < _terrain.CountZ &&
                _path[pos.x, pos.z];
        }

        /// <summary>
        /// Build a path at the position.
        /// </summary>
        /// <param name="line">The line to construct along.</param>
        public void ConstructPath(AxisAlignedLine line)
        {
            foreach ((int lineIndex, Point2 point) in line.PointsAlongLine())
            {
                if (!_path[point.x, point.z])
                {
                    _path[point.x, point.z] = true;
                    _terrain.Editor.SetAnchored(point.x, point.z);
                }
            }

            // Set the updated materials
            for (int scanX = Math.Min(line.Start.x, line.End.x) - 1; scanX <= Math.Max(line.Start.x, line.End.x) + 1; ++scanX)
                for (int scanZ = Math.Min(line.Start.z, line.End.z) - 1; scanZ <= Math.Max(line.Start.z, line.End.z) + 1; ++scanZ)
                    UpdatePathMaterial(scanX, scanZ);
        }

        /// <summary>
        /// Remove a path at the position.
        /// </summary>
        /// <param name="pos">The position to remove the path at.</param>
        public void DestroyPathAt(Point2 pos)
        {
            if (_path[pos.x, pos.z])
            {
                _path[pos.x, pos.z] = false;
                _terrain.Editor.RemoveAnchor(pos.x, pos.z);
            }

            for (int scanX = pos.x - 1; scanX <= pos.x + 1; ++scanX)
                for (int scanZ = pos.z - 1; scanZ <= pos.z + 1; ++scanZ)
                    UpdatePathMaterial(scanX, scanZ);
        }

        /// <summary>
        /// Update the material of the grid to look like the path.
        /// </summary>
        readonly int[] dx = new[] { 0, 1, 0, -1 };
        readonly int[] dz = new[] { 1, 0, -1, 0 };
        private void UpdatePathMaterial(int x, int z)
        {
            if (x < 0 || x >= _terrain.CountX ||
                z < 0 || z >= _terrain.CountZ)
            {
                return;
            }

            if (!_path[x, z])
            {
                _terrain.SetSubmaterial(x, z, _emptyGrassSubmaterialIndex);
            }
            else
            {
                // check the 4 adjacent squares to pick the correct image
                int[] adj = new int[4];
                for (int i = 0; i < 4; ++i)
                {
                    int checkX = x + dx[i];
                    int checkZ = z + dz[i];
                    adj[i] =
                        (checkX >= 0 && checkX < _terrain.CountX &&
                         checkZ >= 0 && checkZ < _terrain.CountZ &&
                         _path[checkX, checkZ])
                         ? 1 : 0;
                }

                _terrain.SetSubmaterial(
                    x, 
                    z, 
                    _subMaterial[adj[0], adj[1], adj[2], adj[3]],
                    _rotation[adj[0], adj[1], adj[2], adj[3]]);
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
