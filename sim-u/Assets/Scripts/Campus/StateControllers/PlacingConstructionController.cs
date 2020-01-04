using Campus.GridTerrain;
using Common;
using GameData;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the PlacingContruction game state.
    /// </summary>
    [StateController(HandledState = GameState.PlacingConstruction)]
    internal class PlacingConstructionController : GameStateMachine.Controller
    {
        private GridMesh _terrain;

        private BuildingData _building;
        private FootprintCursor _cursors;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        public PlacingConstructionController()
        {
            _terrain = Accessor.Terrain;
            _building = null;
            _cursors = new FootprintCursor(
                _terrain,
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_valid"),
                ResourceLoader.Load<Material>(ResourceType.Materials, ResourceCategory.Terrain, "cursor_invalid"));

            OnTerrainGridSelectionUpdate += PlacementUpdate;
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
        private void PlacementUpdate(object sender, TerrainGridUpdateArgs args)
        {
            if (args.GridSelection != Point3.Null)
            {
                Accessor.CampusManager.IsValidForBuilding(_building, args.GridSelection, out bool[,] validGrids);

                _cursors.Place(args.GridSelection);
                _cursors.SetMaterials(validGrids);
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
                if (Accessor.CampusManager.IsValidForBuilding(_building, args.GridSelection, out bool[,] _) &&
                    Accessor.Simulation.Purchase(_building.ConstructionCost))
                {
                    Accessor.CampusManager.ConstructBuilding(_building, args.GridSelection);
                    SelectionManager.UpdateSelection(SelectionManager.Selected.ToMainMenu());
                }
            }
        }
    }
}
