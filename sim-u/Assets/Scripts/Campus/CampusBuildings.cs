using Campus.GridTerrain;
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
        private Dictionary<Point2, Building> _buildings = new Dictionary<Point2, Building>();
        private GridMesh _terrain;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="terrain">The grid terrain.</param>
        /// <param name="onBuild">Action to execute</param>
        /// <param name="onDestroy"></param>
        public CampusBuildings(GridMesh terrain)
        {
            _terrain = terrain;
        }

        /// <summary>
        /// Checks if a building exists at a given grid point.
        /// </summary>
        /// <param name="pos">Grid position to query.</param>
        /// <returns>True if a building exists at position, false otherwise.</returns>
        public bool BuildingAtPosition(Point2 pos)
        {
            return
                pos.x >= 0 && pos.x < _terrain.CountX &&
                pos.y >= 0 && pos.y < _terrain.CountZ &&
                _buildings.TryGetValue(pos, out Building _);
        }

        /// <summary>
        /// Build a building at the location.
        /// Registers the taken grid location with the Safe Terrain Editor.
        /// </summary>
        /// <param name="buildingData">The building to construct.</param>
        /// <param name="location">The location of the building.</param>
        public void ConstructBuilding(BuildingData buildingData, Point3 location)
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
                        _buildings.Add(new Point2(gridX, gridZ), building);
                        _terrain.Editor.SetAnchored(gridX, gridZ);
                    }
                }
            }
        }
    }
}
