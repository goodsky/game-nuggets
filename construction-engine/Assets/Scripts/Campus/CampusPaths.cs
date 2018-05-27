﻿using GridTerrain;
using System;

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
        }

        /// <summary>
        /// Build a path between two points.
        /// The path must be in a straight line.
        /// </summary>
        /// <param name="start">Starting location of the line.</param>
        /// <param name="end">Ending location of the line.</param>
        public void BuildPath(Point3 start, Point3 end)
        {
            if (start.x != end.x && start.z != end.z)
                throw new InvalidOperationException("Path must be built along an axis-aligned line.");

            if (start.x == end.x && start.z == end.z)
            {
                // Case: Building a single square
                _path[start.x, start.z] = true;
            }
            else if (start.x != end.x)
            {
                // Case: Building a line along the x-axis
                int dx = start.x < end.x ? 1 : -1;
                int length = Math.Abs(start.x - end.x) + 1;

                for (int x = 0; x < length; ++x)
                    _path[start.x + x * dx, start.z] = true;
            }
            else
            {
                // Case: Building a line along the z-axis
                int dz = start.z < end.z ? 1 : -1;
                int length = Math.Abs(start.z - end.z) + 1;

                for (int z = 0; z < length; ++z)
                    _path[start.z + z * dz, start.z] = true;
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
        /// <param name="x"></param>
        /// <param name="z"></param>
        private void SetPathMaterial(int x, int z)
        {
            _terrain.SetSubmaterial(x, z, 1);
        }
    }
}
