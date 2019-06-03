using Campus.GridTerrain;
using Common;
using GameData;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the PlacingContruction game state.
    /// </summary>
    internal class PlacingConstructionController : GameStateMachine.Controller
    {
        private GridMesh _terrain;

        private BuildingData _building;
        private FootprintCursor _cursors;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        /// <param name="terrain">The terrain to place construction on.</param>
        public PlacingConstructionController(GridMesh terrain)
        {
            _terrain = terrain;
            _building = null;
            _cursors = new FootprintCursor(
                terrain,
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_valid"),
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_invalid"));

            OnTerrainSelectionUpdate += PlacementUpdate;
            OnTerrainClicked += Build;
        }

        /// <summary>
        /// The state controller is starting.
        /// </summary>
        /// <param name="context">The construction to place.</param>
        public override void TransitionIn(object context)
        {
            _building = context as BuildingData;
            if (_building == null)
                GameLogger.FatalError("PlacingConstructionController was not given a building data!");

            _cursors.Create(_building.Footprint);
        }

        /// <summary>
        /// The state controller is stopped.
        /// </summary>
        public override void TransitionOut()
        {
            _cursors.Destroy();
        }

        /// <summary>
        /// Called each step of this state.
        /// </summary>
        public override void Update() { }

        /// <summary>
        /// Event handler for selection updates on the terrain.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="args">The terrain selection update args.</param>
        private void PlacementUpdate(object sender, TerrainSelectionUpdateArgs args)
        {
            if (args.SelectionLocation != Point3.Null)
            {
                _cursors.Place(args.SelectionLocation);
                _cursors.SetMaterials(GetValidTerrainUnderMouse());
            }
            else
            {
                _cursors.Deactivate();
            }
        }

        /// <summary>
        /// Event handler for clicks on the terrain.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="args">The terrain click arguments.</param>
        private void Build(object sender, TerrainClickedArgs args)
        {
            if (args.Button == MouseButton.Left)
            {
                var validTerrain = GetValidTerrainUnderMouse();
                var footprint = _building.Footprint;

                bool isValid = true;
                for (int x = 0; x < footprint.GetLength(0); ++x)
                    for (int z = 0; z < footprint.GetLength(1); ++z)
                        if (footprint[x, z] && !validTerrain[x, z])
                            isValid = false;

                if (isValid)
                {
                    Game.Campus.ConstructBuilding(_building, args.ClickLocation);
                    SelectionManager.UpdateSelection(SelectionManager.Selected.ToMainMenu());
                }
            }
        }

        /// <summary>
        /// Get a boolean array representing valid construction terrain underneath the cursor.
        /// </summary>
        /// <returns>A boolean array representing the valid terrain beneath the cursor.</returns>
        private bool[,] GetValidTerrainUnderMouse()
        {
            int footprintSizeX = _building.Footprint.GetLength(0);
            int footprintSizeZ = _building.Footprint.GetLength(1);
            return Game.Campus.CheckFlatAndFree(_cursors.Position.x, _cursors.Position.y, footprintSizeX, footprintSizeZ);
        }
    }
}
