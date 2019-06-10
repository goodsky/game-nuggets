using Campus.GridTerrain;
using Common;
using GameData;
using System.Collections.Generic;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Collection of all buildings on campus.
    /// </summary>
    public class CampusBuildings
    {
        private GridMesh _terrain;
        private Building[,] _building;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="terrain">The grid terrain.</param>
        /// <param name="onBuild">Action to execute</param>
        /// <param name="onDestroy"></param>
        public CampusBuildings(GridMesh terrain)
        {
            _terrain = terrain;
            _building = new Building[terrain.CountX, terrain.CountZ];
        }

        /// <summary>
        /// Checks if a building exists at a given grid point.
        /// </summary>
        /// <param name="pos">Grid position to query.</param>
        /// <returns>True if a building exists at position, false otherwise.</returns>
        public bool BuildingAtPosition(Point2 pos)
        {
            return _building[pos.x, pos.z] != null;
        }

        /// <summary>
        /// Build a building at the location.
        /// Registers the taken grid location with the Safe Terrain Editor.
        /// </summary>
        /// <param name="buildingData">The building to construct.</param>
        /// <param name="location">The location of the building.</param>
        /// <returns>The points on the terrain that have been modified.</returns>
        public IEnumerable<Point2> ConstructBuilding(BuildingData buildingData, Point3 location)
        {
            var building = CampusFactory.GenerateBuilding(
                        buildingData,
                        Game.Campus.transform,
                        _terrain.Convert.GridToWorld(location) + new Vector3(0f, 0.01f, 0f) /* Place just above the grass*/,
                        Quaternion.identity);

            int xSize = buildingData.Footprint.GetLength(0);
            int zSize = buildingData.Footprint.GetLength(1);
            for (int dx = 0; dx < xSize; ++dx)
            {
                for (int dz = 0; dz < zSize; ++dz)
                {
                    int gridX = location.x + dx;
                    int gridZ = location.z + dz;

                    if (buildingData.Footprint[dx, dz])
                    {
                        _building[gridX, gridZ] = building;
                        yield return new Point2(gridX, gridZ);
                    }
                }
            }
        }

        /// <summary>
        /// Remove a building at a location.
        /// </summary>
        /// <param name="pos">The position to remove the building at.</param>
        /// <returns>The points on the terrain that have been modified.</returns>
        public IEnumerable<Point2> DestroyBuildingAt(Point2 pos)
        {
            Building building = _building[pos.x, pos.z];
            if (building != null)
            {
                // Potential Bug: Does this WorldToGrid always work?
                //                Could be a dangerous edge case w/ floating point numbers.
                Point3 location = _terrain.Convert.WorldToGrid(building.transform.position);

                int xSize = building.Data.Footprint.GetLength(0);
                int zSize = building.Data.Footprint.GetLength(1);
                for (int dx = 0; dx < xSize; ++dx)
                {
                    for (int dz = 0; dz < zSize; ++dz)
                    {
                        int gridX = location.x + dx;
                        int gridZ = location.z + dz;

                        if (building.Data.Footprint[dx, dz])
                        {
                            _building[gridX, gridZ] = null;
                            yield return new Point2(gridX, gridZ);
                        }
                    }
                }

                Object.Destroy(building.gameObject);
            }
        }
    }
}
