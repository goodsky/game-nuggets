using Common;
using GridTerrain;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// A group of mesh cursors.
    /// Displays things like building footprints.
    /// </summary>
    public class FootprintCursor
    {
        public Point2 Position { get; private set; }

        private GridMesh _terrain;
        private Material _validMaterial;
        private Material _invalidMaterial;
        private GridCursor[,] _cursors;

        /// <summary>
        /// Instantiates a footprint cursor.
        /// </summary>
        /// <param name="terrain">The terrain to put cursors on.</param>
        /// <param name="validMaterial">The material to use when a footprint location is valid.</param>
        /// <param name="invalidMaterial">The material to use when a footprint location is invalid.</param>
        public FootprintCursor(GridMesh terrain, Material validMaterial, Material invalidMaterial)
        {
            _terrain = terrain;
            _validMaterial = validMaterial;
            _invalidMaterial = invalidMaterial;
        }

        /// <summary>
        /// Creates a new collection of cursors.
        /// </summary>
        /// <param name="footprint">The footprint of the building.</param>
        public void Create(bool[,] footprint)
        {
            if (_cursors != null)
                GameLogger.FatalError("Can't recreate a footprint cursor while one already exists!");

            int xSize = footprint.GetLength(0);
            int zSize = footprint.GetLength(1);

            _cursors = new GridCursor[xSize, zSize];
            for (int x = 0; x < xSize; ++x)
            {
                for (int z = 0; z < zSize; ++z)
                {
                    if (footprint[x, z])
                    {
                        _cursors[x, z] = GridCursor.Create(_terrain, _validMaterial);
                    }
                    else
                    {
                        _cursors[x, z] = null;
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

        /// <summary>
        /// Activate all cursors.
        /// </summary>
        public void Activate()
        {
            if (_cursors != null)
            {
                foreach (var cursor in _cursors)
                {
                    if (cursor != null)
                    {
                        cursor.Activate();
                    }
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
        /// Set the color of the cursors based on valid locations.
        /// </summary>
        /// <param name="validLocations">An array representing which locations are valid.</param>
        public void SetMaterials(bool[,] validLocations)
        {
            int xSize = _cursors.GetLength(0);
            int zSize = _cursors.GetLength(1);

            for (int x = 0; x < xSize; ++x)
            {
                for (int z = 0; z < zSize; ++z)
                {
                    if (_cursors != null && _cursors[x, z] != null)
                    {
                        _cursors[x, z].SetMaterial(validLocations[x, z] ? _validMaterial : _invalidMaterial);
                    }
                }
            }
        }

        /// <summary>
        /// Place the cursors at a location.
        /// </summary>
        /// <param name="location"></param>
        public void Place(Point3 location)
        {
            Position = new Point2(location.x, location.z);

            int xSize = _cursors.GetLength(0);
            int zSize = _cursors.GetLength(1);

            for (int x = 0; x < xSize; ++x)
            {
                for (int z = 0; z < zSize; ++z)
                {
                    if (_cursors != null && _cursors[x, z] != null)
                    {
                        int cursorX = location.x + x;
                        int cursorZ = location.z + z;

                        if (cursorX < _terrain.CountX && cursorZ < _terrain.CountZ)
                        {
                            _cursors[x, z].Activate();
                            _cursors[x, z].Place(cursorX, cursorZ);
                        }
                        else
                        {
                            _cursors[x, z].Deactivate();
                        }
                    }
                }
            }
        }
    }
}
