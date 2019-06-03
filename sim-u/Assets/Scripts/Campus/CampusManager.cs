using Campus.GridTerrain;
using Common;
using GameData;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Campus
{
    /// <summary>
    /// The different ways a campus grid can be used.
    /// </summary>
    public enum CampusGridUse
    {
        Empty = 0,
        Path,
        Building,
    }

    /// <summary>
    /// Root level object for the campus.
    /// </summary>
    public class CampusManager : GameDataLoader<CampusData>
    {
        private Dictionary<string, BuildingData> _buildingRegistry = new Dictionary<string, BuildingData>();

        private GridMesh _terrain;
        private CampusBuildings _buildings;
        private CampusPaths _paths;

        /// <summary>
        /// Returns what is at the campus grid position.
        /// </summary>
        /// <param name="pos">Grid position to query.</param>
        /// <returns>The use of the current grid.</returns>
        public CampusGridUse GetGridUse(Point2 pos)
        {
            if (pos.x < 0 || pos.x >= _terrain.CountX ||
                pos.y < 0 || pos.y >= _terrain.CountZ)
            {
                return CampusGridUse.Empty;
            }

            CampusGridUse use = CampusGridUse.Empty;

            if (_buildings.BuildingAtPosition(pos))
            {
                Assert.AreEqual(CampusGridUse.Empty, use);
                use = CampusGridUse.Building;
            }

            if (_paths.PathAtPosition(pos))
            {
                Assert.AreEqual(CampusGridUse.Empty, use);
                use = CampusGridUse.Path;
            }

            return use;
        }

        /// <summary>
        /// Gets the metadata about the requested building.
        /// </summary>
        /// <param name="name">Name of the building.</param>
        /// <returns>The building data from config.</returns>
        public bool TryGetBuildingData(string name, out BuildingData buildingData)
        {
            return _buildingRegistry.TryGetValue(name, out buildingData);
        }

        /// <summary>
        /// Set the parent of the terrain object.
        /// In essence, this makes it so when you click the terrain,
        /// the parent selectable is not closed.
        /// </summary>
        /// <param name="parent">The selectable to keep open after terrain clicks.</param>
        public void SetTerrainSelectionParent(Selectable parent)
        {
            _terrain.Selectable.SelectionParent = parent;
        }

        /// <summary>
        /// Construct a building at the location.
        /// Registers the taken grid location with the Safe Terrain Editor.
        /// </summary>
        /// <param name="building">The building to construct.</param>
        /// <param name="location">The location of the building.</param>
        public void ConstructBuilding(BuildingData building, Point3 location)
        {
            _buildings.ConstructBuilding(building, location);
        }

        /// <summary>
        /// Build a path between two points.
        /// The path must be in a straight line.
        /// </summary>
        /// <param name="start">Starting location of the line.</param>
        /// <param name="end">Ending location of the line.</param>
        public void ConstructPath(Point3 start, Point3 end)
        {
            _paths.ConstructPath(start, end);
        }

        /// <summary>
        /// Destroys the campus improvement at the desired position.
        /// </summary>
        /// <param name="pos">The position to delete at.</param>
        public void DestroyAt(Point2 pos)
        {
            CampusGridUse itemAt = GetGridUse(pos);

            switch (itemAt)
            {
                case CampusGridUse.Path:
                    _paths.DestroyPathAt(pos);
                    break;

                case CampusGridUse.Building:
                    _buildings.DestroyBuildingAt(pos);
                    break;

                default:
                    GameLogger.Error("Could not destroy item '{0}' at {1}.", itemAt, pos);
                    break;
            }
        }

        /// <summary>
        /// Check if the terrain is valid for construction.
        /// i.e. flat and unanchored.
        /// </summary>
        /// <param name="xBase">Grid x position.</param>
        /// <param name="zBase">Grid z position.</param>
        /// <param name="xSize">Width of the area to check.</param>
        /// <param name="zSize">Height of the area to check.</param>
        /// <returns>Array of booleans representing valid or invalid squares.</returns>
        public bool[,] CheckFlatAndFree(int xBase, int zBase, int xSize, int zSize)
        {
            bool[,] check = new bool[xSize, zSize];

            for (int x = 0; x < xSize; ++x)
            {
                for (int z = 0; z < zSize; ++z)
                {
                    int gridX = xBase + x;
                    int gridZ = zBase + z;

                    // grid is valid if it is inside the terrain
                    // and not anchored
                    // and flat
                    check[x, z] =
                        gridX >= 0 &&
                        gridX < _terrain.CountX &&
                        gridZ >= 0 &&
                        gridZ < _terrain.CountZ &&
                        GetGridUse(new Point2(gridX, gridZ)) == CampusGridUse.Empty &&
                        _terrain.IsGridFlat(gridX, gridZ);
                }
            }

            return check;
        }

        /// <summary>
        /// Check if the terrain is valid for pathing.
        /// i.e. smooth and unanchored.
        /// </summary>
        /// <param name="xStart">Start x position.</param>
        /// <param name="zStart">Start z position.</param>
        /// <param name="xEnd">End x position.</param>
        /// <param name="zEnd">End z position.</param>
        /// <returns>Boolean array representing whether or not the square is smooth and free.</returns>
        public bool[] CheckLineSmoothAndFree(int xStart, int zStart, int xEnd, int zEnd)
        {
            if (xStart != xEnd && zStart != zEnd)
                throw new InvalidOperationException("Smooth line is not axis aligned.");

            if (xStart == xEnd && zStart == zEnd)
            {
                // Case: Checking a single square
                return new bool[] {
                    xStart >= 0 &&
                    xStart < _terrain.CountX &&
                    zStart >= 0 &&
                    zStart < _terrain.CountZ &&
                    GetGridUse(new Point2(xStart, zStart)) == CampusGridUse.Empty &&
                    (
                        (_terrain.GetVertexHeight(xStart, zStart) == _terrain.GetVertexHeight(xStart, zStart + 1) &&
                        _terrain.GetVertexHeight(xStart + 1, zStart) == _terrain.GetVertexHeight(xStart + 1, zStart + 1)) ||
                        (_terrain.GetVertexHeight(xStart, zStart) == _terrain.GetVertexHeight(xStart + 1, zStart) &&
                        _terrain.GetVertexHeight(xStart, zStart + 1) == _terrain.GetVertexHeight(xStart + 1, zStart + 1))
                    )
                };
            }
            else if (xStart != xEnd)
            {
                // Case: Checking a line along the x-axis
                int dx = xStart < xEnd ? 1 : -1;
                int length = Math.Abs(xStart - xEnd) + 1;
                bool[] isValid = new bool[length];

                for (int x = 0; x < length; ++x)
                {
                    int gridX = xStart + x * dx;
                    int gridZ = zStart;

                    isValid[x] =
                        gridX >= 0 &&
                        gridX < _terrain.CountX &&
                        gridZ >= 0 &&
                        gridZ < _terrain.CountZ &&
                        GetGridUse(new Point2(gridX, gridZ)) == CampusGridUse.Empty &&
                        _terrain.GetVertexHeight(gridX, gridZ) == _terrain.GetVertexHeight(gridX, gridZ + 1) &&
                        _terrain.GetVertexHeight(gridX + 1, gridZ) == _terrain.GetVertexHeight(gridX + 1, gridZ + 1);
                }

                return isValid;
            }
            else
            {
                // Case: Checking a line along the z-axis
                int dz = zStart < zEnd ? 1 : -1;
                int length = Math.Abs(zStart - zEnd) + 1;
                bool[] isValid = new bool[length];

                for (int z = 0; z < length; ++z)
                {
                    int gridX = xStart;
                    int gridZ = zStart + z * dz;

                    isValid[z] =
                        gridX >= 0 &&
                        gridX < _terrain.CountX &&
                        gridZ >= 0 &&
                        gridZ < _terrain.CountZ &&
                        GetGridUse(new Point2(gridX, gridZ)) == CampusGridUse.Empty &&
                        _terrain.GetVertexHeight(gridX, gridZ) == _terrain.GetVertexHeight(gridX + 1, gridZ) &&
                        _terrain.GetVertexHeight(gridX, gridZ + 1) == _terrain.GetVertexHeight(gridX + 1, gridZ + 1);
                }

                return isValid;
            }
        }

        /// <summary>
        /// Load the campus game data.
        /// </summary>
        /// <param name="gameData">Campus game data.</param>
        protected override void LoadData(CampusData gameData)
        {
            var gridMeshArgs = new GridMeshArgs()
            {
                GridSquareSize = Constant.GridSize,
                GridStepSize = Constant.GridStepSize,
                CountX = gameData.Terrain.GridCountX,
                CountZ = gameData.Terrain.GridCountZ,
                CountY = gameData.Terrain.GridCountY,
                StartingHeight = gameData.Terrain.StartingHeight,
                GridMaterial = gameData.Terrain.TerrainMaterial,
                SubmaterialSize = gameData.Terrain.SubmaterialSquareSize,
                SkirtPrefab = gameData.Terrain.TerrainSkirt,
            };

            GridMesh terrain;
            CampusFactory.GenerateTerrain(transform, gridMeshArgs, out terrain);

            _terrain = terrain;
            _buildings = new CampusBuildings(terrain);
            _paths = new CampusPaths(gameData, terrain);

            Game.State.RegisterController(GameState.SelectingTerrain, new SelectingTerrainController(terrain));
            Game.State.RegisterController(GameState.EditingTerrain, new EditingTerrainController(terrain));
            Game.State.RegisterController(GameState.PlacingConstruction, new PlacingConstructionController(terrain));
            Game.State.RegisterController(GameState.SelectingPath, new SelectingPathController(terrain));
            Game.State.RegisterController(GameState.PlacingPath, new PlacingPathController(terrain));
            Game.State.RegisterController(GameState.Demolishing, new DemolishingController(terrain));

            var footprintCreatorObject = new GameObject("FootprintCreator");
            using (var footprintCreator = footprintCreatorObject.AddComponent<FootprintCreator>())
            {
                // Load the buildings
                foreach (var buildingData in gameData.Buildings)
                {
                    buildingData.Footprint = footprintCreator.CalculateFootprint(buildingData.Mesh, Constant.GridSize);
                    _buildingRegistry[buildingData.Name] = buildingData;
                }
            }
        }

        /// <summary>
        /// Link the campus game data.
        /// </summary>
        /// <param name="gameData">Campus game data.</param>
        protected override void LinkData(CampusData gameData)
        {
            // nothing to link right now
        }
    }
}
