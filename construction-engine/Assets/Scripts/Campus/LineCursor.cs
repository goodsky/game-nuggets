using GridTerrain;
using System;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// A group of mesh cursors.
    /// This time, in a straight line.
    /// </summary>
    public class LineCursor
    {
        private GridMesh _terrain;
        private Material _validMaterial;
        private Material _invalidMaterial;

        private GridCursor[] _cursors;

        /// <summary>
        /// Instantiates a line cursor.
        /// </summary>
        /// <param name="terrain">The terrain to put cursors on.</param>
        /// <param name="validMaterial">The material to use when a footprint location is valid.</param>
        /// <param name="invalidMaterial">The material to use when a footprint location is invalid.</param>
        public LineCursor(GridMesh terrain, Material validMaterial, Material invalidMaterial)
        {
            _terrain = terrain;
            _validMaterial = validMaterial;
            _invalidMaterial = invalidMaterial;

            // just initialize the max possible and keep them inactive
            // https://answers.unity.com/questions/462942/does-inactive-objects-eat-up-performance.html
            _cursors = new GridCursor[Math.Max(terrain.CountX, terrain.CountZ)];
            for (int i = 0; i < _cursors.Length; ++i)
            {
                _cursors[i] = GridCursor.Create(_terrain, _validMaterial);
                _cursors[i].Deactivate();
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
        /// Place the cursors between two points.
        /// </summary>
        /// <param name="start">Starting location of the line.</param>
        /// <param name="end">Ending location of the line.</param>
        /// <param name="isValid">Booleans representing whether each position is valid or not.</param>
        public void Place(Point3 start, Point3 end, bool[] isValid = null)
        {
            if (start.x != end.x && start.z != end.z)
                throw new InvalidOperationException("LineCursor must be placed along an axis-aligned line.");

            int dx = 0;
            int dz = 0;
            int length = 1;

            if (start.x == end.x && start.z == end.z)
            {
                // Case: Placing a single square
            }
            else if (start.x != end.x)
            {
                // Case: Placing a line along the x-axis
                dx = start.x < end.x ? 1 : -1;
                length = Math.Abs(start.x - end.x) + 1;
            }
            else
            {
                // Case: Placing a line along the z-axis
                dz = start.z < end.z ? 1 : -1;
                length = Math.Abs(start.z - end.z) + 1;
            }

            for (int i = 0; i < _cursors.Length; ++i)
            {
                if (i >= length)
                {
                    if (!_cursors[i].IsActive)
                        break;

                    // disable unused cursors
                    _cursors[i].Deactivate();
                    continue;
                }

                int gridX = start.x + i * dx;
                int gridZ = start.z + i * dz;

                var cursor = _cursors[i];
                cursor.Activate();
                cursor.SetMaterial(isValid[i] ? _validMaterial : _invalidMaterial);
                cursor.Place(gridX, gridZ);
            }
        }
    }
}
