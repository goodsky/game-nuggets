using Campus.GridTerrain;
using Common;
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
        /// Place the line cursor along an <see cref="AxisAlignedLine"/>.
        /// </summary>
        /// <param name="line">An axis aligned line.</param>
        /// <param name="isValid">Booleans representing whether each position is valid or not.</param>
        public void Place(AxisAlignedLine line, bool[] isValid = null)
        {
            int maxLineIndex = -1;
            foreach ((int lineIndex, Point2 linePoint) in line.PointsAlongLine())
            {
                maxLineIndex = lineIndex;
                var cursor = _cursors[lineIndex];

                cursor.Activate();
                cursor.SetMaterial(isValid[lineIndex] ? _validMaterial : _invalidMaterial);
                cursor.Place(linePoint.x, linePoint.z);
            }

            for (int lineIndex = maxLineIndex + 1; lineIndex < _cursors.Length; ++lineIndex)
            {
                _cursors[lineIndex].Deactivate();
            }
        }
    }
}
