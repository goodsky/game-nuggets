using Campus.GridTerrain;
using Common;
using GameData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Collection of all buildings on campus.
    /// </summary>
    public class CampusBuildings
    {
        private readonly CampusManager _campusManager;
        private readonly GameDataStore _gameData;
        private readonly GridMesh _terrain;

        private readonly Building[,] _buildingAtGridPosition;

        public CampusBuildings(CampusData campusData, GameAccessor accessor)
        {
            _campusManager = accessor.CampusManager;
            _gameData = accessor.GameData;
            _terrain = accessor.Terrain;
            _buildingAtGridPosition = new Building[_terrain.CountX, _terrain.CountZ];
        }

        /// <summary>
        /// Gets the internal save state for campus buildings.
        /// </summary>
        public BuildingSaveState[] SaveGameState()
        {
            return Utils.GetDistinct(_buildingAtGridPosition)
                .Select(building =>
                    new BuildingSaveState
                    {
                        BuildingDataName = building.Data.Name,
                        PositionX = building.Location.x,
                        PositionY = building.Location.y,
                        PositionZ = building.Location.z,
                        Rotation = building.Rotation,
                    })
                .ToArray();
        }

        /// <summary>
        /// Load the save game state.
        /// </summary>
        public void LoadGameState(BuildingSaveState[] buildingState)
        {
            if (buildingState != null)
            {
                foreach (BuildingSaveState savedBuilding in buildingState)
                {
                    BuildingData buildingData = _gameData.Get<BuildingData>(GameDataType.Building, savedBuilding.BuildingDataName);
                    Point3 position = new Point3(savedBuilding.PositionX, savedBuilding.PositionY, savedBuilding.PositionZ);
                    BuildingRotation rotation = savedBuilding.Rotation;
                    _campusManager.ConstructBuilding(buildingData, position, rotation, updateConnections: false);
                }
            }
        }

        /// <summary>
        /// Get all buildings.
        /// </summary>
        /// <returns>All buildings.</returns>
        public IEnumerable<BuildingInfo> GetBuildings()
        {
            return Utils.GetDistinct(_buildingAtGridPosition)
                .Select(building => building.ToBuildingInfo())
                .ToList();
        }

        /// <summary>
        /// Checks if a building exists at a given grid point.
        /// </summary>
        /// <param name="pos">Grid position to query.</param>
        /// <returns>True if a building exists at position, false otherwise.</returns>
        public bool BuildingAtGrid(Point2 pos)
        {
            return _buildingAtGridPosition[pos.x, pos.z] != null;
        }

        /// <summary>
        /// Returns a building info if it exists at a given grid point.
        /// </summary>
        /// <param name="pos">Grid position to query./</param>
        /// <returns>The building if it exists at the position, null otherwise.</returns>
        public BuildingInfo GetBuildingAtGrid(Point2 pos)
        {
            return _buildingAtGridPosition[pos.x, pos.z]?.ToBuildingInfo();
        }

        /// <summary>
        /// Build a building at the location.
        /// </summary>
        /// <param name="buildingData">The building to construct.</param>
        /// <param name="location">The location of the building.</param>
        /// <param name="rotation">The rotation of the building.</param>
        /// <returns>The points on the terrain that have been modified.</returns>
        public IEnumerable<Point2> ConstructBuilding(BuildingData buildingData, Point3 location, BuildingRotation rotation)
        {
            (Point3 buildingOrigin, Quaternion worldRotation) =
                BuildingRotationUtils.RotateBuilding(buildingData, location, rotation);

            Vector3 worldPosition = _terrain.Convert.GridToWorld(buildingOrigin);

            var building = CampusFactory.GenerateBuilding(
                        buildingData,
                        _campusManager.transform,
                        location,
                        rotation,
                        worldPosition + new Vector3(0f, 0.001f, 0f) /* Place just above the grass*/,
                        worldRotation);

            (Point3 footprintOrigin, bool[,] footprint) =
                BuildingRotationUtils.RotateFootprint(buildingData, location, rotation);

            int xSize = footprint.GetLength(0);
            int zSize = footprint.GetLength(1);
            for (int dx = 0; dx < xSize; ++dx)
            {
                for (int dz = 0; dz < zSize; ++dz)
                {
                    int gridX = footprintOrigin.x + dx;
                    int gridZ = footprintOrigin.z + dz;

                    if (footprint[dx, dz])
                    {
                        _buildingAtGridPosition[gridX, gridZ] = building;
                    }
                }
            }

            // Notify CampusManager of updated tiles
            for (int scanX = footprintOrigin.x - 1; scanX <= footprintOrigin.x + xSize; ++scanX)
                for (int scanZ = footprintOrigin.z - 1; scanZ <= footprintOrigin.z + zSize; ++scanZ)
                    if (_terrain.GridInBounds(scanX, scanZ))
                        yield return new Point2(scanX, scanZ);
        }

        /// <summary>
        /// Remove a building at a location.
        /// </summary>
        /// <param name="pos">The position to remove the building at.</param>
        /// <returns>The points on the terrain that have been modified.</returns>
        public IEnumerable<Point2> DestroyBuildingAt(Point2 pos)
        {
            Building building = _buildingAtGridPosition[pos.x, pos.z];
            if (building != null)
            {
                (Point3 footprintOrigin, bool[,] footprint) =
                    BuildingRotationUtils.RotateFootprint(building.Data, building.Location, building.Rotation);

                int xSize = footprint.GetLength(0);
                int zSize = footprint.GetLength(1);
                for (int dx = 0; dx < xSize; ++dx)
                {
                    for (int dz = 0; dz < zSize; ++dz)
                    {
                        int gridX = footprintOrigin.x + dx;
                        int gridZ = footprintOrigin.z + dz;

                        if (footprint[dx, dz])
                        {
                            _buildingAtGridPosition[gridX, gridZ] = null;
                        }
                    }
                }

                Object.Destroy(building.gameObject);

                // Return all the potentially modified grids around the building for updating.
                for (int scanX = footprintOrigin.x - 1; scanX <= footprintOrigin.x + xSize; ++scanX)
                    for (int scanZ = footprintOrigin.z - 1; scanZ <= footprintOrigin.z + zSize; ++scanZ)
                        if (_terrain.GridInBounds(scanX, scanZ))
                            yield return new Point2(scanX, scanZ);
            }
        }
    }
}
