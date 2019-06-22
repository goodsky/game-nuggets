﻿using Campus.GridTerrain;
using Common;
using System;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// A group of mesh cursors.
    /// This time, in a rectangle.
    /// </summary>
    public class RectCursor
    {
        private GridMesh _terrain;
        private Material _validMaterial;
        private Material _invalidMaterial;

        private GridCursor[,] _cursors;

        /// <summary>
        /// Instantiates a line cursor.
        /// </summary>
        /// <param name="terrain">The terrain to put cursors on.</param>
        /// <param name="validMaterial">The material to use when a footprint location is valid.</param>
        /// <param name="invalidMaterial">The material to use when a footprint location is invalid.</param>
        public RectCursor(GridMesh terrain, Material validMaterial, Material invalidMaterial)
        {
            _terrain = terrain;
            _validMaterial = validMaterial;
            _invalidMaterial = invalidMaterial;

            // just initialize the max possible and keep them inactive
            // https://answers.unity.com/questions/462942/does-inactive-objects-eat-up-performance.html
            _cursors = new GridCursor[terrain.CountX, terrain.CountZ];
            for (int i = 0; i < terrain.CountX; ++i)
                for (int j = 0; j < terrain.CountZ; ++j)
                    _cursors[i, j] = GridCursor.Create(_terrain, _validMaterial);
        }

        /// <summary>
        /// Place the line cursor between two points.
        /// </summary>
        /// <param name="start">The starting point of the rectangle.</param>
        /// <param name="end">The ending point of the rectangle.</param>
        /// <param name="isValid">Booleans representing whether each position is valid or not.</param>
        public void Place(Point3 start, Point3 end, bool[,] isValid)
        {
            int minX = Math.Min(start.x, end.x);
            int maxX = Math.Max(start.x, end.x);
            int minZ = Math.Min(start.z, end.z);
            int maxZ = Math.Max(start.z, end.z);

            int dx = maxX - minX;
            int dz = maxZ - minZ;

            for (int x = 0; x < _cursors.GetLength(0); ++x)
            {
                for (int z = 0; z < _cursors.GetLength(1); ++z)
                {
                    if (x > dx || z > dz)
                    {
                        _cursors[x, z].Deactivate();
                        continue;
                    }

                    _cursors[x, z].Activate();
                    _cursors[x, z].SetMaterial(isValid[x, z] ? _validMaterial : _invalidMaterial);
                    _cursors[x, z].Place(new Point2(minX + x, minZ + z));
                }
            }
        }

        /// <summary>
        /// Deactivate all cursors.
        /// </summary>
        public void Deactivate()
        {
            if (_cursors != null)
            {
                foreach (var cursor in _cursors)
                {
                    if (cursor != null)
                    {
                        cursor.Deactivate();
                    }
                }
            }
        }

        /// <summary>
        /// Destroy the cursor game objects.
        /// </summary>
        public void Destroy()
        {
            if (_cursors != null)
            {
                foreach (var cursor in _cursors)
                {
                    if (cursor != null)
                    {
                        cursor.Deactivate();
                        UnityEngine.Object.Destroy(cursor);
                    }
                }

                _cursors = null;
            }
        }
    }
}
