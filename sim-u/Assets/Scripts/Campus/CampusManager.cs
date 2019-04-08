using Campus.GridTerrain;
using Common;
using GameData;
using System.Collections.Generic;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Root level object for the campus.
    /// </summary>
    public class CampusManager : GameDataLoader<CampusData>
    {
        private Dictionary<string, BuildingData> _buildingRegistry = new Dictionary<string, BuildingData>();

        /// <summary>Terrain grid mesh.</summary>
        public GridMesh Terrain { get; private set; }

        /// <summary>Buildings on the campus.</summary>
        public CampusBuildings Buildings { get; private set; }

        /// <summary>Paths on the campus.</summary>
        public CampusPaths Paths { get; private set; }

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
                SkirtPrefab = gameData.Terrain.TerrainSkirt,
            };

            GridMesh terrain;
            CampusFactory.GenerateTerrain(transform, gridMeshArgs, out terrain);

            Terrain = terrain;
            Buildings = new CampusBuildings(terrain);
            Paths = new CampusPaths(terrain);

            Game.State.RegisterController(GameState.SelectingTerrain, new SelectingTerrainController(terrain));
            Game.State.RegisterController(GameState.EditingTerrain, new EditingTerrainController(terrain));
            Game.State.RegisterController(GameState.PlacingConstruction, new PlacingConstructionController(terrain));
            Game.State.RegisterController(GameState.SelectingPath, new SelectingPathController(terrain));
            Game.State.RegisterController(GameState.PlacingPath, new PlacingPathController(terrain));

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
