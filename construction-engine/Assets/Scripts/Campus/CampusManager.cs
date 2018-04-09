﻿using Common;
using GameData;
using GridTerrain;
using System.Collections.Generic;

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
                GridSquareSize = 1.0f, // some values are hard-coded to keep consistent scale
                GridStepSize = 0.4f,
                CountX = gameData.Terrain.GridCountX,
                CountZ = gameData.Terrain.GridCountZ,
                CountY = gameData.Terrain.GridCountY,
                StartingHeight = gameData.Terrain.StartingHeight,
                Materials = GridMaterials.GetAll(),
            };

            GridMesh terrain;
            CampusFactory.GenerateTerrain(transform, gridMeshArgs, out terrain);

            Terrain = terrain;

            Game.State.RegisterController(GameState.SelectingTerrain, new SelectingTerrainController(terrain));
            Game.State.RegisterController(GameState.EditingTerrain, new EditingTerrainController(terrain));

            // Load the buildings
            foreach (var buildingData in gameData.Buildings)
            {
                _buildingRegistry[buildingData.Name] = buildingData;
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
