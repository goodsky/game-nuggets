using Campus.GridTerrain;
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

        public CampusPaths(GridMesh terrain)
        {
            _terrain = terrain;
            _path = new bool[_terrain.CountX, _terrain.CountZ];
            SetupPathMapping();
        }

        /// <summary>
        /// Gets a value representing whether or not there is a path at grid position.
        /// </summary>
        /// <param name="posx">The x position to check.</param>
        /// <param name="posz">The z position to check.</param>
        /// <returns>True if there is a path, false otherwise.</returns>
        public bool IsPath(int posx, int posz)
        {
            return
                posx >= 0 && posx < _terrain.CountX &&
                posz >= 0 && posz < _terrain.CountZ &&
                _path[posx, posz];
        }

        /// <summary>
        /// Build a path between two points.
        /// The path must be in a straight line.
        /// </summary>
        /// <param name="start">Starting location of the line.</param>
        /// <param name="end">Ending location of the line.</param>
        public void BuildPath(Point3 start, Point3 end)
        {
            if (Application.isEditor)
            {
                Assert.IsFalse(start.x != end.x && start.z != end.z, "Placing path in invalid location!");
            }

            int dx = 0;
            int dz = 0;
            int length = 1;

            if (start.x == end.x && start.z == end.z)
            {
                // Case: Building a single square
            }
            else if (start.x != end.x)
            {
                // Case: Building a line along the x-axis
                dx = start.x < end.x ? 1 : -1;
                length = Math.Abs(start.x - end.x) + 1;
            }
            else
            {
                // Case: Building a line along the z-axis
                dz = start.z < end.z ? 1 : -1;
                length = Math.Abs(start.z - end.z) + 1;
            }

            // Set the paths
            for (int i = 0; i < length; ++i)
            {
                int gridX = start.x + i * dx;
                int gridZ = start.z + i * dz;

                if (!_path[gridX, gridZ])
                {
                    _path[gridX, gridZ] = true;
                    _terrain.Editor.SetAnchored(gridX, gridZ);
                }
            }

            // Set the updated materials
            for (int scanX = Math.Min(start.x, end.x) - 1; scanX <= Math.Max(start.x, end.x) + 1; ++scanX)
                for (int scanZ = Math.Min(start.z, end.z) - 1; scanZ <= Math.Max(start.z, end.z) + 1; ++scanZ)
                    if (scanX >= 0 && scanX < _terrain.CountX &&
                        scanZ >= 0 && scanZ < _terrain.CountZ)
                        SetPathMaterial(scanX, scanZ);
        }

        /// <summary>
        /// Update the material of the grid to look like the path.
        /// </summary>
        readonly int[] dx = new[] { 0, 1, 0, -1 };
        readonly int[] dz = new[] { 1, 0, -1, 0 };
        private void SetPathMaterial(int x, int z)
        {
            if (!_path[x, z])
            {
                _terrain.SetSubmaterial(x, z, 0);
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
                        (checkX > 0 && checkX < _terrain.CountX &&
                         checkZ > 0 && checkZ < _terrain.CountZ &&
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
        private int[,,,] _subMaterial;
        private Rotation[,,,] _rotation;
        private void SetupPathMapping()
        {
            // This is fun! Seemed like the best way to do it at the time. 
            // Future people: let me know if I'm an idiot.

            _subMaterial = new int[2, 2, 2, 2];
            _rotation = new Rotation[2, 2, 2, 2];

            // no adjacent ---
            _subMaterial[0, 0, 0, 0] = 1;
            _rotation   [0, 0, 0, 0] = Rotation.deg0;

            // one adjacent ---
            // top
            _subMaterial[1, 0, 0, 0] = 2;
            _rotation   [1, 0, 0, 0] = Rotation.deg0;
            // right
            _subMaterial[0, 1, 0, 0] = 2;
            _rotation   [0, 1, 0, 0] = Rotation.deg90;
            // bottom
            _subMaterial[0, 0, 1, 0] = 2;
            _rotation   [0, 0, 1, 0] = Rotation.deg180;
            // left
            _subMaterial[0, 0, 0, 1] = 2;
            _rotation   [0, 0, 0, 1] = Rotation.deg270;

            // two adjacent (angled) ---
            // top & right
            _subMaterial[1, 1, 0, 0] = 3;
            _rotation   [1, 1, 0, 0] = Rotation.deg0;
            // right & bottom
            _subMaterial[0, 1, 1, 0] = 3;
            _rotation   [0, 1, 1, 0] = Rotation.deg90;
            // bottom & left
            _subMaterial[0, 0, 1, 1] = 3;
            _rotation   [0, 0, 1, 1] = Rotation.deg180;
            // left & top
            _subMaterial[1, 0, 0, 1] = 3;
            _rotation   [1, 0, 0, 1] = Rotation.deg270;

            // two adjacent (straight) ---
            // top & bottom
            _subMaterial[1, 0, 1, 0] = 4;
            _rotation   [1, 0, 1, 0] = Rotation.deg0;
            // right & left
            _subMaterial[0, 1, 0, 1] = 4;
            _rotation   [0, 1, 0, 1] = Rotation.deg90;

            // three adjacent ---
            // not left
            _subMaterial[1, 1, 1, 0] = 5;
            _rotation   [1, 1, 1, 0] = Rotation.deg0;
            // not top
            _subMaterial[0, 1, 1, 1] = 5;
            _rotation   [0, 1, 1, 1] = Rotation.deg90;
            // not right
            _subMaterial[1, 0, 1, 1] = 5;
            _rotation   [1, 0, 1, 1] = Rotation.deg180;
            // not bottom
            _subMaterial[1, 1, 0, 1] = 5;
            _rotation   [1, 1, 0, 1] = Rotation.deg270;

            // four adjacent ---
            _subMaterial[1, 1, 1, 1] = 6;
            _rotation   [1, 1, 1, 1] = Rotation.deg0;
        }
    }
}
